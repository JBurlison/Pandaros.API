using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.API.Questing.Models
{
    public interface IPandaQuestReward
    {
        string ItemIconName { get; }
        string RewardKey { get; }
        string GetRewardText(IPandaQuest quest, Colony colony, Players.Player player);
        void IssueReward(IPandaQuest quest, Colony colony);
    }

}
