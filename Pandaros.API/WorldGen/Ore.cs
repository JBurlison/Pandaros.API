﻿using BlockTypes;
using Pipliz.JSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrainGeneration;

namespace Pandaros.API.WorldGen
{
    [ModLoader.ModManager]
    public static class Ore
    {
        public static JSONNode LoadedOres { get; set; }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameInitializer.NAMESPACE + ".WorldGen")]
        [ModLoader.ModCallbackDependsOn("create_servermanager_trackers")]
        public static void AddOres()
        {
            var settings = GameInitializer.GetJSONSettingPaths(GameInitializer.NAMESPACE + ".OreLayers");

            if (settings.Count == 0)
            {
                APILogger.Log("No Ore layers loaded.");
                return;
            }

            foreach (var modInfo in settings)
            {
                foreach (var path in modInfo.Value)
                {
                    var newMenu = JSON.Deserialize(modInfo.Key + "/" + path);

                    if (LoadedOres == null)
                        LoadedOres = newMenu;
                    else
                    {
                        LoadedOres.Merge(newMenu);
                    }
                }
            }

            try
            {
                var terrainGen = ServerManager.TerrainGenerator as TerrainGenerator;
                var stoneGen = terrainGen.FinalChunkModifier as TerrainGenerator.InfiniteStoneLayerGenerator;
                var oreGen = stoneGen.InnerGenerator as TerrainGenerator.OreLayersGenerator;

                foreach (var ore in LoadedOres.LoopArray())
                {
                    if (ore.TryGetAs("Chance", out byte chance) && ore.TryGetAs("Type", out string type) && ore.TryGetAs("Depth", out byte depth))
                    {
                        try
                        {
                            var item = ItemTypes.GetType(type);

                            ore.TryGetAsOrDefault("ScienceBiome", out string scienceBiome, null);

                            if (item != null && item.ItemIndex != ColonyBuiltIn.ItemTypes.AIR.Id)
                                oreGen.AddLayer(new TerrainGenerator.Settings.OreLayer() { Depth = depth, Type = item.Name, Chance = chance, ScienceBiome = scienceBiome });
                            else
                                APILogger.Log(ChatColor.yellow, "Unable to find item {0}", type);
                        }
                        catch (Exception ex)
                        {
                            APILogger.LogError(ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                APILogger.LogError(ex);
            }
        }
    }
}
