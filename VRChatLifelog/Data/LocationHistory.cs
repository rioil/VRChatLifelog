using System;
using System.Collections.Generic;

namespace VRChatLifelog.Data
{
    public class LocationHistory
    {
        public int Id { get; set; }
        public string WorldId { get; set; }
        public string InstanceId { get; set; }
        public DateTime Joined { get; set; }
        public DateTime? Left { get; set; }
        public string WorldName { get; set; }
        public EInstanceType Type { get; set; }
        public ERegion Region { get; set; }
        public string? MasterId { get; set; }

        public int LogFileInfoId { get; set; }
        public VRChatLogFileInfo LogFileInfo { get; set; } = default!;

        public List<JoinLeaveHistory> JoinLeaveHistories { get; set; } = default!;

        public LocationHistory(string worldId, string instanceId, DateTime joined, string worldName, EInstanceType type, ERegion region, string? masterId, int logFileInfoId)
        {
            WorldId = worldId;
            InstanceId = instanceId;
            Joined = joined;
            WorldName = worldName;
            Type = type;
            Region = region;
            MasterId = masterId;
            LogFileInfoId = logFileInfoId;
        }

        public LocationHistory(string worldId, string instanceId, DateTime joined, string worldName, EInstanceType type, ERegion region, string? masterId, VRChatLogFileInfo logFileInfo)
            : this(worldId, instanceId, joined, worldName, type, region, masterId, logFileInfo.Id)
        {
            LogFileInfo = logFileInfo;
        }

        public LocationHistory(Instance instance, DateTime joined, VRChatLogFileInfo logFileInfo)
            : this(instance.WorldId, instance.InstanceId, joined, instance.WorldName, instance.Type, instance.Region, instance.MasterId, logFileInfo)
        {
        }
    }
}
