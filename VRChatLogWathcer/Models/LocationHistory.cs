using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRChatLogWathcer.Models
{
    public class LocationHistory
    {
        [Key]
        public Guid WorldId { get; set; }
        public string WorldName { get; set; }
        [Key]
        public DateTime Joined { get; set; }
        public DateTime? Left { get; set; }

        public LocationHistory(Guid worldId, string worldName, DateTime joined)
        {
            WorldId = worldId;
            WorldName = worldName;
            Joined = joined;
        }
    }
}
