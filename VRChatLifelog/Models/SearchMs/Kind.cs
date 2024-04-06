using System.Xml.Serialization;

namespace VRChatLifelog.Models.SearchMs
{
    public record Kind
    {
        public Kind()
        {
        }

        public Kind(KindName name)
        {
            Name = name;
        }

        [XmlAttribute("name")]
        public KindName Name { get; init; }
    }
}
