using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRChatLogWathcer.Models
{
    [Index(nameof(PlayerName), nameof(Joined), IsUnique = true)]
    public class JoinLeaveHistory
    {
        public JoinLeaveHistory(string playerName, DateTime joined, bool isLocal)
        {
            PlayerName = playerName;
            Joined = joined;
            IsLocal = isLocal;
        }

        public int Id { get; set; }
        public string PlayerName { get; set; }
        public DateTime Joined { get; set; }
        public DateTime? Left { get; set; }
        public bool IsLocal { get; set; }

        public int LocaionId { get; set; }
        //public LocationHistory Location { get; set; } = default!;
    }
}
