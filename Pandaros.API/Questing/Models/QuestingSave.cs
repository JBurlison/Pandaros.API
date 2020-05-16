using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.API.Questing.Models
{
    public class ColonyQuestingSave
    {
        public int ColonyId { get; set; }
        public Dictionary<string, QuestingSave> InProgressQuests { get; set; }
        public HashSet<string> CompletedQuests { get; set; }
    }

    public class QuestingSave
    {
        public string Name { get; set; }
        public JObject QuestSave { get; set; }
        public Dictionary<string, JObject> Objectives { get; set; }
    }
}
