using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VRChatLogWathcer.Extensions;

namespace VRChatLogWathcer.Models
{
    internal class VRChatLogWatcher
    {
        /// <summary>
        /// ログファイル情報ID
        /// </summary>
        private readonly int _logFileInfoId;

        /// <summary>
        /// ログ
        /// </summary>
        private readonly ILogger<VRChatLogWatcher> _logger;

        /// <summary>
        /// 現在のインスタンス
        /// </summary>
        private Instance _currentInstance = default!;           // 参照される時点ではnullでない値が入っているはず

        /// <summary>
        /// 現在のロケーション
        /// </summary>
        private LocationHistory _currentLocation = default!;

        /// <summary>
        /// VRChatのプロセス
        /// </summary>
        private Process? _vrchatProcess;

        /// <summary>
        /// ファイル読み取りタスク
        /// </summary>
        private Task? _watchTask;

        /// <summary>
        /// ファイル読み取りタスクキャンセルトークンソース
        /// </summary>
        private CancellationTokenSource? _watchTaskTcs;

        /// <summary>
        /// VRChatのプロセスが実行中か
        /// </summary>
        public bool IsProcessRunning => (!_vrchatProcess?.HasExited) ?? true;   // TODO プロセスを取得していないので常にtrue

        /// <summary>
        /// 監視中のファイルのフルパス
        /// </summary>
        public string WatchingFileFullPath { get; }

        public VRChatLogWatcher(string watchingFileFullPath, ILogger<VRChatLogWatcher> logger)
        {
            _logger = logger;

            WatchingFileFullPath = watchingFileFullPath;
            var created = File.GetCreationTime(watchingFileFullPath);
            using var dbContext = new LifelogContext();
            var fileInfo = dbContext.LogFiles.FirstOrDefault(x => x.Created == created);
            if (fileInfo is null)
            {
                fileInfo = new VRChatLogFileInfo(created);
                dbContext.LogFiles.Add(fileInfo);
                dbContext.SaveChanges();
            }
            _logFileInfoId = fileInfo.Id;
        }

        /// <summary>
        /// ファイルの読み取りを開始します．
        /// </summary>
        public void StartReading()
        {
            if (_watchTask is not null) { return; }
            _watchTaskTcs = new CancellationTokenSource();
            _watchTask = RunWatchTask(_watchTaskTcs.Token);
        }

        /// <summary>
        /// ファイルの読み取りを停止します．
        /// </summary>
        /// <returns></returns>
        public async Task StopReadingAsync()
        {
            if (_watchTask is null) { return; }
            _logger.LogInformation("Stopping reading {fileName}", WatchingFileFullPath);
            _watchTaskTcs?.Cancel();
            try
            {
                await _watchTask.ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is OperationCanceledException || ex is TaskCanceledException) { }
        }

