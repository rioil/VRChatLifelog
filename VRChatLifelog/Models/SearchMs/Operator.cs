using System.Xml.Serialization;

namespace VRChatLifelog.Models.SearchMs
{
    public enum Operator
    {
        [XmlEnum("gte")]
        GreaterOrEqual,

        [XmlEnum("lte")]
        LessOrEqual,
    }
}
