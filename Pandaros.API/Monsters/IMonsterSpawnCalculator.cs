using NPC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Pandaros.API.Monsters
{
    public interface IMonsterSpawnCalculator
    {
        string Name { get; set; }
        bool ShouldSpawn(Colony c, Vector2 distribution, double spawnRoll);
        NPCType GetMonster(Colony c);
    }
}
