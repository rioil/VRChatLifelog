using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;

namespace VRChatLogWathcer.Models
{
    internal class LifelogContext : DbContext
    {
        /// <summary>
        /// 現在のインスタンス
        /// </summary>
        private Instance _currentInstance = default!;           // 参照される時点ではnullでない値が入っているはず

        /// <summary>
        /// 現在のロケーション
        /// </summary>
        private LocationHistory _currentLocation = default!;

        public DbSet<LocationHistory> LocationHistories { get; set; } = default!;
        public DbSet<JoinLeaveHistory> JoinLeaveHistories { get; set; } = default!;

        public string DbPath { get; }

        public LifelogContext()
        {
            var appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            DbPath = Path.Join(appDataDir, "VRChatLifelog", "lifelog.db");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite($"Data Source={DbPath}");

        /// <summary>
        /// ログ項目をDBの対応するテーブルに保存します．
        /// </summary>
        /// <param name="item">ログ項目</param>
        public void Add(LogItem item)
        {
            // プレイヤーjoinログ
            if (VRChatLogUtil.TryParsePlayerJoinLog(item.Content, out var joinLog))
            {
                if (joinLog.IsLocal)
                {
                    // 既に項目が存在すれば追加しない TODO:処理済みのファイルを記録するなどして，もう少し良い方法を取りたい
                    var locationHistory = LocationHistories.Where(h => h.WorldId == _currentInstance.WorldId && h.Joined == item.Time).FirstOrDefault();
                    if (locationHistory is null)
                    {
                        _currentLocation = new LocationHistory(_currentInstance, item.Time);
                        LocationHistories.Add(_currentLocation);
                        SaveChanges();
                    }
                    else
                    {
                        _currentLocation = locationHistory;
                    }
                }

                // 既に項目が存在すれば追加しない TODO:処理済みのファイルを記録するなどして，もう少し良い方法を取りたい
                if (!JoinLeaveHistories.Where(h => h.LocationHistory == _currentLocation && h.PlayerName == joinLog.PlayerName && h.Joined == item.Time).Any())
                {
                    JoinLeaveHistories.Add(new JoinLeaveHistory(joinLog.PlayerName, item.Time, joinLog.IsLocal, _currentLocation));
                    SaveChanges();
                }
            }
            // プレイヤーleaveログ
            else if (VRChatLogUtil.TryParsePlayerLeftLog(item.Content, out var leftLog))
            {
                var history = JoinLeaveHistories.SingleOrDefault(h => h.PlayerName == leftLog.PlayerName && h.Joined <= item.Time && h.Left == null);
                if (history is null) { return; }

                history.Left = item.Time;
                JoinLeaveHistories.Update(history);

                if (history.IsLocal)
                {
                    var locHistory = LocationHistories.SingleOrDefault(h => h.Joined <= item.Time && h.Left == null);
                    if (locHistory is not null)
                    {
                        locHistory.Left = item.Time;
                        LocationHistories.Update(locHistory);
                    }
                }

                SaveChanges();
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
    }
}
