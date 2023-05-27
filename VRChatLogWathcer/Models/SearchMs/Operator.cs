using System.Xml.Serialization;

namespace VRChatLogWathcer.Models.SearchMs
{
    public enum Operator
    {
        [XmlEnum("gte")]
        GreaterOrEqual,

        [XmlEnum("lte")]
        LessOrEqual,
    }
}
