using JetBrains.Annotations;
using Pandaros.API.Entities;
using Pandaros.API.Models;
using Pandaros.API.Questing;
using Pandaros.API.Tutorials.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.API.Tutorials.Prerequisites
{
    public class QuestPrerequisite : ITutorialPrerequisite
    {
        public string Quest { get; set; }

        public string Name => nameof(QuestPrerequisite);

        public QuestPrerequisite(string questName)
        {
            Quest = questName;
        }

        public bool MeetsCondition(Players.Player p)
        {
            if (p.ActiveColony == null || !QuestingSystem.CompletedQuests.TryGetValue(p.ActiveColony, out var completedQuests))
                return false;

            return completedQuests.Contains(Quest);
        }
    }
}
