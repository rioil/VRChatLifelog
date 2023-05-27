using System.Xml.Serialization;

namespace VRChatLogWathcer.Models.SearchMs
{
    public enum ViewMode
    {
        [XmlEnum("details")]
        Details,

        [XmlEnum("icons")]
        Icons,

        [XmlEnum("tiles")]
        Tiles,
    }
}
