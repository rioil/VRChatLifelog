using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRChatLogWathcer.Models
{
    internal class LocationHistory
    {
        [Key]
        public Guid WorldId { get; set; }
        [Key]
        public DateTime Joined { get; set; }
        public DateTime? Left { get; set; }

        public LocationHistory(Guid worldId, DateTime joined)
        {
            WorldId = worldId;
            Joined = joined;
        }

        public override string ToString()
        {
            return $"@{WorldId} : {Joined}~{Left}";
        }
    }
}
