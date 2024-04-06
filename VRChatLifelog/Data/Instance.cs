namespace VRChatLifelog.Data
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

    /// <summary>
    /// インスタンスタイプ
    /// </summary>
    /// <remarks>DBに保存されるため，明示的に値を指定します．</remarks>
    public enum EInstanceType
    {
        Public = 0,
        FriendsPlus = 1,
        Friends = 2,
        InvitePlus = 3,
        Invite = 4,
        Group = 5,
        GroupPlus = 7,
        GroupPublic = 8,

        Unknown = 6,
    }

    /// <summary>
    /// サーバーの地域
    /// </summary>
    /// <remarks>DBに保存されるため，明示的に値を指定します．</remarks>
    public enum ERegion
    {
        USW = 0,
        USE = 1,
        EU = 2,
        JP = 3,
        Unknown = 4,
    }
}
