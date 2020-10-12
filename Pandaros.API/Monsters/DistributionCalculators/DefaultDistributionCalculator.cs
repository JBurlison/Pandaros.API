using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Pandaros.API.Monsters.DistributionCalculators
{
    public class DefaultDistributionCalculator : IMonsterDistributionCalculator
    {
        public Vector2 GetMonsterDistribution(Colony c)
        {
			if (c.FollowerCount < 35f)
			{
				return Vector2.Lerp(new Vector2(1f, 1f), new Vector2(0.95f, 1f), c.FollowerCount / 35f);
			}
			if (c.FollowerCount < 50f)
			{
				return Vector2.Lerp(new Vector2(0.95f, 1f), new Vector2(0.85f, 1f), (c.FollowerCount - 35f) / 15f);
			}
			if (c.FollowerCount < 80f)
			{
				return Vector2.Lerp(new Vector2(0.85f, 1f), new Vector2(0.75f, 0.95f), (c.FollowerCount - 50f) / 30f);
			}
			if (c.FollowerCount < 120f)
			{
				return Vector2.Lerp(new Vector2(0.75f, 0.95f), new Vector2(0.5f, 0.8f), (c.FollowerCount - 80f) / 50f);
			}
			if (c.FollowerCount < 180f)
			{
				return Vector2.Lerp(new Vector2(0.5f, 0.8f), new Vector2(0.25f, 0.75f), (c.FollowerCount - 120f) / 60f);
			}
			if (c.FollowerCount < 250f)
			{
				return Vector2.Lerp(new Vector2(0.25f, 0.75f), new Vector2(0.2f, 0.5f), (c.FollowerCount - 180f) / 70f);
			}
			if (c.FollowerCount < 350f)
			{
				return Vector2.Lerp(new Vector2(0.2f, 0.5f), new Vector2(0.1f, 0.3f), (c.FollowerCount - 250f) / 100f);
			}

			return new Vector2(0.1f, 0.3f);
		}
    }
}
