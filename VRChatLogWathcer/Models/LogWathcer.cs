using Livet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VRChatLogWathcer.Models.VRCLog;

namespace VRChatLogWathcer.Models
{
    public class LogWathcer : NotificationObject
    {
        private FileSystemWatcher? _watcher;

        public LogWathcer(string vrchatLogDir)
        {
            VRChatLogDirectory = vrchatLogDir;
        }

        /// <summary>
        /// VRChatのログディレクトリ
        /// </summary>
        public string VRChatLogDirectory { get; private set; }

        private Task? _readLogFileTask;

        List<JoinLeaveHistory> _joinLeaveHistories = new();
        List<LocationHistory> _locationHistories = new();
        Guid _worldId;

        public void Start()
        {
            if (_watcher is object) { return; }

            _watcher = new FileSystemWatcher()
            {
                Path = VRChatLogDirectory,
                Filter = "*.txt",
                NotifyFilter = NotifyFilters.FileName,
                IncludeSubdirectories = false,
            };

            _watcher.Changed += (s, e) => { System.Diagnostics.Debug.WriteLine($"{e.FullPath} changed."); };
            _watcher.Created += (s, e) => { System.Diagnostics.Debug.WriteLine($"{e.FullPath} created."); };
            _watcher.Deleted += (s, e) => { System.Diagnostics.Debug.WriteLine($"{e.FullPath} deleted."); };
            _watcher.Renamed += (s, e) => { System.Diagnostics.Debug.WriteLine($"{e.FullPath} renamed."); };

            _watcher.EnableRaisingEvents = true;

            // 最新ファイルの内容監視
            // MEMO:ちゃんと読めてる
            var latest = Directory.EnumerateFiles(VRChatLogDirectory, "*.txt")
                .OrderByDescending(path => File.GetCreationTime(path))
                .FirstOrDefault();

            if (latest is null) { return; }
            _readLogFileTask = Task.Run(() => ReadLogFile(latest));
        }

        public void Stop()
        {
            if (_watcher is null) { return; }

            _watcher.EnableRaisingEvents = false;
            _watcher = null;
        }

        private async Task ReadLogFile(string path)
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
                            ProcessLogItem(item);
                        }
                        buffer.Clear();
                    }
                    else
                    {
                        buffer.Add(line);
                    }
                }
                else
                {
                    await Task.Delay(1000);
                }
            }
        }

        private void ProcessLogItem(LogItem item)
        {
            // TODO ログアイテムをDBに保存するとか
            //lang=regex
            const string PlayerJoinLogPattern = "\\[Behaviour\\] Initialized PlayerAPI \"(?<player>.*)\" is (?<type>(remote)|(local))";
            //lang=regex
            const string PlayerLeftLogPattern = @"\[Behaviour\] Unregistering (?<player>.*)";
            //lang=regex
            const string WorldJoinLogPattern = @"\[Behaviour\] Joining wrld_(?<worldId>[a-z\d]{8}\-[a-z\d]{4}\-[a-z\d]{4}\-[a-z\d]{4}\-[a-z\d]{12}):(\d+)(~region\(([\w]+)\))?(~([\w]+)\(usr_([\w-]+)\)((\~canRequestInvite)?)(~region\(([\w].+)\))?~nonce\((.+)\))?";

            // プレイヤーjoinログ
            var match = Regex.Match(item.Content, PlayerJoinLogPattern);
            if (match.Success)
            {
                var playerName = match.Groups["player"].Value;
                var isLocal = match.Groups["type"].Value == "local";
                if (isLocal)
                {
                    _locationHistories.Add(new LocationHistory(_worldId, item.Time));
                }
                _joinLeaveHistories.Add(new JoinLeaveHistory(playerName, item.Time, isLocal));
                return;
            }

            // プレイヤーleaveログ
            match = Regex.Match(item.Content, PlayerLeftLogPattern);
            if (match.Success)
            {
                var playerName = match.Groups["player"].Value;
                var history = _joinLeaveHistories.Find(h => h.PlayerName == playerName && h.Left is null);
                if (history is null) { return; }

                history.Left = item.Time;
                if (history.IsLocal)
                {
                    var locHistory = _locationHistories.Find(h => h.Left is null);
                    if (locHistory is not null)
                    {
                        locHistory.Left = item.Time;
                    }
                }
                return;
            }

            // ワールドjoinログ
            match = Regex.Match(item.Content, WorldJoinLogPattern);
            if (match.Success)
            {
                var worldId = Guid.Parse(match.Groups["worldId"].Value);
                _worldId = worldId;
            }
        }
    }

    internal class JoinLeaveHistory
    {
        public JoinLeaveHistory(string playerName, DateTime joined, bool isLocal)
        {
            PlayerName = playerName;
            Joined = joined;
            IsLocal = isLocal;
        }

        public string PlayerName { get; set; }
        public DateTime Joined { get; set; }
        public DateTime? Left { get; set; }
        public bool IsLocal { get; set; }

        public override string ToString()
        {
            return $"{PlayerName} : {Joined}~{Left}";
        }
    }

    internal class LocationHistory
    {
        public LocationHistory(Guid worldId, DateTime joined)
        {
            WorldId = worldId;
            Joined = joined;
        }

        public Guid WorldId { get; set; }
        public DateTime Joined { get; set; }
        public DateTime? Left { get; set; }

        public override string ToString()
        {
            return $"@{WorldId} : {Joined}~{Left}";
        }
    }
}
