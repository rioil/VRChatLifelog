using System;
using VRChatLogWathcer.Models;

namespace VRChatLogWathcer.Data
{
    public class JoinLeaveHistory
    {
        public JoinLeaveHistory(PlayerJoinLog joinLog, LocationHistory location)
            : this(joinLog.PlayerName, joinLog.Time, joinLog.IsLocal, location.Id)
        {
        }

        public JoinLeaveHistory(string playerName, DateTime joined, bool isLocal, int locationHistoryId)
        {
            PlayerName = playerName;
            Joined = joined;
            IsLocal = isLocal;
            LocationHistoryId = locationHistoryId;
        }

        public int Id { get; set; }
        public string PlayerName { get; set; }
        public DateTime Joined { get; set; }
        public DateTime? Left { get; set; }
        public bool IsLocal { get; set; }

        public int LocationHistoryId { get; set; }
        public LocationHistory LocationHistory { get; set; } = default!;
    }
}
