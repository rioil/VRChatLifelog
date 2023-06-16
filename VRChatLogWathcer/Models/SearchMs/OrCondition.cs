using System.Collections.Generic;
using System.Xml.Serialization;

namespace VRChatLogWathcer.Models.SearchMs
{
    public record OrCondition : Condition
    {
        public OrCondition() : base(ConditionType.OrCondition)
        {
            Conditions = default!;
        }

        public OrCondition(List<Condition> conditions) : base(ConditionType.OrCondition)
        {
            Conditions = conditions;
        }

        [XmlElement("condition")]
        public List<Condition> Conditions { get; init; }
    }
}
