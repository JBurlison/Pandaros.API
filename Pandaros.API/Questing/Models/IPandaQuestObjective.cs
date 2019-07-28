using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.API.Questing.Models
{
    public interface IPandaQuestObjective
    {
        string ObjectiveKey { get; }
        List<IPandaQuestReward> ObjectiveRewards { get; set; }
        List<IPandaObjectiveStep> ObjectiveSteps { get; set; }
        string GetObjectiveProgressText(IPandaQuest quest, Colony colony, Players.Player player);
        JObject Save(IPandaQuest quest, Colony colony);
        void Load(JObject node, IPandaQuest quest, Colony colony);
    }
}
