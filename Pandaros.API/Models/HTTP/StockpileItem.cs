using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.API.Models.HTTP
{
    public class StockpileItem
    {
        public ushort ItemId { get; set; }
        public int Count { get; set; }
        public string ItemName { get; set; }
        public Dictionary<string, string> Translations { get; set; }
        public string IconPath { get; set; }
    }
}
