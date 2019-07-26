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
    public class ImageController : IPandaController
    {
        [PandaHttp(RestVerb.Get, "GetIcon")]
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


    }
}
