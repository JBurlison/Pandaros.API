using NPC;
using Pandaros.API.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NPC.NPCBase;

namespace Pandaros.API.Models.HTTP
{
    public class NPCModel
    {
        public NPCInventory NPCInventory { get; set; }
        public float Health { get; set; }
        public SerializableVector3 Position { get; set; }
        public SerializableVector3 GoalPosition { get; set; }
        public SerializableVector3 BedPosition { get; set; }
        public int Id { get; set; }
        public ushort NpcTypeJobId { get; set; }
        public ColonistInventory ColonistInventory { get; set; }
    }
}
