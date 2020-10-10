using Newtonsoft.Json.Linq;
using Pandaros.API.localization;
using Pandaros.API.Questing.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.API.Questing.BuiltinObjectives
{
    public class ColonistCountObjective : IPandaQuestObjective
    {
        public string ObjectiveKey { get; set; }
        public float ColonistGoal { get; set; }
        public string LocalizationKey { get; set; } = nameof(ColonistCountObjective);

        public ColonistCountObjective(string key, int goalCount)
        {
            ObjectiveKey = key;
            ColonistGoal = goalCount;
        }

        public string GetObjectiveProgressText(IPandaQuest quest, Colony colony, Players.Player player)
        {
            var formatStr = QuestingSystem.LocalizationHelper.LocalizeOrDefault(LocalizationKey, player);

            if (formatStr.Count(c => c == '{') == 2)
                return string.Format(QuestingSystem.LocalizationHelper.LocalizeOrDefault(LocalizationKey, player), colony.FollowerCount, ColonistGoal);
            else
                return formatStr;
        }

        public float GetProgress(IPandaQuest quest, Colony colony)
        {
            if (ColonistGoal == 0)
                return 1;

            if (colony.FollowerCount == 0)
                return 0;
            else if (colony.FollowerCount == ColonistGoal)
                return 1;
            else
                return colony.FollowerCount / ColonistGoal;
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
