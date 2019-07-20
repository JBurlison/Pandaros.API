using Pandaros.API.Items;
using Pandaros.API.Items.Weapons;
using Pipliz.JSON;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pandaros.API.Extender.Providers
{
    public class WeaponProvider : IAfterWorldLoad
    {
        public List<Type> LoadedAssembalies { get; } = new List<Type>();

        public string InterfaceName => nameof(IWeapon);
        public Type ClassType => null;

        public void AfterWorldLoad()
        {
            StringBuilder sb = new StringBuilder();
            APILogger.LogToFile("-------------------Weapons Loaded----------------------");
            var i = 0;
            List<IWeapon> loadedWeapons = new List<IWeapon>();

            foreach (var item in LoadedAssembalies)
                if (Activator.CreateInstance(item) is IWeapon weapon &&
                    !string.IsNullOrEmpty(weapon.name))
                    loadedWeapons.Add(weapon);

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
                                if (item.TryGetAs("WepDurability", out int durability))
                                    loadedWeapons.Add(item.JsonDeerialize<MagicWeapon>());
                            }
                    }
                    catch (Exception ex)
                    {
                        APILogger.LogError(ex);
                    }
                }
            }

            foreach (var weapon in loadedWeapons)
            {
                if (ItemTypes.IndexLookup.TryGetIndex(weapon.name, out var index))
                {
                    WeaponFactory.WeaponLookup[index] = weapon;
                    sb.Append($"{weapon.name}, ");
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
