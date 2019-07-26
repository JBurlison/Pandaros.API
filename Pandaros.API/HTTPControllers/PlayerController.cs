using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Pandaros.API.Entities;
using Pandaros.API.Extender;
using Pandaros.API.Models;
using Pandaros.API.Models.HTTP;


namespace Pandaros.API.HTTPControllers
{
    public class PlayerController : IPandaController
    {
        [PandaHttp(OperationType.Get, "/Players/All", "")]
        public RestResponse GetPlayers()
        {
            Dictionary<ulong, PlayerModel> players = new Dictionary<ulong, PlayerModel>();

            foreach (var player in Players.PlayerDatabase)
                players.Add(player.Key.steamID.m_SteamID, MapPlayer(player));

            return new RestResponse() { Content = players.ToUTF8SerializedJson() };
        }

        [PandaHttp(OperationType.Get, "/Players", "")]
        public RestResponse GetPlayers(ulong steamId)
        {
            Dictionary<ulong, PlayerModel> players = new Dictionary<ulong, PlayerModel>();

            foreach (var player in Players.PlayerDatabase)
                if (player.Key.steamID.m_SteamID == steamId)
                {
                    players.Add(player.Key.steamID.m_SteamID, MapPlayer(player));
                    break;
                }

            return new RestResponse() { Content = players.ToUTF8SerializedJson() };
        }

        private static PlayerModel MapPlayer(KeyValuePair<NetworkID, Players.Player> player)
        {
            var model = new PlayerModel()
            {
                Colonies = player.Value.Colonies.Select(c => c.ColonyID).ToList(),
                ConnectionState = player.Value.ConnectionState.ToString(),
                Health = player.Value.Health,
                LastKnownLocale = player.Value.LastKnownLocale,
                Name = player.Value.Name,
                Position = new SerializableVector3(player.Value.Position),
                NetworkIdType = player.Key.type.ToString(),
                SteamId = player.Key.steamID.m_SteamID,
                Inventory = new List<StockpileItem>(),
                PlayerState = PlayerState.GetPlayerState(player.Value)
            };

            if (player.Value.ActiveColony != null)
                model.ActiveColony = player.Value.ActiveColony.ColonyID;

            foreach (var item in player.Value.Inventory.Items)
                model.Inventory.Add(ColoniesController.MapStockpileItem(item.Type, item.Amount));
            return model;
        }
    }
}
