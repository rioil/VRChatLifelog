using System.Xml.Serialization;

namespace VRChatLifelog.Models.SearchMs
{
    public record Include : ScopeElement
    {
        public Include()
        {
        }

        public Include(string? path, string? knownFolder, bool nonRecursive)
        {
            Path = path;
            KnownFolder = knownFolder;
            NonRecursive = nonRecursive;
        }

        [XmlAttribute("path")]
        public string? Path { get; init; }
        
        [XmlAttribute("knownFolder")]
        public string? KnownFolder { get; init; }
        
        [XmlAttribute("nonRecursive")]
        public bool NonRecursive { get; init; }
    }
}
