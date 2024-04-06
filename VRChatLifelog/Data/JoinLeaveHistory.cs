using System;

namespace VRChatLifelog.Data
{
    public class JoinLeaveHistory
    {
        public JoinLeaveHistory(string playerName, DateTime joined, bool isLocal, LocationHistory location)
            : this(playerName, joined, isLocal, location.Id)
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
