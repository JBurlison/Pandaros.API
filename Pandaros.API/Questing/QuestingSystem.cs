using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pandaros.API.Questing.Models;
using Pandaros.API.Extender;
using NetworkUI;
using Pipliz.JSON;
using System.IO;
using Newtonsoft.Json;

namespace Pandaros.API.Questing
{
    public class QuestingSystem : IOnTimedUpdate, IOnConstructInventoryManageColonyUI, IOnLoadingColony, IOnSavingColony
    {
        public static Dictionary<string, IPandaQuest> QuestPool { get; set; } = new Dictionary<string, IPandaQuest>();
        public static Dictionary<Colony, List<string>> CompletedQuests { get; set; } = new Dictionary<Colony, List<string>>();

        public double NextUpdateTimeMin => 3;

        public double NextUpdateTimeMax => 6;

        public double NextUpdateTime { get; set; }

        public void OnConstructInventoryManageColonyUI(Players.Player player, NetworkMenu networkMenu)
        {
           
        }

        public void OnLoadingColony(Colony c, JSONNode n)
        {
            if (!CompletedQuests.ContainsKey(c))
                CompletedQuests.Add(c, new List<string>());

            var saveDir = GameInitializer.SAVE_LOC + "Quests/";
            var saveFile = saveDir + c.ColonyID + ".json";

            if (!Directory.Exists(saveDir))
                Directory.CreateDirectory(saveDir);

            if (File.Exists(saveFile))
            {
                var colonySave = JsonConvert.DeserializeObject<ColonyQuestingSave>(File.ReadAllText(saveFile));

                CompletedQuests[c] = colonySave.CompletedQuests;

                foreach (var kvp in colonySave.InProgressQuests)
                {
                    if (QuestPool.TryGetValue(kvp.Key, out var quest))
                    {
                        quest.Load(kvp.Value.QuestSave, c);

                        foreach (var o in kvp.Value.Objectives)
                            if (quest.QuestObjectives.TryGetValue(o.Key, out var objective))
                                objective.Load(o.Value, quest, c);
                    }
                }
            }
        }

        public void OnSavingColony(Colony c, JSONNode n)
        {
            if (!CompletedQuests.ContainsKey(c))
                CompletedQuests.Add(c, new List<string>());

            var colonySave = new ColonyQuestingSave()
            {
                ColonyId = c.ColonyID,
                CompletedQuests = CompletedQuests[c],
                InProgressQuests = new Dictionary<string, QuestingSave>()
            };

            var saveDir = GameInitializer.SAVE_LOC + "Quests/";
            var saveFile = saveDir + c.ColonyID + ".json";

            if (!Directory.Exists(saveDir))
                Directory.CreateDirectory(saveDir);

            if (File.Exists(saveFile))
                File.Delete(saveFile);

            foreach (var kvp in QuestPool)
            {
                colonySave.InProgressQuests.Add(kvp.Key, new QuestingSave()
                {
                    Name = kvp.Key,
                    QuestSave = kvp.Value.Save(c),
                    Objectives = new Dictionary<string, Newtonsoft.Json.Linq.JObject>()
                });

                foreach (var o in kvp.Value.QuestObjectives)
                {
                    var saveNode = o.Value.Save(kvp.Value, c);

                    if (saveNode != null)
                        colonySave.InProgressQuests[kvp.Key].Objectives.Add(o.Key, saveNode);
                }
            }

            File.WriteAllText(saveFile, JsonConvert.SerializeObject(colonySave));
        }

        public void OnTimedUpdate()
        {
            foreach (var quest in QuestPool)
            {
                foreach (var colony in ServerManager.ColonyTracker.ColoniesByID.ValsRaw)
                {
                    if (!CompletedQuests.ContainsKey(colony))
                        CompletedQuests.Add(colony, new List<string>());

                    bool ok = true;

                    foreach (var pre in quest.Value.QuestPrerequisites)
                        if (!pre.MeetsPrerequisite(quest.Value, colony))
                            ok = false;

                    if (ok)
                    {
                        bool allComplete = true;

                        foreach (var objective in quest.Value.QuestObjectives.Values)
                        {
                            if (objective.GetProgress(quest.Value, colony) < 1f)
                                allComplete = false;
                        }

                        if (allComplete)
                        {
                            foreach (var reward in quest.Value.QuestRewards)
                                reward.IssueReward(quest.Value, colony);

                            if (!quest.Value.CanRepeat(colony) && !CompletedQuests[colony].Contains(quest.Value.QuestKey))
                                CompletedQuests[colony].Add(quest.Value.QuestKey);
                        }
                    }
                }
            }
        }
    }
}
