using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.API.Questing.Models
{
    public interface IPandaQuest
    {
        string QuestKey { get; }
        List<IPandaQuestObjective> QuestObjectives { get; set; }
        List<IPandaQuestReward> QuestRewards { get; set; }
        List<IPandaQuestPrerequisite> QuestPrerequisites { get; set; }
        int NumberOfTimesCompleted { get; set; }

        string GetQuestText(IPandaQuest quest, Colony colony, Players.Player player);
        bool CanRepeat(IPandaQuest quest, Colony colony);
        void QuestComplete(IPandaQuest quest, Colony colony);
        JObject Save(IPandaQuest quest, Colony colony);
        void Load(JObject node, IPandaQuest quest, Colony colony);
    }
}