        /// <summary>
        /// ログファイルを解析します．一度EOFに到達してもファイルが更新されるのを待機します．
        /// </summary>
        /// <param name="cancellationToken">キャンセルトークン</param>
        /// <returns></returns>
        /// <exception cref="IOException"></exception>
        /// <remarks>キャンセル要求が行われても，ファイルの末尾に到達するまで解析処理を続行します．</remarks>
        private async Task RunWatchTask(CancellationToken cancellationToken)
        {
            using var reader = await OpenLogFileAsync(WatchingFileFullPath, cancellationToken: cancellationToken).ConfigureAwait(false);
            var buffer = new List<string>();

            DateTime? lastRead = null;
            using (var dbContext = new LifelogContext())
            {
                var logFileInfo = dbContext.LogFiles.Single(x => x.Id == _logFileInfoId);
                lastRead = logFileInfo.LastRead;
                var currentFileLength = reader.BaseStream.Length;
                var lastWriteTime = File.GetLastWriteTime(WatchingFileFullPath);
                if (lastRead > lastWriteTime)
                {
                    reader.BaseStream.Seek(currentFileLength, SeekOrigin.Begin);
                }
            }

            while (true)
            {
                if (!reader.EndOfStream)
                {
                    lastRead = DateTime.Now;
                    var line = await reader.ReadLineAsync().ConfigureAwait(false);
                    if (string.IsNullOrEmpty(line))
                    {
                        if (LogItem.TryParse(buffer.ToArray(), out var item))
                        {
                            AddLogItem(item);
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
                else if (!IsProcessRunning)
                {
                    break;
                }
                else
                {
                    try
                    {
                        await Task.Delay(1000, cancellationToken).ConfigureAwait(false);
                    }
                    catch (TaskCanceledException) { }
                }
            }

            using (var dbContext = new LifelogContext())
            {
                var fileInfoId = dbContext.LogFiles.Single(x => x.Id == _logFileInfoId);
                fileInfoId.LastRead = lastRead;
                await dbContext.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);     // MEMO:最終読み取り時刻の保存はキャンセルしないのでキャンセルトークンは伝播させない
            }
        }

        /// <summary>
        /// VRChatのログファイルを開きます．
        /// </summary>
        /// <param name="path">ログファイルのパス</param>
        /// <param name="maxRetry">リトライ回数</param>
        /// <param name="retryIntervalMs">リトライ間隔(ms)</param>
        /// <returns>ログファイルの<see cref="StreamReader"/></returns>
        private static async Task<StreamReader> OpenLogFileAsync(string path, int maxRetry = 5, int retryIntervalMs = 500, CancellationToken cancellationToken = default)
        {
            int retryCount = 0;
            while (true)
            {
                try
                {
                    return new StreamReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                }
                catch (FileNotFoundException)
                {
                    if (retryCount >= maxRetry) { throw; }
                    retryCount++;
                }

                await Task.Delay(retryIntervalMs, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// ログ項目をDBの対応するテーブルに保存します．
        /// </summary>
        /// <param name="item">ログ項目</param>
        private void AddLogItem(LogItem item)
        {
            using var dbContext = new LifelogContext();

            // プレイヤーjoinログ
            if (VRChatLogUtil.TryParsePlayerJoinLog(item.Content, out var joinLog))
            {
                if (joinLog.IsLocal)
                {
                    // 既に項目が存在すれば追加しない TODO:処理済みのファイルを記録するなどして，もう少し良い方法を取りたい
                    var locationHistory = dbContext.LocationHistories.Where(h => h.WorldId == _currentInstance.WorldId && h.Joined == item.Time).FirstOrDefault();
                    if (locationHistory is null)
                    {
                        var logFileInfo = dbContext.LogFiles.First(x => x.Id == _logFileInfoId);
                        _currentLocation = new LocationHistory(_currentInstance, item.Time, logFileInfo);
                        dbContext.LocationHistories.Add(_currentLocation);
                        dbContext.SaveChanges();
                    }
                    else
                    {
                        _currentLocation = locationHistory;
                    }
                }
                else
                {
                    _currentLocation = dbContext.LocationHistories.First(x => x.Id == _currentLocation.Id);
                }

                // 既に項目が存在すれば追加しない TODO:処理済みのファイルを記録するなどして，もう少し良い方法を取りたい
                if (!dbContext.JoinLeaveHistories.Where(h => h.LocationHistory.Id == _currentLocation.Id && h.PlayerName == joinLog.PlayerName && h.Joined == item.Time).Any())
                {
                    dbContext.JoinLeaveHistories.Add(new JoinLeaveHistory(joinLog.PlayerName, item.Time, joinLog.IsLocal, _currentLocation));
                    dbContext.SaveChanges();
                }
            }
            // プレイヤーleaveログ
            else if (VRChatLogUtil.TryParsePlayerLeftLog(item.Content, out var leftLog))
            {
                // MEMO:ローカルテスト時は同名のプレイヤーが複数人存在することになるためSingleOrDefaultではなく，FirstOrDefaultを用いる
                var history = dbContext.JoinLeaveHistories.FirstOrDefault(h => h.LocationHistoryId == _currentLocation.Id && h.PlayerName == leftLog.PlayerName && h.Joined <= item.Time && h.Left == null);
                if (history is null) { return; }   // TODO 見つからないのは正常ではないのでログ出力等の対応が必要

                history.Left = item.Time;
                dbContext.JoinLeaveHistories.Update(history);

                if (history.IsLocal)
                {
                    var locHistory = dbContext.LocationHistories.SingleOrDefault(h => h.LogFileInfoId == _logFileInfoId && h.Joined <= item.Time && h.Left == null);
                    if (locHistory is not null)
                    {
                        locHistory.Left = item.Time;
                        dbContext.LocationHistories.Update(locHistory);
                    }
                }

                dbContext.SaveChanges();
            }
            // ワールドjoinログ（ワールドID取得）
            else if (VRChatLogUtil.TryParseWorldJoinLog(item.Content, out var instance))
            {
                _currentInstance = instance;
            }
            // ルームjoinログ（ワールド名取得）
            else if (VRChatLogUtil.TryParseRoomJoinLog(item.Content, out var roomJoinLog))
            {
                _currentInstance.WorldName = roomJoinLog.WorldName;
            }
        }

        // ブルースクリーン等で異常終了した場合のための自動修復処理
        // 退出時刻が記録されていない履歴があればファイルの最終更新日時を退出時刻として設定する
        private void RecoverCollapsedLog()
        {
            using var lifelogContext = new LifelogContext();
            var lastWriteTime = File.GetLastWriteTime(WatchingFileFullPath);
            lifelogContext.JoinLeaveHistories.Where(h => h.Joined <= lastWriteTime && h.Left == null).ForEach(h => RecoverCollapsedJoinLeaveHistory(h));
            lifelogContext.LocationHistories.Where(h => h.Joined <= lastWriteTime && h.Left == null).ForEach(h => RecoverCollapsedLocationHistory(h));
            lifelogContext.SaveChanges();

            // 破損した入退出履歴を修復
            void RecoverCollapsedJoinLeaveHistory(JoinLeaveHistory history)
            {
                history.Left = lastWriteTime;
                lifelogContext.JoinLeaveHistories.Update(history);
                _logger.LogWarning("Collapsed join/leave history (ID:{id}) was recovered.", history.Id);
            }

            // 破損したロケーション履歴を修復
            void RecoverCollapsedLocationHistory(LocationHistory history)
            {
                history.Left = lastWriteTime;
                lifelogContext.LocationHistories.Update(history);
                _logger.LogWarning("Collapsed location history (ID:{id}) was recovered.", history.Id);
            }
        }
    }
}
