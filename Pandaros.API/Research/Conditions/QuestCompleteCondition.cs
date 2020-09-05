using Pandaros.API.Questing;
using Science;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.API.Research.Conditions
{
    public class QuestCompleteGoal : IResearchableCondition
    {
        public string QuestKey { get; set; }

        public QuestCompleteGoal(string questKey)
        {
            QuestKey = questKey;
        }

        public bool IsConditionMet(AbstractResearchable researchable, ColonyScienceState manager)
        {
            return QuestingSystem.CompletedQuests.TryGetValue(manager.Colony, out var completed) && completed.Contains(QuestKey);
        }
    }
}
