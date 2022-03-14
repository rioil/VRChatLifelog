using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRChatLogWathcer.Models
{
    public class JoinLeaveHistory
    {
        public JoinLeaveHistory(string playerName, DateTime joined, bool isLocal)
        {
            PlayerName = playerName;
            Joined = joined;
            IsLocal = isLocal;
        }

        [Key]
        public string PlayerName { get; set; }
        [Key]
        public DateTime Joined { get; set; }
        public DateTime? Left { get; set; }
        public bool IsLocal { get; set; }
    }
}
