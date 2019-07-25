using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pandaros.API.Extender;
using Pandaros.API.Models;
using Pandaros.API.Models.HTTP;

namespace Pandaros.API.HTTPControllers
{
    public class ColoniesController : IPandaController
    {
        [PandaHttp(RestVerb.Get, "AllColonies")]
        public List<ColonyModel> GetColonies()
        {
            var retVal = new List<ColonyModel>();

            foreach (var c in ServerManager.ColonyTracker.ColoniesByID)
                retVal.Add(new ColonyModel()
                {
                    ColonyId = c.Key,
                    BannerCount = c.Value.Banners.Length,
                    ColonistCount = c.Value.FollowerCount
                });

            return retVal;
        }
    }
}
