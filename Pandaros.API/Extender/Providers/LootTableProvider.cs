using Pandaros.API.Items;
using Pandaros.API.Items.Armor;
using Pandaros.API.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pandaros.API.Extender.Providers
{
    public class LootTableProvider : IAfterWorldLoad
    {
        public List<Type> LoadedAssembalies { get; } = new List<Type>();

        public string InterfaceName => nameof(ILootTable);
        public Type ClassType => null;

        public void AfterWorldLoad()
        {
            StringBuilder sb = new StringBuilder();
            APILogger.LogToFile("-------------------Loot Tables Loaded----------------------");
            var i = 0;

            foreach (var item in LoadedAssembalies)
            {
                if (Activator.CreateInstance(item) is ILootTable lootTable)
                {
                    foreach (var table in lootTable.MonsterTypes)
                    {
                        if (LootTables.Lookup.TryGetValue(table, out var existingTable))
                            existingTable.LootPoolList.AddRange(existingTable.LootPoolList);
                        else
                            LootTables.Lookup[table] =lootTable;
                    }

                    sb.Append($"{lootTable.name}, ");
                    i++;

                    if (i > 5)
                    {
                        i = 0;
                        sb.AppendLine();
                    }
                }
            }

            APILogger.LogToFile(sb.ToString());
            APILogger.LogToFile("---------------------------------------------------------");
        }
    }
}
