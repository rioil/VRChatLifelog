using System;

namespace VRChatLogWathcer.Models
{
    public class VRChatLogFileInfo
    {
        public VRChatLogFileInfo() { }

        public VRChatLogFileInfo(DateTime created)
        {
            Created = created;
        }

        public int Id { get; set; }
        public DateTime Created { get; set; }
        public DateTime? LastRead { get; set; }
    }
}
