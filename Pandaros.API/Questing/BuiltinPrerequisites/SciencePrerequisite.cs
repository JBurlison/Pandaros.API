using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.OpenApi.Models;
using Pandaros.API.localization;
using Pandaros.API.Questing.Models;
using Science;

namespace Pandaros.API.Questing.BuiltinPrerequisites
{
    public class SciencePrerequisite : IPandaQuestPrerequisite
    {
        public string ScienceKey { get; set; }
        public string LocalizationKey { get; set; }
        public LocalizationHelper LocalizationHelper { get; set; }

        public SciencePrerequisite(string scienceKey, string localizationKey = null, LocalizationHelper localizationHelper = null)
        {
            ScienceKey = scienceKey;
            LocalizationHelper = localizationHelper;
            LocalizationKey = localizationKey;

            if (LocalizationHelper == null)
                LocalizationHelper = new LocalizationHelper(GameInitializer.NAMESPACE, "Quests");

            if (string.IsNullOrEmpty(LocalizationKey))
                LocalizationKey = nameof(SciencePrerequisite);
        }

        public string GetPrerequisiteText(IPandaQuest quest, Colony colony, Players.Player player)
        {
            return string.Format(LocalizationHelper.LocalizeOrDefault(LocalizationKey, player), LocalizationHelper.LocalizeOrDefault(ScienceKey, player));
        }

        public bool MeetsPrerequisite(IPandaQuest quest, Colony colony)
        {
            return colony.ScienceData.CompletedScience.Contains(new Science.ScienceKey(ScienceKey));
        }
    }
}
