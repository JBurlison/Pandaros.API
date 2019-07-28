using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pandaros.API.Questing.Models;
using Pandaros.API.Extender;
using NetworkUI;

namespace Pandaros.API.Questing
{
    public class QuestingSystem : IOnTimedUpdate, IOnConstructInventoryManageColonyUI
    {
        public static Dictionary<string, IPandaQuest> QuestPool { get; set; } = new Dictionary<string, IPandaQuest>();

        public double NextUpdateTimeMin => 3;

        public double NextUpdateTimeMax => 6;

        public double NextUpdateTime { get; set; }

        public void OnConstructInventoryManageColonyUI(Players.Player player, NetworkMenu networkMenu)
        {
           
        }

        public void OnTimedUpdate()
        {
            
        }
    }
}
