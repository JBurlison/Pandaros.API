using Jobs;
using Microsoft.OpenApi.Models;
using Pandaros.API.Entities;
using Pandaros.API.Extender;
using Pandaros.API.Models;
using Pandaros.API.Models.HTTP;
using System.Collections.Generic;
using System.Linq;

namespace Pandaros.API.HTTPControllers
{
    public class ColoniesController : IPandaController
    {
        [PandaHttp(OperationType.Get, "/Colonies/All", "Gets all the colonies established on the server.")]
        public RestResponse GetColonies()
        {
            var retVal = new Dictionary<int, ColonyModel>();

            foreach (var c in ServerManager.ColonyTracker.ColoniesByID)
                retVal.Add(c.Key, MapColony(c.Value));

            return new RestResponse() { Content = retVal.ToUTF8SerializedJson() };
        }

        [PandaHttp(OperationType.Get, "/Colonies", "Gets a specific colony by colonyId")]
        public RestResponse GetColonies(int colonyId)
        {
            var retVal = new Dictionary<int, ColonyModel>();

            if (ServerManager.ColonyTracker.ColoniesByID.TryGetValue(colonyId, out var c))
            {
                retVal.Add(c.ColonyID, MapColony(c));
                return new RestResponse() { Content = retVal.ToUTF8SerializedJson() };
            }
            else
                return RestResponse.BlankJsonObject;
        }

        [PandaHttp(OperationType.Get, "/Colonies/Stockpile/All", "Gets a colonies entire stockpile.")]
        public RestResponse GetStockpile(int colonyId)
        {
            if (ServerManager.ColonyTracker.ColoniesByID.TryGetValue(colonyId, out var colony))
            {
                Dictionary<ushort, StockpileItem> stockpileItems = new Dictionary<ushort, StockpileItem>();

                foreach (var item in colony.Stockpile.Items)
                    stockpileItems.Add(item.Key, MapStockpileItem(item.Key, item.Value));

                return new RestResponse() { Content = stockpileItems.ToUTF8SerializedJson() };
            }
            else
                return RestResponse.BlankJsonObject;
        }
        
        [PandaHttp(OperationType.Get, "/Colonies/Researh", "Gets the colonies current research status.")]
        public RestResponse GetResearh(int colonyId)
        {
            if (ServerManager.ColonyTracker.ColoniesByID.TryGetValue(colonyId, out var colony))
            {
                Dictionary<ushort, StockpileItem> stockpileItems = new Dictionary<ushort, StockpileItem>();

                foreach (var item in colony.Stockpile.Items)
                    stockpileItems.Add(item.Key, MapStockpileItem(item.Key, item.Value));

                return new RestResponse() { Content = stockpileItems.ToUTF8SerializedJson() };
            }
            else
                return RestResponse.BlankJsonObject;
        }

        [PandaHttp(OperationType.Get, "/Colonies/Colonists/All", "Gets all the colonists in a colony.")]
        public RestResponse GetColonists(int colonyId)
        {
            if (ServerManager.ColonyTracker.ColoniesByID.TryGetValue(colonyId, out var colony))
            {
                var npcDic = new Dictionary<int, NPCModel>();

                foreach (var follower in colony.Followers)
                {
                    npcDic[follower.ID] = MapNPC(follower);
                }

                return new RestResponse() { Content = npcDic.ToUTF8SerializedJson() };
            }
            else
                return RestResponse.BlankJsonObject;
        }

        [PandaHttp(OperationType.Get, "/Colonies/Colonists", "Gets a specific colonists by npcId.")]
        public RestResponse GetColonists(int colonyId, int npcId)
        {
            if (ServerManager.ColonyTracker.ColoniesByID.TryGetValue(colonyId, out var colony))
            {
                var npcDic = new Dictionary<int, NPCModel>();

                foreach (var follower in colony.Followers)
                {
                    if (follower.ID == npcId)
                    {
                        npcDic[follower.ID] = MapNPC(follower);
                        break;
                    }
                }

                if (npcDic.Count != 0)
                    return new RestResponse() { Content = npcDic.ToUTF8SerializedJson() };
                else
                    return RestResponse.BlankJsonObject;
            }
            else
                return RestResponse.BlankJsonObject;
        }

        [PandaHttp(OperationType.Get, "/Colonies/Jobs/All", "Gets the job status of all the jobs in a colony.")]
        public RestResponse GetJobs(int colonyId)
        {
            if (ServerManager.ColonyTracker.ColoniesByID.TryGetValue(colonyId, out var colony))
            {
                var colonyJobs = new ColonyJobs()
                {
                    OpenJobsByTypeId = new Dictionary<int, int>(),
                    TakenJobsByTypeId = new Dictionary<int, int>()
                };

                List<IJob> openJobs = colony.JobFinder?.JobsData?.OpenJobs;

                if (openJobs != null)
                    foreach (var job in openJobs)
                    {
                        if (!colonyJobs.OpenJobsByTypeId.ContainsKey(job.NPCType.Type))
                            colonyJobs.OpenJobsByTypeId.Add(job.NPCType.Type, 0);

                        colonyJobs.OpenJobsByTypeId[job.NPCType.Type]++;
                    }

                foreach (var npc in colony.Followers)
                {
                    if (!colonyJobs.TakenJobsByTypeId.ContainsKey(npc.NPCType.Type))
                        colonyJobs.TakenJobsByTypeId.Add(npc.NPCType.Type, 0);

                    colonyJobs.TakenJobsByTypeId[npc.NPCType.Type]++;
                }

                return new RestResponse() { Content = colonyJobs.ToUTF8SerializedJson() };
            }
            else
                return RestResponse.BlankJsonObject;
        }

        public static NPCModel MapNPC(NPC.NPCBase follower)
        {
            return new NPCModel()
            {
                FoodHoursCarried = follower.FoodHoursCarried,
                GoalPosition = new SerializableVector3(follower.UsedPath.path?.Goal.Vector),
                Health = follower.health,
                Id = follower.ID,
                NpcTypeJobId = follower.Job.NPCType.Type,
                NPCInventory = follower.Inventory,
                Position = new SerializableVector3(follower.Position),
                ColonistInventory = ColonistInventory.Get(follower),
                BedPosition = new SerializableVector3(follower.UsedBedLocation)
            };
        }

        public static StockpileItem MapStockpileItem(ushort itemId, int count)
        {
            var itemType = ItemTypes.GetType(itemId);

            var newItem = new StockpileItem()
            {
                Count = count,
                ItemId = itemId,
                ItemName = itemType.Name,
                IconPath = itemType.Icon,
                Translations = new Dictionary<string, string>()
            };

            foreach (var localization in Localization.LocaleTexts)
            {
                if (Localization.TryGetType(localization.Key, itemType, out string localized))
                    newItem.Translations[localization.Key] = localized;
            }

            return newItem;
        }

        public static ColonyModel MapColony(Colony c)
        {
            return new ColonyModel()
            {
                ColonyId = c.ColonyID,
                Name = c.Name,
                LaborerCount = c.LaborerCount,
                AutoRecruit = c.JobFinder.AutoRecruit,
                AvailableFood = c.Stockpile.TotalFood,
                BedCount = c.BedTracker.BedCount,
                Happiness = c.HappinessData.CachedHappiness,
                OpenJobCount = c.JobFinder.OpenJobCount,
                StockpileCount = c.Stockpile.ItemCount,
                BannerCount = c.Banners.Length,
                ColonistCount = c.FollowerCount,
                Owners = c.Owners.Select(o => o.Name).ToList(),
                ColonyState = ColonyState.GetColonyState(c)
            };
        }
    }
}
