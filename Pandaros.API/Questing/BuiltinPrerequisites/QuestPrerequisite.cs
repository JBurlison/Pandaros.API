﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.OpenApi.Models;
using Pandaros.API.localization;
using Pandaros.API.Questing.Models;

namespace Pandaros.API.Questing.BuiltinPrerequisites
{
    public class QuestPrerequisite : IPandaQuestPrerequisite
    {
        public string QuestKey { get; set; }
        public string LocalizationKey { get; set; }

        public QuestPrerequisite(string questKey)
        {
            QuestKey = questKey;

            if (string.IsNullOrEmpty(LocalizationKey))
                LocalizationKey = nameof(QuestPrerequisite);
        }

        public string GetPrerequisiteText(IPandaQuest quest, Colony colony, Players.Player player)
        {
            if (QuestingSystem.QuestPool.TryGetValue(QuestKey, out var requiredQuest))
            {
                return string.Format(QuestingSystem.LocalizationHelper.LocalizeOrDefault(LocalizationKey, player), requiredQuest.GetQuestTitle(colony, player));
            }
            else
            {
                return string.Format(QuestingSystem.LocalizationHelper.LocalizeOrDefault("QuestNotFound", player), QuestKey);
            }
        }

        public bool MeetsPrerequisite(IPandaQuest quest, Colony colony)
        {
            if (!QuestingSystem.QuestPool.ContainsKey(QuestKey))
                return true;

            return QuestingSystem.CompletedQuests.TryGetValue(colony, out var quests) && quests.Contains(QuestKey);
        }
    }
}
