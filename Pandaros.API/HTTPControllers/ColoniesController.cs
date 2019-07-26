using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Pandaros.API.Extender;
using Pandaros.API.Models;
using Pandaros.API.Models.HTTP;

namespace Pandaros.API.HTTPControllers
{
    public class ColoniesController : IPandaController
    {
        [PandaHttp(RestVerb.Get, "/Colonies/All")]
        public RestResponse GetColonies()
        {
            var retVal = new List<ColonyModel>();

            foreach (var c in ServerManager.ColonyTracker.ColoniesByID)
                retVal.Add(new ColonyModel()
                {
                    ColonyId = c.Key,
                    Name = c.Value.Name,
                    LaborerCount = c.Value.LaborerCount,
                    AutoRecruit = c.Value.JobFinder.AutoRecruit,
                    AvailableFood = c.Value.Stockpile.TotalFood,
                    BedCount = c.Value.BedTracker.BedCount,
                    Happiness = c.Value.HappinessData.CachedHappiness,
                    OpenJobCount = c.Value.JobFinder.OpenJobCount,
                    StockpileCount = c.Value.Stockpile.ItemCount,
                    BannerCount = c.Value.Banners.Length,
                    ColonistCount = c.Value.FollowerCount,
                    Owners = c.Value.Owners.Select(o => o.Name).ToList()
                });

            return new RestResponse() { Content = retVal.ToJsonSerializedByteArray() };
        }

        [PandaHttp(RestVerb.Get, "/Colonies/Stockpile")]
        public RestResponse GetStockpile(int colonyId)
        {
            if (ServerManager.ColonyTracker.ColoniesByID.TryGetValue(colonyId, out var colony))
            {
                List<StockpileItem> stockpileItems = new List<StockpileItem>();

                foreach (var item in colony.Stockpile.Items)
                {
                    var itemType = ItemTypes.GetType(item.Key);

                    var newItem = new StockpileItem()
                    {
                        Count = item.Value,
                        ItemId = item.Key,
                        ItemName = itemType.Name,
                        IconPath = itemType.Icon,
                        Translations = new Dictionary<string, string>()
                    };

                    foreach (var localization in Localization.LocaleTexts)
                    {
                        if (Localization.TryGetType(localization.Key, itemType, out string localized))
                            newItem.Translations[localization.Key] = localized;
                    }

                    stockpileItems.Add(newItem);
                }

                return new RestResponse() { Content = stockpileItems.ToJsonSerializedByteArray() };
            }
            else
                throw new IndexOutOfRangeException("Unknown colony id " + colonyId);
        }
    }
}
