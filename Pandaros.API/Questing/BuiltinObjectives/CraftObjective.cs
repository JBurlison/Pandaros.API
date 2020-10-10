using Jobs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pandaros.API.localization;
using Pandaros.API.Models;
using Pandaros.API.Questing.Models;
using Recipes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.API.Questing.BuiltinObjectives
{
    [ModLoader.ModManager]
    public class CraftObjective : IPandaQuestObjective
    {
        public static Dictionary<string, List<CraftObjective>> CraftObjectivesByItem { get; set; } = new Dictionary<string, List<CraftObjective>>();

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnNPCCraftedRecipe, GameInitializer.NAMESPACE + ".Questing.BuiltinObjectives.OnNPCCraftedRecipe")]
        public static void OnNPCCraftedRecipe(IJob job, Recipe recipe, List<RecipeResult> results)
        {
           foreach (var item in results)
                if (CraftObjectivesByItem.TryGetValue(ItemId.GetItemId(item.Type), out var craftObjectives))
                    foreach (var objective in craftObjectives)
                    {
                        if (!objective.CurrentCraftCount.TryGetValue(job.Owner.ColonyID, out var count))
                        {
                            objective.CurrentCraftCount[job.Owner.ColonyID] = 0;
                            count = 0;
                        }

                        objective.CurrentCraftCount[job.Owner.ColonyID] = count + item.Amount;
                    }
        }

        public CraftObjective(string objectiveKey, string item, int count)
        {
            ObjectiveKey = objectiveKey;
            Item = item;
            CraftCount = count;

            if (string.IsNullOrEmpty(LocalizationKey))
                LocalizationKey = nameof(CraftObjective);

            if (!CraftObjectivesByItem.TryGetValue(Item, out var craftObjectives))
            {
                craftObjectives = new List<CraftObjective>();
                CraftObjectivesByItem[item] = craftObjectives;
            }

            if (!craftObjectives.Contains(this))
                craftObjectives.Add(this);
        }

        public string ObjectiveKey { get; set; }
        public string LocalizationKey { get; set; }
        public float CraftCount { get; set; }
        public Dictionary<int, int> CurrentCraftCount { get; set; } = new Dictionary<int, int>();

        public string Item { get; set; }

        public string GetObjectiveProgressText(IPandaQuest quest, Colony colony, Players.Player player)
        {
            if (!CurrentCraftCount.ContainsKey(colony.ColonyID))
                CurrentCraftCount[colony.ColonyID] = 0;

            var formatStr = QuestingSystem.LocalizationHelper.LocalizeOrDefault(LocalizationKey, player);

            if (formatStr.Count(c => c == '{') == 3)
                return string.Format(QuestingSystem.LocalizationHelper.LocalizeOrDefault(LocalizationKey, player), CurrentCraftCount[colony.ColonyID], CraftCount, QuestingSystem.LocalizationHelper.LocalizeOrDefault(Item, player));
            else
                return formatStr;
        }

        public float GetProgress(IPandaQuest quest, Colony colony)
        {
            if (!CurrentCraftCount.ContainsKey(colony.ColonyID))
                CurrentCraftCount[colony.ColonyID] = 0;

            if (CraftCount == 0)
                return 1;
            else if (CurrentCraftCount[colony.ColonyID] != 0)
                return CurrentCraftCount[colony.ColonyID] / CraftCount;
            else
                return 0;
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
