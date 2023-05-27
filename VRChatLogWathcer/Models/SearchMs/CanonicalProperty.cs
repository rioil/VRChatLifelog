using System.Xml.Serialization;

namespace VRChatLogWathcer.Models.SearchMs
{
    public enum CanonicalProperty
    {
        /// <summary>
        /// デフォルト値
        /// </summary>
        [XmlEnum("")]
        Unknown,

        /// <summary>
        /// ファイルの作成日時
        /// </summary>
        [XmlEnum("System.DateCreated")]
        DateCreated,
    }
}
