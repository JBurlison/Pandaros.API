using Jobs;
using Microsoft.OpenApi.Models;
using Pandaros.API.Entities;
using Pandaros.API.Extender;
using Pandaros.API.Models;
using Pandaros.API.Models.HTTP;
using Science;
using System.Collections.Generic;
using System.Linq;

namespace Pandaros.API.HTTPControllers
{
    public class ScienceController : IPandaController
    {
        [PandaHttp(OperationType.Get, "/Science/Id", "Gets the Science based on id")]
        public RestResponse GetScience(uint id)
        {
            var science = ServerManager.ScienceManager.ScienceKeyToResearchableMapping.FirstOrDefault(kvp => kvp.Key.Index == id).Value;

            if (science != null)
            {
                return MapScience(science);
            }
            else
                return RestResponse.BlankJsonObject;
        }

        [PandaHttp(OperationType.Get, "/Science/Key", "Gets the Science based on key")]
        public RestResponse GetScience(string key)
        {
            var science = ServerManager.ScienceManager.ScienceKeyToResearchableMapping.FirstOrDefault(kvp => kvp.Value.GetKey() == key).Value;

            if (science != null)
            {
                return MapScience(science);
            }
            else
                return RestResponse.BlankJsonObject;
        }

        private static RestResponse MapScience(AbstractResearchable science)
        {
            ScienceModel scienceModel = new ScienceModel();
            scienceModel.Id = science.AssignedKey.Index;
            scienceModel.Key = science.GetKey();
            scienceModel.Icon = science.GetIcon();
            scienceModel.RequiredScienceBiome = science.RequiredScienceBiome;

            var unlocks = science.GetClientRecipeUnlocks();

            if (unlocks != null)
                scienceModel.Unlocks = unlocks.ToList();

            foreach (var item in science.GetConditions())
            {
                if (item is ScientistCyclesCondition cycles)
                    scienceModel.ScientistCyclesCondition = cycles;

                if (item is ColonistCountCondition count)
                    scienceModel.ColonistCountCondition = count;
            }

            return new RestResponse() { Content = scienceModel.ToUTF8SerializedJson() };
        }
    }
}
