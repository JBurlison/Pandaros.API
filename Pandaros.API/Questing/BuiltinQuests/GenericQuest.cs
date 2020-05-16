using Newtonsoft.Json.Linq;
using Pandaros.API.localization;
using Pandaros.API.Questing.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.API.Questing.BuiltinQuests
{
    public class GenericQuest : IPandaQuest
    {
        public GenericQuest(string questKey, string questTextSentenceKey, LocalizationHelper localizationHelper)
        {
            QuestKey = questKey;
            QuestTextSentenceKey = questTextSentenceKey;
            LocalizationHelper = localizationHelper;
        }

        public string QuestKey { get; set; }
        public string QuestTextSentenceKey { get; set; }
        public bool Repeatable { get; set; }
        public LocalizationHelper LocalizationHelper { get; set; }

        public Dictionary<string, IPandaQuestObjective> QuestObjectives { get; set; }
        public List<IPandaQuestReward> QuestRewards { get; set; }
        public List<IPandaQuestPrerequisite> QuestPrerequisites { get; set; }

        public bool CanRepeat(Colony colony)
        {
            return Repeatable;
        }

        public string GetQuestText(Colony colony, Players.Player player)
        {
            return LocalizationHelper.LocalizeOrDefault(QuestTextSentenceKey, player);
        }

        public string GetQuestTitle(Colony colony, Players.Player player)
        {
            return LocalizationHelper.LocalizeOrDefault(QuestKey, player);
        }

        public void Load(JObject node, Colony colony)
        {
            
        }

        public void QuestComplete(Colony colony)
        {
            
        }

        public JObject Save(Colony colony)
        {
            return null;
        }
    }
}
