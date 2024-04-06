using System.Xml.Serialization;

namespace VRChatLifelog.Models.SearchMs
{
    public record ViewInfo
    {
        public ViewInfo()
        {
        }

        public ViewInfo(ViewMode viewMode, int iconSize)
        {
            ViewMode = viewMode;
            IconSize = iconSize;
        }

        [XmlAttribute("viewMode")]
        public ViewMode ViewMode { get; init; }

        [XmlAttribute("iconSize")]
        public int IconSize { get; init; }
    }
}
