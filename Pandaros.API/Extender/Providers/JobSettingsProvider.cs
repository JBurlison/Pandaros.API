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
    public class JobSettingsProvider : IAfterModsLoadedExtention
    {
        public List<Type> LoadedAssembalies { get; } = new List<Type>();

        public string InterfaceName => nameof(ICSBlockJobSettings);
        public Type ClassType => null;

        public void AfterModsLoaded(List<ModLoader.ModDescription> list)
        {
            StringBuilder sb = new StringBuilder();
            APILogger.LogToFile("-------------------Block Job Settings Loaded----------------------");
            var i = 0;
            List<ICSBlockJobSettings> json = new List<ICSBlockJobSettings>();

            foreach (var item in LoadedAssembalies)
            {
                if (Activator.CreateInstance(item) is ICSBlockJobSettings generateType &&
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
