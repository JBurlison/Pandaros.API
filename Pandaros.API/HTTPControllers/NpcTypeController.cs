using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NPC;
using Pandaros.API.Extender;
using Pandaros.API.Models;
using Pandaros.API.Models.HTTP;

namespace Pandaros.API.HTTPControllers
{
    public class NpcTypeController : IPandaController
    {
        [PandaHttp(RestVerb.Get, "/NpcType/All")]
        public RestResponse GetAllJobTypes()
        {
            Dictionary<ushort, NPCTypeModel> jobs = new Dictionary<ushort, NPCTypeModel>();

            foreach (var job in NPCType.NPCTypes)
            {
                jobs[job.Key.Type] = new NPCTypeModel()
                {
                    NpcTypeId = job.Key.Type,
                    NPCType = job.Key,
                    NPCTypeSettings = job.Value
                };
            }

            return new RestResponse() { Content = jobs.ToUTF8SerializedJson() };
        }

        [PandaHttp(RestVerb.Get, "/NpcType/Get")]
        public RestResponse GetAllJobTypes(ushort npcTypeId)
        {
            Dictionary<ushort, NPCTypeModel> jobs = new Dictionary<ushort, NPCTypeModel>();

            foreach (var job in NPCType.NPCTypes)
            {
                if (job.Key.Type == npcTypeId)
                {
                    jobs[job.Key.Type] = new NPCTypeModel()
                    {
                        NpcTypeId = job.Key.Type,
                        NPCType = job.Key,
                        NPCTypeSettings = job.Value
                    };

                    break;
                }
            }

            if (jobs.Count != 0)
                return new RestResponse() { Content = jobs.ToUTF8SerializedJson() };
            else
                return RestResponse.BlankJsonObject;
        }
    }
}
