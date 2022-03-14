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
    public class LogWathcer
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
        private CancellationTokenSource? _tokenSource;

        /// <summary>
        /// ログファイル名の正規表現パターン
        /// </summary>
        //lang=regex
        private readonly Regex LogFileNamePattern = new(@"output_log_\d{2}-\d{2}-\d{2}.txt");

        /// <summary>
        /// ログファイル名のフィルターパターン
        /// </summary>
        /// <remarks>正規表現が使用できない場合に使用します．</remarks>
        private const string LogFileNameFilter = @"output_log_*.txt";

        /// <summary>
        /// ログファイルのディレクトリを指定してインスタンスを作成します．
        /// </summary>
        /// <param name="vrchatLogDir">ログディレクトリのパス</param>
        public LogWathcer(string vrchatLogDir)
        {
            VRChatLogDirectory = vrchatLogDir;
        }

        /// <summary>
        /// VRChatのログディレクトリ
        /// </summary>
        public string VRChatLogDirectory { get; private set; }

        /// <summary>
        /// ログ監視中か
        /// </summary>
        public bool IsWatching { get; private set; }

        #region method
        /// <summary>
        /// ログディレクトリの監視とログファイルの読み取りを開始します．
        /// </summary>
        /// <returns></returns>
        public async Task Start()
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
        public async Task Stop()
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
            if (_watcher is not null)
            {
                StopLogDirectoryWatching();
            }

            // ログディレクトリのファイル変更監視
            _watcher = new FileSystemWatcher()
            {
                Path = VRChatLogDirectory,
                Filter = LogFileNameFilter,
                NotifyFilter = NotifyFilters.FileName,
                IncludeSubdirectories = false,
            };

            _watcher.Created += (s, e) => OnFileCreated(e.FullPath);
            //_watcher.Renamed += (s, e) => { System.Diagnostics.Debug.WriteLine($"{e.FullPath} renamed."); };

            _watcher.EnableRaisingEvents = true;
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
            if (LogFileNamePattern.IsMatch(fileFullPath))
            {
                await ChangeWatchingFile(fileFullPath);
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

            using var context = new LifelogContext();
            while (true)
            {
                if (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (string.IsNullOrEmpty(line))
                    {
                        if (LogItem.TryParse(buffer.ToArray(), out var item))
                        {
                            context.Add(item);
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
        /// <returns></returns>
        /// <exception cref="IOException"></exception>
        private void ReadLogFile(string path)
        {
            using var reader = new StreamReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
            var buffer = new List<string>();

            using var context = new LifelogContext();
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (string.IsNullOrEmpty(line))
                {
                    if (LogItem.TryParse(buffer.ToArray(), out var item))
                    {
                        context.Add(item);
                    }
                    buffer.Clear();
                }
                else
                {
                    buffer.Add(line);
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
            await StopLogFileWatching();

            _tokenSource = new CancellationTokenSource();
            _readLogFileTask = Task.Run(async () => await WatchLogFile(filePath, _tokenSource.Token));
        }

        /// <summary>
        /// ログファイルの内容監視を開始します．
        /// </summary>
        /// <returns></returns>
        private async Task StartLogFileWatching()
        {
            var logFiles = Directory.EnumerateFiles(VRChatLogDirectory, LogFileNameFilter)
                .Where(path => LogFileNamePattern.IsMatch(path))
                .OrderByDescending(path => File.GetCreationTime(path))
                .ToArray();
            if (!logFiles.Any()) { return; }

            var isVRChatRunning = IsVRChatRunning();

            if (isVRChatRunning)
            {
                // 既存のファイルを処理
                logFiles.Skip(1).ForEach(path => ReadLogFile(path));

                // 実行中のプロセスで使用されているファイルを処理
                var latestFile = logFiles.First();
                await ChangeWatchingFile(latestFile);
            }
            else
            {
                logFiles.ForEach(path => ReadLogFile(path));
            }
        }

        /// <summary>
        /// ログファイルの内容監視を停止します．
        /// </summary>
        /// <returns></returns>
        private async Task StopLogFileWatching()
        {
            _tokenSource?.Cancel();
            _tokenSource?.Dispose();
            _tokenSource = null;

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
}
