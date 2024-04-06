using System.Collections.Generic;
using System.Xml.Serialization;

namespace VRChatLifelog.Models.SearchMs
{
    public record AndCondition : Condition
    {
        public AndCondition() : base(ConditionType.AndCondition)
        {
            Conditions = default!;
        }

        public AndCondition(List<Condition> conditions) : base(ConditionType.AndCondition)
        {
            Conditions = conditions;
        }

        [XmlElement("condition")]
        public List<Condition> Conditions { get; init; }
    }
}
