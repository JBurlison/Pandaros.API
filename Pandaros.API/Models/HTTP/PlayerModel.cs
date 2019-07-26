using Pandaros.API.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.API.Models.HTTP
{
    public class PlayerModel
    {
        public List<StockpileItem> Inventory { get; set; }
        public SerializableVector3 Position { get; set; }
        public float HealthMax { get; }
        public List<int> Colonies { get; set; }
        public string LastKnownLocale { get; set; }
        public int ActiveColony { get; set; } = -1;
        public string ConnectionState { get; set; }
        public string Name { get; set; }
        public float Health { get; set; }
        public ulong SteamId { get; set; }
        public string NetworkIdType { get; set; }
        public PlayerState PlayerState { get; set; }
    }
}
