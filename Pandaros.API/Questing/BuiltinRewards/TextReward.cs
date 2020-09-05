using Pandaros.API.localization;
using Pandaros.API.Questing.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.API.Questing.BuiltinRewards
{
    public class TextReward : IPandaQuestReward
    {
        public string ItemIconName { get; set; }

        public string RewardKey { get; set; }

        public string LocalizationKey { get; set; }
        public LocalizationHelper LocalizationHelper { get; set; }

        public TextReward(string rewardKey, string itemIconName, string localizationKey, LocalizationHelper localizationHelper)
        {
            RewardKey = rewardKey;
            LocalizationKey = localizationKey;
            LocalizationHelper = localizationHelper;
            ItemIconName = itemIconName;
        }

        public string GetRewardText(IPandaQuest quest, Colony colony, Players.Player player)
        {
            return LocalizationHelper.LocalizeOrDefault(LocalizationKey, player);
        }

        public void IssueReward(IPandaQuest quest, Colony colony)
        {
            
        }
    }
}
