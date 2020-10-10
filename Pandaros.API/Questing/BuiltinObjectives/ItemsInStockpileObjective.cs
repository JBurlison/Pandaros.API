using ModLoaderInterfaces;
using Newtonsoft.Json.Linq;
using Pandaros.API.ColonyManagement;
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
    public class ItemsInStockpileObjective : IPandaQuestObjective
    {
        public string ItemName { get; set; }
        public string ObjectiveKey { get; set; }
        public float GoalCount { get; set; }
        public string LocalizationKey { get; set; }
        public LocalizationHelper LocalizationHelper { get; set; }

        public ItemsInStockpileObjective(string key, string jobName, int goalCount, LocalizationHelper localizationHelper)
        {
            ObjectiveKey = key;
            LocalizationHelper = localizationHelper;
            ItemName = jobName;
            GoalCount = goalCount;

            if (LocalizationHelper == null)
                LocalizationHelper = new LocalizationHelper(GameInitializer.NAMESPACE, "Quests");

            if (string.IsNullOrEmpty(LocalizationKey))
                LocalizationKey = nameof(ItemsInStockpileObjective);
        }

        public string GetObjectiveProgressText(IPandaQuest quest, Colony colony, Players.Player player)
        {
            var formatStr = QuestingSystem.LocalizationHelper.LocalizeOrDefault(LocalizationKey, player);
            int itemCount = 0;
            var item = ItemId.GetItemId(ItemName);

            if (colony.Stockpile.Contains(item))
                itemCount = colony.Stockpile.Items[item];

            if (formatStr.Count(c => c == '{') == 3)
                return string.Format(QuestingSystem.LocalizationHelper.LocalizeOrDefault(LocalizationKey, player), itemCount, GoalCount, LocalizationHelper.LocalizeOrDefault(ItemName, player));
            else
                return formatStr;
        }

        public float GetProgress(IPandaQuest quest, Colony colony)
        {
            if (GoalCount == 0)
                return 1;

            var item = ItemId.GetItemId(ItemName);
            int itemCount = 0;

            if (colony.Stockpile.Contains(item))
                itemCount = colony.Stockpile.Items[item];

            if (itemCount == 0)
                return 0;

            return itemCount / GoalCount;
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
