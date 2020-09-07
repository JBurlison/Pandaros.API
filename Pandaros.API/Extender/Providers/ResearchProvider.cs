using Pandaros.API.Research;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pandaros.API.Extender.Providers
{
    public class ResearchProvider : IOnAddResearchablesExtender
    {
        public List<Type> LoadedAssembalies { get; } = new List<Type>();

        public string InterfaceName => nameof(IPandaResearch);
        public Type ClassType => null;

        public void OnAddResearchables()
        {
            StringBuilder sb = new StringBuilder();
            APILogger.LogToFile("-------------------Research Loaded----------------------");
            var i = 0;

            foreach (var s in LoadedAssembalies)
            {
                if (Activator.CreateInstance(s) is IPandaResearch pandaResearch &&
                    !string.IsNullOrEmpty(pandaResearch.name))
                {
                    var research = new PandaResearchable(pandaResearch, 1);
                    research.ResearchComplete += pandaResearch.ResearchComplete;

                    if (pandaResearch.NumberOfLevels > 1)
                        for (var l = 2; l <= pandaResearch.NumberOfLevels; l++)
                        {
                            research = new PandaResearchable(pandaResearch, l);
                            research.ResearchComplete += pandaResearch.ResearchComplete;
                        }

                    sb.Append(pandaResearch.name + ", ");
                    pandaResearch.BeforeRegister();

                    foreach (var item in research.Recipes)
                    {
                        if (item.UnlockType == Science.ERecipeUnlockType.Recipe)
                        {
                            if (ServerManager.RecipeStorage.TryGetRecipe(new Recipes.RecipeKey(item.Identifier), out var recipe))
                                ServerManager.RecipeStorage.AddScienceRequirement(recipe);
                        }
                        else
                        {
                            if (ServerManager.RecipeStorage.TryGetRecipes(item.Identifier, out var recipe))
                                for (int r = 0; r < recipe.Count; r++)
                                    ServerManager.RecipeStorage.AddScienceRequirement(recipe[r]);
                        }
                    }

                    research.Register();
                    pandaResearch.OnRegister();
                    i++;
                    
                    if (i > 5)
                    {
                        i = 0;
                        sb.AppendLine();
                    }
                }
            }

            APILogger.LogToFile(sb.ToString());
            APILogger.LogToFile("---------------------------------------------------------");
        }
    }
}
