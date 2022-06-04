using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VRChatLogWathcer.Models
{
    public class LocationHistory
    {
        [Key]
        public int Id { get; set; }

        public string WorldId { get; set; }
        public string InstanceId { get; set; }
        public DateTime Joined { get; set; }
        public DateTime? Left { get; set; }
        public string WorldName { get; set; }
        public EInstanceType Type { get; set; }
        public ERegion Region { get; set; }
        public string? MasterId { get; set; }

        //public List<JoinLeaveHistory> JoinLeaveHistories { get; set; } = default!;

        public LocationHistory(string worldId, string instanceId, DateTime joined, string worldName, EInstanceType type, ERegion region, string? masterId)
        {
            WorldId = worldId;
            InstanceId = instanceId;
            Joined = joined;
            WorldName = worldName;
            Type = type;
            Region = region;
            MasterId = masterId;
        }

        public LocationHistory(Instance instance, DateTime joined)
            : this(instance.WorldId, instance.InstanceId, joined, instance.WorldName, instance.Type, instance.Region, instance.MasterId)
        {
        }
    }
}
