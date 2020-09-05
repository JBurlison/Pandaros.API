using Jobs;
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
    public class GuardJobSettingsProvider : IAfterModsLoadedExtention
    {
        public List<Type> LoadedAssembalies { get; } = new List<Type>();

        public string InterfaceName => nameof(ICSGuardJobSettings);
        public Type ClassType => null;

        public void AfterModsLoaded(List<ModLoader.ModDescription> list)
        {
            StringBuilder sb = new StringBuilder();
            APILogger.LogToFile("-------------------Guard Settings Loaded----------------------");
            var i = 0;
            List<ICSGuardJobSettings> json = new List<ICSGuardJobSettings>();

            foreach (var item in LoadedAssembalies)
            {
                if (Activator.CreateInstance(item) is ICSGuardJobSettings generateType &&
                    !string.IsNullOrEmpty(generateType.jobType))
                {
                    json.Add(generateType);
                    
                    sb.Append($"{generateType.jobType}, ");
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
                BlockJobLoader.BlockJobPatches.AddPatch(new BlockJobLoader.BlockJobPatch(-99999, JSON.DeserializeString(strValue)));
            }

            APILogger.LogToFile(sb.ToString());
            APILogger.LogToFile("---------------------------------------------------------");
        }
    }
}
