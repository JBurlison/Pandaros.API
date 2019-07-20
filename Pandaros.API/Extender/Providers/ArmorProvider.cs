using Pandaros.API.Items;
using Pandaros.API.Items.Armor;
using Pipliz.JSON;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pandaros.API.Extender.Providers
{
    public class ArmorProvider : IAfterWorldLoad
    {
        public List<Type> LoadedAssembalies { get; } = new List<Type>();

        public string InterfaceName => nameof(IArmor);
        public Type ClassType => null;

        public void AfterWorldLoad()
        {
            StringBuilder sb = new StringBuilder();
            APILogger.LogToFile("-------------------Armor Loaded----------------------");
            var i = 0;
            List<IArmor> armors = new List<IArmor>();


            foreach (var item in LoadedAssembalies)
            {
                if (Activator.CreateInstance(item) is IArmor armor &&
                    !string.IsNullOrEmpty(armor.name))
                {
                    armors.Add(armor);
                }
            }

            var settings = GameInitializer.GetJSONSettingPaths(GameInitializer.NAMESPACE + ".CSItems");

            foreach (var modInfo in settings)
            {
                foreach (var path in modInfo.Value)
                {
                    try
                    {
                        var jsonFile = JSON.Deserialize(modInfo.Key + "/" + path);

                        if (jsonFile.NodeType == NodeType.Array && jsonFile.ChildCount > 0)
                            foreach (var item in jsonFile.LoopArray())
                            {
                                if (item.TryGetAs("Durability", out int durability))
                                    armors.Add(item.JsonDeerialize<MagicArmor>());
                            }
                    }
                    catch (Exception ex)
                    {
                        APILogger.LogError(ex);
                    }
                }
            }
            
            foreach (var armor in armors)
            {
                if (ItemTypes.IndexLookup.TryGetIndex(armor.name, out var index))
                {
                    ArmorFactory.ArmorLookup[index] = armor;
                    sb.Append($"{armor.name}, ");
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
