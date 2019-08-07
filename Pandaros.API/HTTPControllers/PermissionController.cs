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
using static PermissionsManager;

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

        [PandaHttp(OperationType.Get, "/Permissions/Player", "Gets a players current permissions")]
        public RestResponse GetPlayerPermission(ulong steamId)
        {
            if (Players.TryGetPlayer(new NetworkID(new Steamworks.CSteamID(steamId)), out var player) &&
                PermissionsManager.Users.TryGetValue(player.ID, out var permissionsGroup))
            {
                return new RestResponse() { Content = permissionsGroup.ToUTF8SerializedJson(new PermissionsManager.PermissionsList.Converter()) };
            }

            return RestResponse.BlankJsonObject;
        }

        [PandaHttp(OperationType.Patch, "/Permissions/Player", "Updates a players current permission group")]
        public RestResponse SetPlayerGroup(ulong steamId, string group)
        {
            RestResponse restResponse = new RestResponse();
            SuccessResponse successResponse = new SuccessResponse();

            if (Players.TryGetPlayer(new NetworkID(new Steamworks.CSteamID(steamId)), out var player))
            {
                EnsureLoaded();
                if (!Groups.ContainsKey(group))
                {
                    successResponse.Success = false;
                    successResponse.Details = $"Failed to set user to group [{group}], it does not exist.";
                }
                else
                {
                    if (Users.TryGetValue(player.ID, out PermissionsGroup value))
                    {
                        value.SetGroup(group);
                    }
                    else
                    {
                        PermissionsGroup permissionsGroup = DefaultNormal.Copy();
                        permissionsGroup.SetGroup(group);
                        Users[player.ID] = permissionsGroup;
                    }
                    SaveUsers();
                    successResponse.Success = true;
                }
            }

            restResponse.Content = successResponse.ToUTF8SerializedJson();
            return restResponse;
        }

        [PandaHttp(OperationType.Get, "/Permissions/Groups", "Gets all permission groups on a server")]
        public RestResponse GetServerPermissions()
        {
          return new RestResponse(){ Content = PermissionsManager.Groups.ToUTF8SerializedJson(new PermissionsManager.PermissionsList.Converter()) };
        }

        //[PandaHttp(OperationType.Patch, "/Permissions/Groups", "Updates permission groups on a server")]
        //public RestResponse SetServerPermissions(string permissionJson)
        //{
        //    PermissionsManager.Groups = JsonConvert.DeserializeObject<Dictionary<string, PermissionsGroup>>(permissionJson, new PermissionsManager.PermissionsList.Converter());
        //    var permissionsSave = new PermissionsManager.
        //}
    }
}
