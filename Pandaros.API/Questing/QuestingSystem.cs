using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pandaros.API.Questing.Models;
using Pandaros.API.Extender;
using NetworkUI;
using Pipliz.JSON;

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
            
        }

        public void OnSavingColony(Colony c, JSONNode n)
        {
            
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

                    if (CompletedQuests[colony].Contains(quest.Value.QuestKey))
                        if (!quest.Value.CanRepeat(colony))
                            ok = false;

                    foreach (var pre in quest.Value.QuestPrerequisites)
                        if (!pre.MeetsPrerequisite(quest.Value, colony))
                            ok = false;

                    if (ok)
                    {
                        bool allComplete = true;

                        foreach (var objective in quest.Value.QuestObjectives)
                        {
                            if (objective.GetProgress(quest.Value, colony) < 1f)
                                allComplete = false;
                        }

                        if (allComplete)
                        {
                            foreach (var reward in quest.Value.QuestRewards)
                                reward.IssueReward(quest.Value, colony);

                            if (!CompletedQuests[colony].Contains(quest.Value.QuestKey))
                                CompletedQuests[colony].Add(quest.Value.QuestKey)
                        }
                    }
                }
            }
        }
    }
}
