using Jobs;
using Pandaros.API.Entities;
using Pandaros.API.Extender;
using Pandaros.API.Models;
using Pandaros.API.Models.HTTP;
using Shared.Options;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using Microsoft.OpenApi.Models;

namespace Pandaros.API.HTTPControllers
{
    public class ServerController : IPandaController
    {
        [PandaHttp(OperationType.Get, "/Server/WorldName", "Gets the name of the currently loaded world.")]
        public RestResponse GetWorldName()
        {
            return new RestResponse() { Content = new Dictionary<string, string>() { { "WorldName", ServerManager.WorldName } }.ToUTF8SerializedJson() };
        }

        [PandaHttp(OperationType.Get, "/Server/HostingSettings", "")]
        public RestResponse GetHostingSettings()
        {
            return new RestResponse() { Content = ServerManager.HostingSettings.ToUTF8SerializedJson() };
        }

        [PandaHttp(OperationType.Patch, "/Server/HostingSettings", "")]
        public RestResponse SetHostingSettings(string body)
        {
            ServerManager.HostingSettings = JsonConvert.DeserializeObject<HostingSettingsData>(body);
            return RestResponse.Success;
        }

        [PandaHttp(OperationType.Get, "/Server/ServerSettings", "")]
        public RestResponse GetServerSettings()
        {
            return new RestResponse() { Content = ServerManager.ServerSettings.ToUTF8SerializedJson() };
        }

        [PandaHttp(OperationType.Patch, "/Server/ServerSettings", "")]
        public RestResponse SetServerSettings(string body)
        {
            ServerManager.ServerSettings = JsonConvert.DeserializeObject<ServerManager.ServerSettingsData>(body);
            return RestResponse.Success;
        }
    }
}
