using System.Xml.Serialization;

namespace VRChatLogWathcer.Models.SearchMs
{
    public record LeafCondition : Condition
    {
        public LeafCondition()
        {
            PropertyType = string.Empty;
            Value = string.Empty;
            ValueType = string.Empty;
        }

        public LeafCondition(CanonicalProperty property, string propertyType, Operator @operator, string value, string valueType)
        {
            Property = property;
            PropertyType = propertyType;
            Type = ConditionType.LeafCondition;
            Operator = @operator;
            Value = value;
            ValueType = valueType;
        }

        [XmlAttribute("property")]
        public CanonicalProperty Property { get; init; }

        [XmlAttribute("propertyType")]
        public string PropertyType { get; init; }

        [XmlAttribute("operator")]
        public Operator Operator { get; init; }

        [XmlAttribute("value")]
        public string Value { get; init; }

        [XmlAttribute("valuetype")]
        public string ValueType { get; init; }
    }
}
