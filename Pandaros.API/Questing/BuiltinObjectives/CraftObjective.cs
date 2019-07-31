using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
    public class CraftObjective : IPandaQuestObjective
    {
        public CraftObjective(string objectiveKey, string item, int count, string localizationKey = null, LocalizationHelper localizationHelper = null)
        {
            ObjectiveKey = objectiveKey;
            Item = item;
            CraftCount = count;
            LocalizationHelper = localizationHelper;
            LocalizationKey = localizationKey;
        }

        public string ObjectiveKey { get; set; }
        public string LocalizationKey { get; set; }
        public int CraftCount { get; set; }
        public Dictionary<int, int> CurrentCraftCount { get; set; } = new Dictionary<int, int>();

        public string Item { get; set; }
        public LocalizationHelper LocalizationHelper { get; set; }

        public string GetObjectiveProgressText(IPandaQuest quest, Colony colony, Players.Player player)
        {
            if (!CurrentCraftCount.ContainsKey(colony.ColonyID))
                CurrentCraftCount[colony.ColonyID] = 0;

            var formatStr = LocalizationHelper.LocalizeOrDefault(LocalizationKey, player);

            if (formatStr.Count(c => c == '{') == 2)
                return string.Format(LocalizationHelper.LocalizeOrDefault(LocalizationKey, player), CurrentCraftCount[colony.ColonyID], CraftCount);
            else
                return formatStr;
        }

        public float GetProgress(IPandaQuest quest, Colony colony)
        {
            if (!CurrentCraftCount.ContainsKey(colony.ColonyID))
                CurrentCraftCount[colony.ColonyID] = 0;

            if (CurrentCraftCount[colony.ColonyID] != 0)
                return CurrentCraftCount[colony.ColonyID] / CraftCount;
            else
                return 1;
        }

        public void Load(JObject node, IPandaQuest quest, Colony colony)
        {
            if (!CurrentCraftCount.ContainsKey(colony.ColonyID))
                CurrentCraftCount[colony.ColonyID] = 0;

            CurrentCraftCount = node.ToObject<Dictionary<int, int>>();
        }

        public JObject Save(IPandaQuest quest, Colony colony)
        {
            if (!CurrentCraftCount.ContainsKey(colony.ColonyID))
                CurrentCraftCount[colony.ColonyID] = 0;

            return JObject.FromObject(CurrentCraftCount);
        }
    }
}
