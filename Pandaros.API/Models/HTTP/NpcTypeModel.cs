using NPC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.API.Models.HTTP
{
    public class NPCTypeModel
    {
        public NPCType NPCType { get; set; }
        public INPCTypeSettings NPCTypeSettings { get; set; }
        public int NpcTypeId { get; set; }
    }
}
