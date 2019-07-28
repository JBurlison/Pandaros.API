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

        string GetQuestText(Colony colony, Players.Player player);
        bool CanRepeat(Colony colony);
        void QuestComplete(Colony colony);
        JObject Save(Colony colony);
        void Load(JObject node, Colony colony);
    }
}
