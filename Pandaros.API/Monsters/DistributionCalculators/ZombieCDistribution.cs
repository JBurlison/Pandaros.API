using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Pandaros.API.Monsters.DistributionCalculators
{
    public class ZombieCDistribution : IMonsterDistributionCalculator
    {
        public Vector2 GetMonsterDistribution(Colony c)
        {
            if (c.FollowerCount < 80f)
            {
                return new Vector2(1f, 1f);
            }
            else if (c.FollowerCount < 400f)
            {
                return Vector2.Lerp(new Vector2(1f, 1f), new Vector2(0.25f, 0.75f), (c.FollowerCount - 80f) / 320f);
            }
            else if (c.FollowerCount < 1000f)
            {
                return Vector2.Lerp(new Vector2(0.25f, 0.75f), new Vector2(0.1f, 0.3f), (c.FollowerCount - 400f) / 600f);
            }
            else
            {
                return new Vector2(0.1f, 0.3f);
            }
        }
    }
}
