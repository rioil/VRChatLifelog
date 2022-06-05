using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VRChatLogWathcer.Models
{
    internal class LifelogContext : DbContext
    {
        private readonly Regex PlayerJoinLogPattern = new("\\[Behaviour\\] Initialized PlayerAPI \"(?<player>.*)\" is (?<type>(remote)|(local))");

        private readonly Regex PlayerLeftLogPattern = new(@"\[Behaviour\] Unregistering (?<player>.*)");

        private readonly Regex WorldJoinLogPattern = new(@"\[Behaviour\] Joining (?<worldId>wr?ld_[a-z\d]{8}\-[a-z\d]{4}\-[a-z\d]{4}\-[a-z\d]{4}\-[a-z\d]{12}):(?<instanceId>\w+)(~(?<type>[\w]+)\((?<master>(usr_[a-z\d]{8}\-[a-z\d]{4}\-[a-z\d]{4}\-[a-z\d]{4}\-[a-z\d]{12})|\w{10})\))?(?<canReqInvite>\~canRequestInvite)?(~region\((?<region>[\w]+)\))?(~nonce\((.+)\))?");

        private readonly Regex RoomJoinLogPattern = new(@"\[Behaviour\] Joining or Creating Room: (?<name>.*)");

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
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
            DbPath = Path.Join(path, "VRChatLifelog", "lifelog.db");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite($"Data Source={DbPath}");

        /// <summary>
        /// ログ項目をDBの対応するテーブルに保存します．
        /// </summary>
        /// <param name="item">ログ項目</param>
        public void Add(LogItem item)
        {
            // TODO 主キーの変更，外部キー制約の追加に対応

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
