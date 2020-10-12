using NPC;
using Pandaros.API.Monsters.DistributionCalculators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Pandaros.API.Monsters.MonsterSpawnCalculators
{
    public class GetZombieCCalculator : IMonsterSpawnCalculator
    {
        public static NPCType MonsterCA;
        public static NPCType MonsterCB;
        public static NPCType MonsterCC;

        public string Name { get; set; }

        public NPCType GetMonster(Colony c)
        {
            var calculator = PandaMonsterSpawner.GetDistributionCalculator(nameof(ZombieCDistribution));

            Vector2 chances = calculator.GetMonsterDistribution(c);
            float rand = Pipliz.Random.NextFloat(0f, 1f);
            if (rand < chances.x)
            {
                return MonsterCA;
            }
            if (rand < chances.y)
            {
                return MonsterCB;
            }
            return MonsterCC;
        }

        public bool ShouldSpawn(Colony c, Vector2 distribution, double spawnRoll)
        {
            return true;
        }
    }
}
