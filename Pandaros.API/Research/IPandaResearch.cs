﻿using Pandaros.API.Models;
using Science;
using Shared;
using System.Collections.Generic;

namespace Pandaros.API.Research
{
    public interface IPandaResearch : INameable
    {
        string IconDirectory { get; }
        Dictionary<int, List<InventoryItem>> RequiredItems { get; }
        Dictionary<int, List<IResearchableCondition>> Conditions { get; }
        Dictionary<int, List<(string, RecipeUnlockClient.EType)>> AdditionalUnlocks { get; }
        Dictionary<int, List<RecipeUnlock>> Unlocks { get; }
        Dictionary<int, List<string>> Dependancies { get; }
        int NumberOfLevels { get; }
        float BaseValue { get; }
        int BaseIterationCount { get; }
        bool AddLevelToName { get; }       
        void ResearchComplete(object sender, ResearchCompleteEventArgs e);
        void BeforeRegister();
        void OnRegister();
    }
}
