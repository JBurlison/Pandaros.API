using Pandaros.API.localization;
using Pandaros.API.Models;
using Pandaros.API.Questing.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.API.Questing.BuiltinRewards
{
    public class RecipeUnlockReward : IPandaQuestReward
    {
        public Recipes.Recipe Recipe { get; set; }
        public string ItemIconName { get; set; }
        public string RewardKey { get; set; }
        public string RecipeKey { get; set; }
        public string LocalizationKey { get; set; }
        public LocalizationHelper LocalizationHelper { get; set; }

        public RecipeUnlockReward(string itemName, string rewardKey, string localizationKey = null, LocalizationHelper localizationHelper = null)
        {
            RecipeKey = itemName;
            RewardKey = rewardKey;
            if (ServerManager.RecipeStorage.TryGetRecipe(new Recipes.RecipeKey(itemName), out var recipe))
                Recipe = recipe;
            else
                APILogger.Log(ChatColor.red, "Item " + itemName + " recipe not found. unable to create RecipeUnlockReward for reward key " + rewardKey);

            if (LocalizationHelper == null)
                LocalizationHelper = new LocalizationHelper(GameInitializer.NAMESPACE, "Quests");

            if (string.IsNullOrEmpty(LocalizationKey))
                LocalizationKey = nameof(RecipeUnlockReward);
        }

        public string GetRewardText(IPandaQuest quest, Colony colony, Players.Player player)
        {
            var formatStr = LocalizationHelper.LocalizeOrDefault(LocalizationKey, player);
            var item = ItemId.GetItemId(RecipeKey);

            if (formatStr.Count(c => c == '{') == 1)
                return string.Format(LocalizationHelper.LocalizeOrDefault(LocalizationKey, player), LocalizationHelper.LocalizeOrDefault(RecipeKey, player));
            else
                return formatStr;
        }

        public void IssueReward(IPandaQuest quest, Colony colony)
        {
            colony.RecipeData.UnlockPartial(Recipe);
        }
    }
}
