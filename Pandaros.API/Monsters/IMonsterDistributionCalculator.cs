using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Pandaros.API.Monsters
{
    public interface IMonsterDistributionCalculator
    {
        Vector2 GetMonsterDistribution(Colony c);
    }
}
