using Newtonsoft.Json.Linq;
using Pandaros.API.ColonyManagement;
using Pandaros.API.localization;
using Pandaros.API.Questing.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.API.Questing.BuiltinObjectives
{
    public class FoodStoredObjective : IPandaQuestObjective
    {
        public string ObjectiveKey { get; set; }
        public float GoalCount { get; set; }
        public string LocalizationKey { get; set; }

        public FoodStoredObjective(string key, int goalCount)
        {
            ObjectiveKey = key;
            GoalCount = goalCount;

            if (string.IsNullOrEmpty(LocalizationKey))
                LocalizationKey = nameof(FoodStoredObjective);
        }

        public string GetObjectiveProgressText(IPandaQuest quest, Colony colony, Players.Player player)
        {
            var formatStr = QuestingSystem.LocalizationHelper.LocalizeOrDefault(LocalizationKey, player);

            if (formatStr.Count(c => c == '{') == 2)
                return string.Format(QuestingSystem.LocalizationHelper.LocalizeOrDefault(LocalizationKey, player), colony.Stockpile.TotalFood, GoalCount);
            else
                return formatStr;
        }

        public float GetProgress(IPandaQuest quest, Colony colony)
        {
            if (GoalCount == 0)
                return 1;

            if (colony.Stockpile.TotalFood == GoalCount)
                return 1;
            else if (colony.Stockpile.TotalFood == 0)
                return 0;
            else
                return colony.Stockpile.TotalFood / GoalCount;
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
