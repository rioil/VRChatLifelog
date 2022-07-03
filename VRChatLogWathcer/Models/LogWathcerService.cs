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

namespace VRChatLogWathcer.Models
{
    /// <summary>
    /// VRChatのログの監視を行います．
    /// </summary>
    internal class LogWathcerService : BackgroundService
    {
        /// <summary>
        /// ログディレクトリのファイル操作を監視するオブジェクト
        /// </summary>
        private FileSystemWatcher? _watcher;

        /// <summary>
        /// ログファイル内容監視タスク
        /// </summary>
        private Task? _readLogFileTask;

        /// <summary>
        /// ログファイル内容監視タスクのキャンセル用トークンソース
        /// </summary>
        private CancellationTokenSource? _logWatchCts;

        /// <summary>
        /// ログ監視設定
        /// </summary>
        private readonly LogWatchOption _settings;

        /// <summary>
        /// ライフログのDB Context
        /// </summary>
        private readonly LifelogContext _lifelogContext;

        /// <summary>
        /// logger
        /// </summary>
        private readonly ILogger<LogWathcerService> _logger;

        /// <summary>
        /// ログファイル名の正規表現パターン
        /// </summary>
        //lang=regex
        private readonly Regex LogFileNamePattern = new(@"^output_log_\d{2}-\d{2}-\d{2}.txt$");

        /// <summary>
        /// ログファイル名のフィルターパターン
        /// </summary>
        /// <remarks>正規表現が使用できない場合に使用します．</remarks>
        private const string LogFileNameFilter = @"output_log_*.txt";

        /// <summary>
        /// ログ監視中か
        /// </summary>
        public bool IsWatching { get; private set; }

        /// <summary>
        /// ログファイルのディレクトリを指定してインスタンスを作成します．
        /// </summary>
        /// <param name="settings">ログ監視設定</param>
        /// <param name="context">ライフログのDB Context</param>
        public LogWathcerService(IOptions<LogWatchOption> settings, LifelogContext context, ILogger<LogWathcerService> logger)
        {
            _settings = settings.Value;
            _lifelogContext = context;
            _logger = logger;
        }

        #region method
        /// <summary>
        /// ログディレクトリの監視とログファイルの読み取りを開始します．
        /// </summary>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (IsWatching) { return; }

            await StartLogFileWatching();
            StartLogDirectoryWatching();

            IsWatching = true;
        }

        /// <summary>
        /// ログディレクトリの監視とログファイルの読み取りを停止します．
        /// </summary>
        /// <returns></returns>
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (!IsWatching) { return; }

            StopLogDirectoryWatching();
            await StopLogFileWatching();

            IsWatching = false;
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

