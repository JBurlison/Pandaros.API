using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Pandaros.API.Monsters.DistributionCalculators
{
    public class ZombieBDistribution : IMonsterDistributionCalculator
    {
        public Vector2 GetMonsterDistribution(Colony c)
        {
            if (c.FollowerCount < 35f)
            {
                return new Vector2(1f, 1f);
            }
            else if (c.FollowerCount < 150f)
            {
                return Vector2.Lerp(new Vector2(1f, 1f), new Vector2(0.25f, 0.75f), (c.FollowerCount - 35f) / 115f);
            }
            else if (c.FollowerCount < 500f)
            {
                return Vector2.Lerp(new Vector2(0.25f, 0.75f), new Vector2(0.1f, 0.3f), (c.FollowerCount - 150f) / 350f);
            }
            else
            {
                return new Vector2(0.1f, 0.3f);
            }
        }
    }
}
