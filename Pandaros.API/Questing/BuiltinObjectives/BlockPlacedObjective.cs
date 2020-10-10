using Newtonsoft.Json.Linq;
using Pandaros.API.Entities;
using Pandaros.API.localization;
using Pandaros.API.Models;
using Pandaros.API.Questing.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.API.Questing.BuiltinObjectives
{
    public class BlockPlacedObjective : IPandaQuestObjective
    {
        public string ObjectiveKey { get; set; }
        public string BlockName { get; set; }
        public float BlocksGoal { get; set; }
        public string LocalizationKey { get; set; } = nameof(BlockPlacedObjective);

        public BlockPlacedObjective(string key, string blockName, int goalCount)
        {
            ObjectiveKey = key;
            BlocksGoal = goalCount;
            BlockName = blockName;
        }

        public string GetObjectiveProgressText(IPandaQuest quest, Colony colony, Players.Player player)
        {
            var formatStr = QuestingSystem.LocalizationHelper.LocalizeOrDefault(LocalizationKey, player);
            var ps = PlayerState.GetPlayerState(player);
            var itemsPlaced = 0;

            ps.ItemsPlaced.TryGetValue(ItemId.GetItemId(BlockName), out itemsPlaced);

            if (formatStr.Count(c => c == '{') == 3)
                return string.Format(QuestingSystem.LocalizationHelper.LocalizeOrDefault(LocalizationKey, player), itemsPlaced, BlocksGoal, QuestingSystem.LocalizationHelper.LocalizeOrDefault(BlockName, player));
            else
                return formatStr;
        }

        public float GetProgress(IPandaQuest quest, Colony colony)
        {
            if (BlocksGoal == 0)
                return 1;

            var itemsPlaced = 0;

            foreach (var p in colony.Owners)
            {
                var ps = PlayerState.GetPlayerState(p);

                if (ps.ItemsPlaced.TryGetValue(ItemId.GetItemId(BlockName), out itemsPlaced) && itemsPlaced > 0)
                    break;
            }

            if (itemsPlaced == 0)
                return 0;
            else if (itemsPlaced == BlocksGoal)
                return 1;
            else
                return itemsPlaced / BlocksGoal;
        }

        public void Load(JObject node, IPandaQuest quest, Colony colony)
        {
            
        }

        public JObject Save(IPandaQuest quest, Colony colony)
        {
            return null;
        }
    }
}
