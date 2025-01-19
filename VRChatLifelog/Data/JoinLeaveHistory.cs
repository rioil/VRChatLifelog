using System;

namespace VRChatLifelog.Data
{
    public class JoinLeaveHistory(string playerName, string playerId, DateTime joined, bool isLocal, int locationHistoryId)
    {
        public JoinLeaveHistory(string playerName, string playerId, DateTime joined, bool isLocal, LocationHistory location)
            : this(playerName, playerId, joined, isLocal, location.Id)
        {
        }

        public int Id { get; set; }
        public string PlayerName { get; set; } = playerName;
        public string? PlayerId { get; set; } = playerId;
        public DateTime Joined { get; set; } = joined;
        public DateTime? Left { get; set; }
        public bool IsLocal { get; set; } = isLocal;

        public int LocationHistoryId { get; set; } = locationHistoryId;
        public LocationHistory LocationHistory { get; set; } = default!;
    }
}
