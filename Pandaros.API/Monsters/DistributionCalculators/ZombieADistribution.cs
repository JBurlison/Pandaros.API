using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Pandaros.API.Monsters.DistributionCalculators
{
    public class ZombieADistribution : IMonsterDistributionCalculator
    {
        public string Name { get; set; } = nameof(ZombieADistribution);

        public Vector2 GetMonsterDistribution(Colony c)
        {
			if (c.FollowerCount < 10f)
			{
				return Vector2.Lerp(new Vector2(1f, 1f), new Vector2(0.9f, 1f), c.FollowerCount / 10f);
			}
			else if (c.FollowerCount < 30f)
			{
				return Vector2.Lerp(new Vector2(0.9f, 1f), new Vector2(0.7f, 0.9f), (c.FollowerCount - 10f) / 20f);
			}
			else if (c.FollowerCount < 50f)
			{
				return Vector2.Lerp(new Vector2(0.7f, 0.9f), new Vector2(0.5f, 0.8f), (c.FollowerCount - 30f) / 50f);
			}
			else if (c.FollowerCount < 100f)
			{
				return Vector2.Lerp(new Vector2(0.5f, 0.8f), new Vector2(0.25f, 0.75f), (c.FollowerCount - 50f) / 100f);
			}
			else if (c.FollowerCount < 150f)
			{
				return Vector2.Lerp(new Vector2(0.25f, 0.75f), new Vector2(0.2f, 0.5f), (c.FollowerCount - 100f) / 150f);
			}
			else if (c.FollowerCount < 200f)
			{
				return Vector2.Lerp(new Vector2(0.2f, 0.5f), new Vector2(0.1f, 0.3f), (c.FollowerCount - 150f) / 200f);
			}
			else
			{
				return new Vector2(0.1f, 0.3f);
			}
		}
    }
}
