using System.Xml.Serialization;

namespace VRChatLifelog.Models.SearchMs
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
