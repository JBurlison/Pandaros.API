using Pandaros.API.Entities;
using Pandaros.API.Models;
using Pandaros.API.Tutorials.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.API.Tutorials.Prerequisites
{
    public class ColonistsCountPrerequisite : ITutorialPrerequisite
    {
        public int Count { get; set; }

        public string Name => nameof(ColonistsCountPrerequisite);

        public ColonistsCountPrerequisite(int count)
        {
            Count = count;
        }

        public bool MeetsCondition(Players.Player p)
        {
            if (p.ActiveColony == null)
                return false;

            return p.ActiveColony.FollowerCount >= Count;
        }
    }
}
