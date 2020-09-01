using Pandaros.API.Models;
using Recipes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pandaros.API.Extender.Providers
{
    public class RecipeProvider : IAfterItemTypesDefinedExtender
    {
        public List<Type> LoadedAssembalies { get; } = new List<Type>();

        public string InterfaceName => nameof(ICSRecipe);
        public Type ClassType => null;

        public void AfterItemTypesDefined()
        {
            StringBuilder sb = new StringBuilder();
            APILogger.LogToFile("-------------------Recipes Loaded----------------------");
            var i = 0;

            foreach (var item in LoadedAssembalies)
            {
                if (Activator.CreateInstance(item) is ICSRecipe recipe &&
                    !string.IsNullOrEmpty(recipe.name))
                {
                    var requirements = new List<InventoryItem>();
                    var results = new List<RecipeResult>();
                    recipe.JsonSerialize();

                    foreach (var ri in recipe.requires)
                        if (ItemTypes.IndexLookup.TryGetIndex(ri.type, out var itemIndex))
                            requirements.Add(new InventoryItem(itemIndex, ri.amount));

                    foreach (var ri in recipe.results)
                            results.Add(ri);

                    var newRecipe = new Recipe(recipe.name, requirements, results, recipe.defaultLimit, 0, (int)recipe.defaultPriority);

                    ServerManager.RecipeStorage.AddLimitTypeRecipe(recipe.Job, newRecipe);
                    ServerManager.RecipeStorage.AddBlockToRecipeMapping(recipe.JobBlock, recipe.Job);

                    if (recipe.isOptional)
                        ServerManager.RecipeStorage.AddScienceRequirement(newRecipe);
                        
                    sb.Append($"{recipe.name}, ");
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
