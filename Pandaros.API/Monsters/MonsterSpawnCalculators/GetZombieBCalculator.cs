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
    public class GetZombieBCalculator : IMonsterSpawnCalculator
    {

        public static NPCType MonsterBA;
        public static NPCType MonsterBB;
        public static NPCType MonsterBC;

        public string Name { get; set; }

        public NPCType GetMonster(Colony c)
        {
            var calculator = PandaMonsterSpawner.GetDistributionCalculator(nameof(ZombieBDistribution));

            Vector2 chances = calculator.GetMonsterDistribution(c);
            float rand = Pipliz.Random.NextFloat(0f, 1f);
            if (rand < chances.x)
            {
                return MonsterBA;
            }
            if (rand < chances.y)
            {
                return MonsterBB;
            }
            return MonsterBC;
        }

        public bool ShouldSpawn(Colony c, Vector2 distribution, double spawnRoll)
        {
            return spawnRoll < distribution.y;
        }
    }
}
