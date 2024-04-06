using System.Xml.Serialization;

namespace VRChatLifelog.Models.SearchMs
{
    public enum ConditionType
    {
        [XmlEnum("andCondition")]
        AndCondition,

        [XmlEnum("orCondition")]
        OrCondition,

        [XmlEnum("notCondition")]
        NotCondition,

        [XmlEnum("leafCondition")]
        LeafCondition,
    }
}
