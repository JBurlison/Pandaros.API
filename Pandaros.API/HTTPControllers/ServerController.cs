using Jobs;
using Pandaros.API.Entities;
using Pandaros.API.Extender;
using Pandaros.API.Models;
using Pandaros.API.Models.HTTP;
using System.Collections.Generic;
using System.Linq;

namespace Pandaros.API.HTTPControllers
{
    public class ServerController : IPandaController
    {
        [PandaHttp(RestVerb.Get, "/Server/WorldName")]
        public RestResponse GetWorldName()
        {
            return new RestResponse() { Content = new Dictionary<string, string>() { { "WorldName", ServerManager.WorldName } }.ToUTF8SerializedJson() };
        }
    }
}
