using Pandaros.API.Upgrades;

namespace Pandaros.API.Upgrades
{
    public class SkillChance : IPandaUpgrade
    {
        public static string KEY => GameInitializer.NAMESPACE + "ColonyManagement.SkillChance";

        static localization.LocalizationHelper _localization = new localization.LocalizationHelper(GameInitializer.NAMESPACE, "Settlers");

        public int LevelCount => 5;

        public string UniqueKey => KEY;

        public static float GetSkillChance(Colony colony, int level = -1)
        {
            if (level == -1)
                level = colony.GetUpgradeLevel(KEY);

            return level * .05f;
        }

        public void GetLocalizedValues(Players.Player player, Colony colony, int unlockedLevelCount, out string upgradeName, out string currentResults, out string nextResults)
        {
            upgradeName = _localization.LocalizeOrDefault("SkillChance", player);
            currentResults = string.Format(_localization.LocalizeOrDefault("SkillChancepct", player), GetSkillChance(colony) * 100);
            nextResults = string.Format(_localization.LocalizeOrDefault("SkillChancepct", player), GetSkillChance(colony, colony.GetUpgradeLevel(KEY) + 1) * 100);
        }

        public long GetUpgradeCost(int unlockedLevels)
        {
            return (unlockedLevels * 500) + 1000;
        }

        public void ApplyLevel(Colony colony, int unlockedLevels, bool isLoading)
        {
            
        }

        public bool AppliesToColony(Colony colony)
        {
            return true;
        }
    }
}
