using System.Xml.Serialization;

namespace VRChatLifelog.Models.SearchMs
{
    public enum KindName
    {
        [XmlEnum("calendar")]
        Calendar,

        [XmlEnum("communication")]
        Communication,

        [XmlEnum("contact")]
        Contact,

        [XmlEnum("document")]
        Document,

        [XmlEnum("email")]
        Email,

        [XmlEnum("feed")]
        Feed,

        [XmlEnum("folder")]
        Folder,

        [XmlEnum("game")]
        Game,

        [XmlEnum("instantmessage")]
        Instantmessage,

        [XmlEnum("journal")]
        Journal,

        [XmlEnum("link")]
        Link,

        [XmlEnum("movie")]
        Movie,

        [XmlEnum("music")]
        Music,

        [XmlEnum("note")]
        Note,

        [XmlEnum("picture")]
        Picture,

        [XmlEnum("program")]
        Program,

        [XmlEnum("recordedtv")]
        Recordedtv,

        [XmlEnum("searchfolder")]
        Searchfolder,

        [XmlEnum("task")]
        Task,

        [XmlEnum("video")]
        Video,

        [XmlEnum("webhistory")]
        Webhistory,

        [XmlEnum("item")]
        Item,

        [XmlEnum("other")]
        Other,
    }
}
