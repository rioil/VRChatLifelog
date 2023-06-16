using System.Collections.Generic;
using System.Xml.Serialization;

namespace VRChatLogWathcer.Models.SearchMs
{
    public record Scope
    {
        public Scope()
        {
            Elements = default!;
        }

        public Scope(List<ScopeElement> elements)
        {
            Elements = elements;
        }

        [XmlElement("include", typeof(Include))]
        public List<ScopeElement> Elements { get; init; }
    }
}
