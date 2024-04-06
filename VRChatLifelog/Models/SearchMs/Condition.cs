using System.Xml.Serialization;

namespace VRChatLifelog.Models.SearchMs
{
    [XmlInclude(typeof(AndCondition))]
    [XmlInclude(typeof(OrCondition))]
    [XmlInclude(typeof(LeafCondition))]
    public abstract record Condition
    {
        protected Condition()
        {
        }

        protected Condition(ConditionType type)
        {
            Type = type;
        }

        [XmlAttribute("type")]
        public ConditionType Type { get; init; }
    }
}
