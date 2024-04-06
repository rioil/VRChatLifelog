using System.Collections.Generic;
using System.Xml.Serialization;

namespace VRChatLifelog.Models.SearchMs
{

    public record Query
    {
        public Query()
        {
            Scope = default!;
            KindList = default!;
            Conditions = default!;
        }

        public Query(Scope scope, List<Kind> kindList, List<Condition> conditions)
        {
            Scope = scope;
            KindList = kindList;
            Conditions = conditions;
        }

        [XmlElement("scope")]
        public Scope Scope { get; init; }

        [XmlArray("kindList")]
        [XmlArrayItem("kind")]
        public List<Kind> KindList { get; init; }

        [XmlArray("conditions")]
        [XmlArrayItem("condition")]
        public List<Condition> Conditions { get; init; }
    }
}
