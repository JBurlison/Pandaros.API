using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.API.Models
{
    public class CSBlockJobSettings
    {
        public virtual string blockType { get; set; }
        public virtual int cooldown { get; set; }
        public virtual string jobType { get; set; }
        public virtual int maxCraftsPerHaul { get; set; }
        public virtual string npcType { get; set; }
    }
}
