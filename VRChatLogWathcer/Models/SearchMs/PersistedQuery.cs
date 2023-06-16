using System.Xml.Serialization;

namespace VRChatLogWathcer.Models.SearchMs
{
    [XmlRoot("persistedQuery")]
    public record PersistedQuery
    {
        public PersistedQuery()
        {
            Version = string.Empty;
            ViewInfo = default!;
            Query = default!;
            Properties = default!;
        }

        public PersistedQuery(string version, ViewInfo viewInfo, Query query, Properties properties)
        {
            Version = version;
            ViewInfo = viewInfo;
            Query = query;
            Properties = properties;
        }

        [XmlAttribute("version")]
        public string Version { get; init; }

        [XmlElement("viewInfo")]
        public ViewInfo ViewInfo { get; init; }

        [XmlElement("query")]
        public Query Query { get; init; }

        [XmlElement("properties")]
        public Properties Properties { get; init; }
    }
}
