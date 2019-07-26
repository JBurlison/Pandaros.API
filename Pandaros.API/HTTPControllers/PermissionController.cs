using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Pandaros.API.Extender;
using Pandaros.API.Models;
using Pandaros.API.Models.HTTP;

namespace Pandaros.API.HTTPControllers
{
    public class PermissionController : IPandaController
    {
        [PandaHttp(OperationType.Post, "/Permissions/Kick", "Kicks a player from the server by their steamId.")]
        public RestResponse KickPlayer(ulong steamId)
        {
            if (Players.TryGetPlayer(new NetworkID(new Steamworks.CSteamID(steamId)), out var player))
                ServerManager.Disconnect(player);
            
            return RestResponse.Success;
        }
    }
}
