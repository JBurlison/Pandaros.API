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
    public class GetZombieACalculator : IMonsterSpawnCalculator
    {
        public static NPCType MonsterAA;
        public static NPCType MonsterAB;
        public static NPCType MonsterAC;

        public string Name { get; set; }

        public NPCType GetMonster(Colony c)
        {
            var calculator = PandaMonsterSpawner.GetDistributionCalculator(nameof(ZombieADistribution));

            Vector2 chances = calculator.GetMonsterDistribution(c);
            float rand = Pipliz.Random.NextFloat(0f, 1f);
            if (rand < chances.x)
            {
                return MonsterAA;
            }
            if (rand < chances.y)
            {
                return MonsterAB;
            }
            return MonsterAC;
        }

        public bool ShouldSpawn(Colony c, Vector2 distribution, double spawnRoll)
        {
            return spawnRoll < distribution.x;
        }
    }
}
