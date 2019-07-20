using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pandaros.API.Models;

namespace Pandaros.API.Items
{
    public static class LootTables
    {
        public static Dictionary<string, ILootTable> Lookup { get; private set; } = new Dictionary<string, ILootTable>();
    }
}
