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

        private readonly Regex WorldJoinLogPattern = new(@"\[Behaviour\] Joining wrld_(?<worldId>[a-z\d]{8}\-[a-z\d]{4}\-[a-z\d]{4}\-[a-z\d]{4}\-[a-z\d]{12}):(\d+)(~region\(([\w]+)\))?(~([\w]+)\(usr_([\w-]+)\)((\~canRequestInvite)?)(~region\(([\w].+)\))?~nonce\((.+)\))?");

        private readonly Regex RoomJoinLogPattern = new(@"\[Behaviour\] Joining or Creating Room: (?<name>.*)");

        private Guid _worldId;
        private string _worldName = string.Empty;

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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<LocationHistory>().HasKey(e => new { e.WorldId, e.Joined });
            modelBuilder.Entity<JoinLeaveHistory>().HasKey(e => new { e.PlayerName, e.Joined });
        }

        /// <summary>
        /// ログ項目をDBの対応するテーブルに保存します．
        /// </summary>
        /// <param name="item">ログ項目</param>
        public void Add(LogItem item)
        {
            // プレイヤーjoinログ
            var match = PlayerJoinLogPattern.Match(item.Content);
            if (match.Success)
            {
                var playerName = match.Groups["player"].Value;
                var isLocal = match.Groups["type"].Value == "local";
                if (isLocal)
                {
                    // 既に項目が存在すれば追加しない TODO:処理済みのファイルを記録するなどして，もう少し良い方法を取りたい
                    if (LocationHistories.Find(_worldId, item.Time) is null)
                    {
                        LocationHistories.Add(new LocationHistory(_worldId, _worldName, item.Time));
                    }
                }

                // 既に項目が存在すれば追加しない TODO:処理済みのファイルを記録するなどして，もう少し良い方法を取りたい
                if (JoinLeaveHistories.Find(playerName, item.Time) is null)
                {
                    JoinLeaveHistories.Add(new JoinLeaveHistory(playerName, item.Time, isLocal));
                }

                SaveChanges();
                return;
            }

            // プレイヤーleaveログ
            match = PlayerLeftLogPattern.Match(item.Content);
            if (match.Success)
            {
                var playerName = match.Groups["player"].Value;
                var history = JoinLeaveHistories.SingleOrDefault(h => h.PlayerName == playerName && h.Joined <= item.Time && h.Left == null);
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
                return;
            }

            // ワールドjoinログ（ワールドID取得）
            match = WorldJoinLogPattern.Match(item.Content);
            if (match.Success)
            {
                _worldId = Guid.Parse(match.Groups["worldId"].Value);
            }

            // ルームjoinログ（ワールド名取得）
            match = RoomJoinLogPattern.Match(item.Content);
            if (match.Success)
            {
                _worldName = match.Groups["name"].Value;
            }
        }
    }
}
