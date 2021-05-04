using Science;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.API.Models.HTTP
{
    public class ScienceModel
    {
        public uint Id { get; set; }
        public string Key { get; set; }
        public string RequiredScienceBiome { get; set; }
        public string Icon { get; set; }
        public ScientistCyclesCondition ScientistCyclesCondition { get; set; }
        public ColonistCountCondition ColonistCountCondition { get; set; }
        public List<RecipeUnlockClient> Unlocks { get; set; }
    }
}
