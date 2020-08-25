using Pandaros.API.Models;
using Recipes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.API.Models
{
   
    public interface ICSPlayerRecipe : INameable
    {
        List<RecipeItem> requires { get; }
        List<RecipeResult> results { get; }
        bool isOptional { get; }
    }
}
