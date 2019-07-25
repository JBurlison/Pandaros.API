using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.API.Models.HTTP
{
    public class ColonyModel
    {
        public int ColonyId { get; set; }
        public string Name { get; set; }
        public int ColonistCount { get; set; }
        public int LaborerCount { get; set; }
        public int BannerCount { get; set; }
        public int BedCount { get; set; }
        public float Happiness { get; set; }
        public float AvailableFood { get; set; }
        public int StockpileCount { get; set; }
        public int OpenJobCount { get; set; }
        public bool AutoRecruit { get; set; }
        public List<string> Owners { get; set; }
    }
}
