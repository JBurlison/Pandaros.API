using Newtonsoft.Json;
using Pandaros.API.Items;
using Pandaros.API.Models;
using Pipliz.JSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.API.Extender.Providers
{
    [LoadPriority(100)]
    public class GenerateTypesProvider : IAfterModsLoadedExtention
    {
        public List<Type> LoadedAssembalies { get; } = new List<Type>();

        public string InterfaceName => nameof(ICSGenerateType);
        public Type ClassType => null;

        public void AfterModsLoaded(List<ModLoader.ModDescription> list)
        {
            StringBuilder sb = new StringBuilder();
            APILogger.LogToFile("-------------------Generate Type Loaded----------------------");
            var i = 0;
            List<ICSGenerateType> json = new List<ICSGenerateType>();

            foreach (var item in LoadedAssembalies)
            {
                if (Activator.CreateInstance(item) is ICSGenerateType generateType &&
                    !string.IsNullOrEmpty(generateType.typeName))
                {
                    json.Add(generateType);
                    
                    sb.Append($"{generateType.typeName}, ");
                    i++;

                    if (i > 5)
                    {
                        i = 0;
                        sb.AppendLine();
                    }
                }
            }

            if (json.Count != 0)
            {
                var strValue = JsonConvert.SerializeObject(json, Formatting.None, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                APILogger.LogToFile(strValue);
                ItemTypesServer.BlockRotator.Patches.AddPatch(new ItemTypesServer.BlockRotator.BlockGeneratePatch(GameInitializer.MOD_FOLDER, -99999, JSON.DeserializeString(strValue)));
            }

            APILogger.LogToFile(sb.ToString());
            APILogger.LogToFile("---------------------------------------------------------");
        }
    }
}