            _watcher.EnableRaisingEvents = false;
            _watcher = null;
        }

        /// <summary>
        /// 監視対象ディレクトリにファイルが作成された際の処理を行います．
        /// </summary>
        /// <param name="fileFullPath">作成されたファイルのフルパス</param>
        private async void OnFileCreated(string fileFullPath)
        {
            var fileName = Path.GetFileName(fileFullPath);

            if (LogFileNamePattern.IsMatch(fileName))
            {
                var absolutePath = Path.GetFullPath(fileFullPath);
                _logger.LogInformation("New log file created : {filePath}", absolutePath);
                await ChangeWatchingFile(absolutePath);
            }
        }

        /// <summary>
        /// ログファイルを解析します．一度EOFに到達してもファイルが更新されるのを待機します．
        /// </summary>
        /// <param name="path">ファイルパス</param>
        /// <param name="cancellationToken">キャンセルトークン</param>
        /// <returns></returns>
        /// <exception cref="IOException"></exception>
        /// <remarks>キャンセル要求が行われた場合，ファイルの末尾まで解析してから処理を終了します．</remarks>
        private async Task WatchLogFile(string path, CancellationToken cancellationToken)
        {
            using var reader = new StreamReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
            var buffer = new List<string>();

            while (true)
            {
                if (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (string.IsNullOrEmpty(line))
                    {
                        if (LogItem.TryParse(buffer.ToArray(), out var item))
                        {
                            _lifelogContext.Add(item);
                        }
                        buffer.Clear();
                    }
                    else
                    {
                        buffer.Add(line);
                    }
                }
                else if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                else
                {
                    try
                    {
                        await Task.Delay(1000, cancellationToken);
                    }
                    catch (TaskCanceledException) { }
                }
            }
        }

        /// <summary>
        /// ログファイルを解析します．EOFに到達すれば処理を終了します．
        /// </summary>
        /// <param name="path">ファイルパス</param>
        /// <exception cref="IOException"></exception>
        private void ReadLogFile(string path)
        {
            using var reader = new StreamReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
            var buffer = new List<string>();

            while (!reader.EndOfStream)
            {
                // ログの境目には空行が入るので空行で分割して解析
                var line = reader.ReadLine();
                if (string.IsNullOrEmpty(line))
                {
                    if (LogItem.TryParse(buffer.ToArray(), out var item))
                    {
                        _lifelogContext.Add(item);
                    }
                    buffer.Clear();
                }
                else
                {
                    buffer.Add(line);
                }
            }

            // ファイル終端まで読んだ後に自動修復を行う
            RecoverCollapsedLog();

            // ブルースクリーン等で異常終了した場合のための自動修復処理
            // 退出時刻が記録されていない履歴があればファイルの最終更新日時を退出時刻として設定する
            void RecoverCollapsedLog()
            {
                var lastWriteTime = File.GetLastWriteTime(path);
                _lifelogContext.JoinLeaveHistories.Where(h => h.Joined <= lastWriteTime && h.Left == null).ForEach(h => RecoverCollapsedJoinLeaveHistory(h));
                _lifelogContext.LocationHistories.Where(h => h.Joined <= lastWriteTime && h.Left == null).ForEach(h => RecoverCollapsedLocationHistory(h));

                // 破損した入退出履歴を修復
                void RecoverCollapsedJoinLeaveHistory(JoinLeaveHistory history)
                {
                    history.Left = lastWriteTime;
                    _logger.LogWarning("Collapsed join/leave history (ID:{id}) was recovered.", history.Id);
                }

                // 破損したロケーション履歴を修復
                void RecoverCollapsedLocationHistory(LocationHistory history)
                {
                    history.Left = lastWriteTime;
                    _logger.LogWarning("Collapsed location history (ID:{id}) was recovered.", history.Id);
                }
            }
        }

        /// <summary>
        /// 監視対象ログファイルを変更します．
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <returns></returns>
        private async Task ChangeWatchingFile(string filePath)
        {
            _logger.LogInformation("Watching file changed to {filePath}", filePath);

            await StopLogFileWatching();

            _logWatchCts = new CancellationTokenSource();
            _readLogFileTask = Task.Run(async () => await WatchLogFile(filePath, _logWatchCts.Token));
        }

        /// <summary>
        /// ログファイルの内容監視を開始します．
        /// </summary>
        /// <returns></returns>
        private async Task StartLogFileWatching()
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

            var isVRChatRunning = IsVRChatRunning();

            if (isVRChatRunning)
            {
                // 既存のファイルを処理
                _logger.LogInformation("VRChat is running now");
                _logger.LogInformation("Reading existing log files...");
                logFiles.SkipLast(1).ForEach(path => ReadLogFile(path));

                // 実行中のプロセスで使用されているファイルを処理
                var latestFile = logFiles.Last();
                await ChangeWatchingFile(latestFile);
            }
            else
            {
                _logger.LogInformation("Reading existing log files...");
                logFiles.ForEach(path => ReadLogFile(path));
                _logger.LogInformation("Reading existing log files completed");
            }
        }

        /// <summary>
        /// ログファイルの内容監視を停止します．
        /// </summary>
        /// <returns></returns>
        private async Task StopLogFileWatching()
        {
            _logWatchCts?.Cancel();
            _logWatchCts?.Dispose();
            _logWatchCts = null;

            if (_readLogFileTask is not null)
            {
                await _readLogFileTask.ConfigureAwait(false);
            }
        }

        /// <summary>
        /// VRChatを実行中かを判定します．
        /// </summary>
        /// <returns>VRChatを起動中であればtrue</returns>
        private bool IsVRChatRunning()
        {
            // TODO exeのパスを指定して厳密にチェックしたほうが良い
            return System.Diagnostics.Process.GetProcessesByName("VRChat").Any();
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
            = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"..\LocalLow\VRChat\VRChat\");
    }
}
