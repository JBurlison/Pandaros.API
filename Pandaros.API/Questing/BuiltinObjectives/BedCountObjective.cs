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
    public class BedCountObjective : IPandaQuestObjective
    {
        public string ObjectiveKey { get; set; }
        public float BedCount { get; set; }
        public string LocalizationKey { get; set; } = nameof(BedCountObjective);

        public BedCountObjective(string key, int goalCount)
        {
            ObjectiveKey = key;
            BedCount = goalCount;
        }

        public string GetObjectiveProgressText(IPandaQuest quest, Colony colony, Players.Player player)
        {
            var formatStr = QuestingSystem.LocalizationHelper.LocalizeOrDefault(LocalizationKey, player);

            if (formatStr.Count(c => c == '{') == 2)
                return string.Format(formatStr, colony.BedTracker.BedCount, BedCount);
            else
                return formatStr;
        }

        public float GetProgress(IPandaQuest quest, Colony colony)
        {
            if (BedCount == 0)
                return 1;

            if (colony.BedTracker.BedCount == 0)
                return 0;
            else if (colony.BedTracker.BedCount == BedCount)
                return 1;
            else
                return colony.BedTracker.BedCount / BedCount;
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
