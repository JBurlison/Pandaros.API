using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.API.Models
{
    public class CSBlockJobSettings
    {
        public string blockType { get; set; }
        public int cooldown { get; set; }
        public string jobType { get; set; }
        public int maxCraftsPerHaul { get; set; }
        public string npcType { get; set; }
    }
}
