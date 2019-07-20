using NPC;
using Monsters;
using Pandaros.API.Entities;
using System;

namespace Pandaros.API.Monsters
{
    public class BossSpawnedEvent : EventArgs
    {
        public BossSpawnedEvent(ColonyState cs, IMonster boss)
        {
            Colony = cs;
            Boss   = boss;
        }

        public ColonyState Colony { get; }

        public IMonster Boss { get; }
    }
}