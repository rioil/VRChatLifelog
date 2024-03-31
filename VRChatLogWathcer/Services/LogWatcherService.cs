using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using VRChatLogWathcer.Extensions;
using VRChatLogWathcer.Models;

namespace VRChatLogWathcer.Services
{
    /// <summary>
    /// VRChatのログの監視を行います．
    /// </summary>
    internal class LogWatcherService : BackgroundService
    {
        /// <summary>
        /// ログファイル名の正規表現パターン
        /// </summary>
        private readonly Regex LogFileNamePattern = new(@"^output_log_(\d{4}-\d{2}-\d{2}_)?\d{2}-\d{2}-\d{2}.txt$");

        /// <summary>
        /// ログファイル名のフィルターパターン
        /// </summary>
        /// <remarks>正規表現が使用できない場合に使用します．</remarks>
        private const string LogFileNameFilter = @"output_log_*.txt";

        /// <summary>
        /// ログディレクトリのファイル操作を監視するオブジェクト
        /// </summary>
        private FileSystemWatcher? _watcher;

        /// <summary>
        /// ログ監視設定
        /// </summary>
        private readonly LogWatchOption _settings;

        /// <summary>
        /// logger
        /// </summary>
        private readonly ILogger<LogWatcherService> _logger;

        /// <summary>
        /// Service Provider
        /// </summary>
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// 監視中のログファイル
        /// </summary>
        private readonly List<VRChatLogWatcher> _watchingFiles = new(1);

        /// <summary>
        /// 監視対象ファイル数変更時に発生するイベント
        /// </summary>
        public event WatchingFileCountChangedEventHandler? WatchingFileCountChanged;

        /// <summary>
        /// 監視中のログファイルの数
        /// </summary>
        public int WatchingFileCount => _watchingFiles.Count;

        /// <summary>
        /// ログファイルのディレクトリを指定してインスタンスを作成します．
        /// </summary>
        /// <param name="settings">ログ監視設定</param>
        /// <param name="serviceProvider"></param>
        public LogWatcherService(IOptions<LogWatchOption> settings, IServiceProvider serviceProvider)
        {
            _settings = settings.Value;
            _serviceProvider = serviceProvider;
            _logger = serviceProvider.GetRequiredService<ILogger<LogWatcherService>>();
        }

        #region method
        /// <summary>
        /// ログディレクトリの監視とログファイルの読み取りを開始します．
        /// </summary>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            StartLogFileWatching();
            StartLogDirectoryWatching();

            try
            {
                await Task.Delay(Timeout.Infinite, stoppingToken).ConfigureAwait(false);
            }
            catch (TaskCanceledException) { }

            _logger.LogInformation("Stopping log watching...");
            StopLogDirectoryWatching();
            await StopLogFileWatchingAsync().ConfigureAwait(false);
        }
        #endregion method

        #region private method
        /// <summary>
        /// ログファイルディレクトリのファイル変更監視を開始します．
        /// </summary>
        private void StartLogDirectoryWatching()
        {
            _logger.LogInformation("Starting log directory watching...");

            if (_watcher is not null)
            {
                StopLogDirectoryWatching();
            }

            // ログディレクトリのファイル変更監視
            _watcher = new FileSystemWatcher()
            {
                Path = _settings.VRChatLogDirectory,
                Filter = LogFileNameFilter,
                NotifyFilter = NotifyFilters.FileName,
                IncludeSubdirectories = false,
            };

            _watcher.Created += (s, e) => OnFileCreated(e.FullPath);
            //_watcher.Renamed += (s, e) => { System.Diagnostics.Debug.WriteLine($"{e.FullPath} renamed"); };

            _watcher.EnableRaisingEvents = true;

            _logger.LogInformation("Log directory watching started");
        }

        /// <summary>
        /// ログファイルディレクトリのファイル変更監視を停止します．
        /// </summary>
        private void StopLogDirectoryWatching()
        {
            if (_watcher is null) { return; }
            _logger.LogInformation("Stopping log directory watching...");

            _watcher.EnableRaisingEvents = false;
            _watcher = null;
        }

        /// <summary>
        /// ログファイルの内容監視を開始します．
        /// </summary>
        /// <returns></returns>
        private void StartLogFileWatching()
        {
            _logger.LogInformation("Starting log file watching...");

            var logFiles = Directory.EnumerateFiles(_settings.VRChatLogDirectory, LogFileNameFilter)
                .Where(path => LogFileNamePattern.IsMatch(Path.GetFileName(path)))
                .OrderBy(path => File.GetCreationTime(path))
                .ToArray();
            if (!logFiles.Any())
            {
                _logger.LogInformation("No log file found");
                return;
            }

            _logger.LogInformation("Reading existing log files...");
            logFiles.ForEach(path => StartReadingFile(path));
            _logger.LogInformation("Log file watching started");
        }

        /// <summary>
        /// ログファイルの内容監視を停止します．
        /// </summary>
        /// <returns></returns>
        private async Task StopLogFileWatchingAsync()
        {
            _logger.LogInformation("Stopping log file watching...");
            foreach (var logFile in _watchingFiles)
            {
                await logFile.StopReadingAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 監視対象ディレクトリにファイルが作成された際の処理を行います．
        /// </summary>
        /// <param name="fileFullPath">作成されたファイルのフルパス</param>
        private void OnFileCreated(string fileFullPath)
        {
            var fileName = Path.GetFileName(fileFullPath);

            if (LogFileNamePattern.IsMatch(fileName))
            {
                _logger.LogInformation("New log file created : {filePath}", fileFullPath);
                StartReadingFile(fileFullPath);
                WatchingFileCountChanged?.Invoke(this, new WatchingFileCountChangedEventArgs(_watchingFiles.Count));
            }
        }

        private void StartReadingFile(string filePath)
        {
            try
            {
                var logFile = new VRChatLogWatcher(filePath, _serviceProvider.GetRequiredService<ILogger<VRChatLogWatcher>>());
                logFile.StartReading();
                _watchingFiles.Add(logFile);
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogWarning("Log file not found : {filePath}", ex.FileName);
            }
        }
        #endregion private method
    }

    public class LogWatchOption
    {
        /// <summary>
        /// VRChatのログ出力先ディレクトリ
        /// </summary>
        /// <remarks>
        /// デフォルト値は
        /// <see href="https://docs.vrchat.com/docs/frequently-asked-questions#how-do-i-find-the-vrchat-output-logs">公式ページのFAQ</see>
        /// に従って設定
        /// </remarks>
        public string VRChatLogDirectory { get; set; }
            = Path.GetFullPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"..\LocalLow\VRChat\VRChat\"));
    }

    public class WatchingFileCountChangedEventArgs : EventArgs
    {
        public int Count { get; set; }

        public WatchingFileCountChangedEventArgs(int count)
        {
            Count = count;
        }
    }
    public delegate void WatchingFileCountChangedEventHandler(object sender, WatchingFileCountChangedEventArgs args);
}
