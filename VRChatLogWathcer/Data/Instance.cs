using System;

namespace VRChatLogWathcer.Data
{
    public class Instance
    {
        public Instance(string worldId, string instanceId, EInstanceType instanceType, ERegion region, string? masterId)
        {
            WorldId = worldId;
            InstanceId = instanceId;
            Type = instanceType;
            Region = region;
            MasterId = masterId;
        }

        public string WorldId { get; set; }
        public string InstanceId { get; set; }
        public string WorldName { get; set; } = string.Empty;
        public EInstanceType Type { get; set; }
        public ERegion Region { get; set; }
        public string? MasterId { get; set; }
    }

    public enum EInstanceType
    {
        Public,
        FriendsPlus,
        Friends,
        InvitePlus,
        Invite,
        Group,
        Unknown,
    }

    public enum ERegion
    {
        USW,
        USE,
        EU,
        JP,
        Unknown,
    }
}
