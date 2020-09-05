using Jobs.Implementations;
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
        public GenericQuest() { }

        public GenericQuest(string questKey, string questTextSentenceKey, string icon, LocalizationHelper localizationHelper)
        {
            ItemIconName = icon;
            QuestKey = questKey;
            QuestTextSentenceKey = questTextSentenceKey;
            LocalizationHelper = localizationHelper;
            QuestingSystem.QuestPool[questKey] = this;
        }

        public virtual string ItemIconName { get; set; }
        public virtual string QuestKey { get; set; }
        public virtual string QuestTextSentenceKey { get; set; }
        public virtual bool Repeatable { get; set; }
        public virtual LocalizationHelper LocalizationHelper { get; set; }

        public virtual Dictionary<string, IPandaQuestObjective> QuestObjectives { get; set; }
        public virtual List<IPandaQuestReward> QuestRewards { get; set; }
        public virtual List<IPandaQuestPrerequisite> QuestPrerequisites { get; set; }

        public virtual bool CanRepeat(Colony colony)
        {
            return Repeatable;
        }

        public virtual string GetQuestText(Colony colony, Players.Player player)
        {
            return LocalizationHelper.LocalizeOrDefault(QuestTextSentenceKey, player);
        }

        public virtual string GetQuestTitle(Colony colony, Players.Player player)
        {
            return LocalizationHelper.LocalizeOrDefault(QuestKey, player);
        }

        public virtual void Load(JObject node, Colony colony)
        {
            foreach (var item in QuestObjectives)
            {
                if(node.TryGetValue(item.Key, out var save))
                    item.Value.Load(save as JObject, this, colony);
            }
        }

        public virtual void QuestComplete(Colony colony)
        {
            
        }

        public virtual JObject Save(Colony colony)
        {
            JObject retval = new JObject();

            foreach (var item in QuestObjectives)
            {
                var save = item.Value.Save(this, colony);

                if (save != null)
                    retval[item.Key] = save;
            }

            return retval;
        }
    }
}
