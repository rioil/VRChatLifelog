using System.Xml.Serialization;

namespace VRChatLifelog.Models.SearchMs
{
    [XmlInclude(typeof(Include))]
    public abstract record ScopeElement();
}
