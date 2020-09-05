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
    public class JobsTakenObjective : IPandaQuestObjective
    {
        public string JobPrintName { get; set; }
        public string ObjectiveKey { get; set; }
        public float GoalCount { get; set; }
        public string LocalizationKey { get; set; }
        public LocalizationHelper LocalizationHelper { get; set; }

        public JobsTakenObjective(string key, string jobPrintName, int goalCount, string localizationKey = null, LocalizationHelper localizationHelper = null)
        {
            ObjectiveKey = key;
            LocalizationHelper = localizationHelper;
            LocalizationKey = localizationKey;
            JobPrintName = jobPrintName;
            GoalCount = goalCount;

            if (LocalizationHelper == null)
                LocalizationHelper = new LocalizationHelper(GameInitializer.NAMESPACE, "Quests");

            if (string.IsNullOrEmpty(LocalizationKey))
                LocalizationKey = nameof(JobsTakenObjective);
        }

        public string GetObjectiveProgressText(IPandaQuest quest, Colony colony, Players.Player player)
        {
            var formatStr = LocalizationHelper.LocalizeOrDefault(LocalizationKey, player);
            var jobs = ColonyTool.GetJobCounts(colony);
            var jobCount = 0;

            if (jobs.TryGetValue(JobPrintName, out var counts))
                jobCount = counts.TakenCount;

            if (formatStr.Count(c => c == '{') == 3)
                return string.Format(LocalizationHelper.LocalizeOrDefault(LocalizationKey, player), jobCount, GoalCount, LocalizationHelper.LocalizeOrDefault(JobPrintName, player));
            else
                return formatStr;
        }

        public float GetProgress(IPandaQuest quest, Colony colony)
        {
            if (GoalCount == 0)
                return 1;

            var jobs = ColonyTool.GetJobCounts(colony);

            if (jobs.TryGetValue(JobPrintName, out var counts))
            {
                if (counts.TakenCount == 0)
                    return 0;

                if (counts.TakenCount == GoalCount)
                    return 1;
                else
                    return counts.TakenCount / GoalCount;
            }
            else
                return 0;
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
