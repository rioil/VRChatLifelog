using System.Xml.Serialization;

namespace VRChatLogWathcer.Models.SearchMs
{
    [XmlInclude(typeof(Include))]
    public abstract record ScopeElement();
}
