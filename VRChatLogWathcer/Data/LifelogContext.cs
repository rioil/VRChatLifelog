using Microsoft.EntityFrameworkCore;
using System;
using System.IO;

namespace VRChatLogWathcer.Data
{
    internal class LifelogContext : DbContext
    {
        public DbSet<LocationHistory> LocationHistories { get; set; } = default!;
        public DbSet<JoinLeaveHistory> JoinLeaveHistories { get; set; } = default!;
        public DbSet<VRChatLogFileInfo> LogFiles { get; set; } = default!;

        public string DbPath { get; }

        public LifelogContext()
        {
            var appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            DbPath = Path.Join(appDataDir, "VRChatLifelog", "lifelog.db");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite($"Data Source={DbPath}");
    }
}
