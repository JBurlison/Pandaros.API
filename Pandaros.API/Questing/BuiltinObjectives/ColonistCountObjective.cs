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
        public int ColonistGoal { get; set; }
        public string LocalizationKey { get; set; }
        public LocalizationHelper LocalizationHelper { get; set; }

        public ColonistCountObjective(int goalCount, string localizationKey = null, LocalizationHelper localizationHelper = null)
        {
            LocalizationHelper = localizationHelper;
            LocalizationKey = localizationKey;

            if (LocalizationHelper == null)
                LocalizationHelper = new LocalizationHelper(GameInitializer.NAMESPACE, "Quests");

            if (string.IsNullOrEmpty(LocalizationKey))
                LocalizationKey = nameof(ColonistCountObjective);
        }

        public string GetObjectiveProgressText(IPandaQuest quest, Colony colony, Players.Player player)
        {
            var formatStr = LocalizationHelper.LocalizeOrDefault(LocalizationKey, player);

            if (formatStr.Count(c => c == '{') == 2)
                return string.Format(LocalizationHelper.LocalizeOrDefault(LocalizationKey, player), colony.FollowerCount, ColonistGoal);
            else
                return formatStr;
        }

        public float GetProgress(IPandaQuest quest, Colony colony)
        {
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
