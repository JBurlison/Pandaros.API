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
        float GetProgress(IPandaQuest quest, Colony colony);
        string GetObjectiveProgressText(IPandaQuest quest, Colony colony, Players.Player player);
        JObject Save(IPandaQuest quest, Colony colony);
        void Load(JObject node, IPandaQuest quest, Colony colony);
    }
}
