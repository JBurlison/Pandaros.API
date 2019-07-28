using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.API.Questing.Models
{
    public interface IPandaObjectiveStep
    {
        string QuestStepKey { get; }
        List<IPandaQuestReward> StepRewards { get; set; }
        float GetStepProgress(IPandaQuest quest, Colony colony);
        string GetStepProgressText(IPandaQuest quest, Colony colony, Players.Player player);
        JObject Save(IPandaQuest quest, Colony colony);
        void Load(JObject node, IPandaQuest quest, Colony colony);
    }
}
