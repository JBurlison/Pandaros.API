using Pandaros.API.Entities;
using Pandaros.API.Models;
using Pandaros.API.Tutorials.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.API.Tutorials.Prerequisites
{
    public class ItemPlacedPrerequisite : ITutorialPrerequisite
    {
        public string Item { get; set; }
        public int Count { get; set; }

        public ItemPlacedPrerequisite(string item, int count)
        {
            Item = item;
            Count = count;
        }

        public bool MeetsCondition(Players.Player p)
        {
            var ps = PlayerState.GetPlayerState(p);

            return ps.ItemsPlaced.TryGetValue(ItemId.GetItemId(Item), out var itemsPlaced) && itemsPlaced >= Count;
        }
    }
}
