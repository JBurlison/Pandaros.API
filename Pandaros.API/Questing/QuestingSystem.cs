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
using NetworkUI.Items;
using ModLoaderInterfaces;

namespace Pandaros.API.Questing
{
    public class QuestingSystem : IOnTimedUpdate, IOnConstructInventoryManageColonyUI, IOnLoadingColony, IOnSavingColony
    {
        static readonly Pandaros.API.localization.LocalizationHelper _localizationHelper = new localization.LocalizationHelper(GameInitializer.NAMESPACE, "Quests");
        public static Dictionary<string, IPandaQuest> QuestPool { get; set; } = new Dictionary<string, IPandaQuest>();
        public static Dictionary<Colony, HashSet<string>> CompletedQuests { get; set; } = new Dictionary<Colony, HashSet<string>>();
        public static Dictionary<Colony, Dictionary<string, long>> NumberOfQuestsComplete { get; set; } = new Dictionary<Colony, Dictionary<string, long>>();
        public static Dictionary<Colony, HashSet<string>> ActiveQuests { get; set; } = new Dictionary<Colony, HashSet<string>>();

        public int NextUpdateTimeMinMs => 2000;

        public int NextUpdateTimeMaxMs => 4000;

        public ServerTimeStamp NextUpdateTime { get; set; }

        public void OnConstructInventoryManageColonyUI(Players.Player player, NetworkMenu networkMenu, (Table, Table) table)
        {
            if (QuestPool.Count != 0 && player.ActiveColony != null)
                table.Item1.Rows.Add(new ButtonCallback(GameInitializer.NAMESPACE + ".QuestingMainMenu", new LabelData(_localizationHelper.LocalizeOrDefault("Quests", player)), 200));
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerPushedNetworkUIButton, GameInitializer.NAMESPACE + ".ColonyManager.ColonyTool.PressButton")]
        public static void PressButton(ButtonPressCallbackData data)
        {
            if (!data.ButtonIdentifier.Contains(".QuestingMainMenu") || data.Player.ActiveColony == null)
                return;

            NetworkMenu menu = new NetworkMenu();
            menu.LocalStorage.SetAs("header", _localizationHelper.LocalizeOrDefault("Quests", data.Player));
            menu.Width = 1000;
            menu.Height = 600;
            menu.ForceClosePopups = true;
            menu.Items.Add(new Label(new LabelData(_localizationHelper.LocalizeOrDefault("ActiveQuests", data.Player), UnityEngine.Color.white, UnityEngine.TextAnchor.LowerLeft, 30)));

            if (!ActiveQuests.ContainsKey(data.Player.ActiveColony))
                ActiveQuests.Add(data.Player.ActiveColony, new HashSet<string>());

            foreach (var questKey in ActiveQuests[data.Player.ActiveColony])
            {
                if (QuestPool.TryGetValue(questKey, out var quest))
                {
                    menu.Items.Add(new Line());
                    menu.Items.Add(new Label(new LabelData(quest.GetQuestTitle(data.Player.ActiveColony, data.Player), UnityEngine.Color.white, UnityEngine.TextAnchor.LowerLeft, 26)));
                    menu.Items.Add(new Label(new LabelData(quest.GetQuestText(data.Player.ActiveColony, data.Player), UnityEngine.Color.white)));
                    menu.Items.Add(new Label(new LabelData(_localizationHelper.LocalizeOrDefault("Objectives", data.Player), UnityEngine.Color.white, UnityEngine.TextAnchor.LowerLeft, 20)));
                    
                    foreach (var req in quest.QuestObjectives)
                    {
                        var questObj = new List<ValueTuple<IItem, int>>();
                        questObj.Add(ValueTuple.Create<IItem, int>(new Label(new LabelData(req.Value.GetObjectiveProgressText(quest, data.Player.ActiveColony, data.Player), UnityEngine.Color.white)), 700));
                        questObj.Add(ValueTuple.Create<IItem, int>(new Label(new LabelData(req.Value.GetProgress(quest, data.Player.ActiveColony) * 100 + "%", UnityEngine.Color.white)), 200));
                        menu.Items.Add(new HorizontalRow(questObj));
                    }

                    menu.Items.Add(new Label(new LabelData(_localizationHelper.LocalizeOrDefault("Rewards", data.Player), UnityEngine.Color.white, UnityEngine.TextAnchor.LowerLeft, 20)));

                    foreach (var reward in quest.QuestRewards)
                        menu.Items.Add(new Label(new LabelData(reward.GetRewardText(quest, data.Player.ActiveColony, data.Player), UnityEngine.Color.white)));
                }
            }

            menu.Items.Add(new Label(new LabelData(_localizationHelper.LocalizeOrDefault("RequrementsNotMet", data.Player), UnityEngine.Color.white, UnityEngine.TextAnchor.LowerLeft, 30)));

            foreach (var qpq in QuestPool.OrderBy(key => key.Key))
            {
                if (!ActiveQuests[data.Player.ActiveColony].Contains(qpq.Key))
                {
                    var quest = qpq.Value;

                    menu.Items.Add(new Line());
                    menu.Items.Add(new Label(new LabelData(quest.GetQuestTitle(data.Player.ActiveColony, data.Player), UnityEngine.Color.white, UnityEngine.TextAnchor.LowerLeft, 26)));
                    menu.Items.Add(new Label(new LabelData(quest.GetQuestText(data.Player.ActiveColony, data.Player), UnityEngine.Color.white)));

                    foreach (var req in quest.QuestPrerequisites)
                    {
                        var questObj = new List<ValueTuple<IItem, int>>();
                        questObj.Add(ValueTuple.Create<IItem, int>(new Label(new LabelData(req.GetPrerequisiteText(quest, data.Player.ActiveColony, data.Player), UnityEngine.Color.white)), 700));
                        questObj.Add(ValueTuple.Create<IItem, int>(new Label(new LabelData(_localizationHelper.LocalizeOrDefault("Done", data.Player) + ": " + req.MeetsPrerequisite(quest, data.Player.ActiveColony), UnityEngine.Color.white)), 200));
                        menu.Items.Add(new HorizontalRow(questObj));
                    }

                    menu.Items.Add(new Label(new LabelData(_localizationHelper.LocalizeOrDefault("Rewards", data.Player), UnityEngine.Color.white, UnityEngine.TextAnchor.LowerLeft, 20)));

                    foreach (var reward in quest.QuestRewards)
                        menu.Items.Add(new Label(new LabelData(reward.GetRewardText(quest, data.Player.ActiveColony, data.Player), UnityEngine.Color.white)));
                }
            }
        }

        public void OnLoadingColony(Colony c, JSONNode n)
        {
            if (!CompletedQuests.ContainsKey(c))
                CompletedQuests.Add(c, new HashSet<string>());

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
                CompletedQuests.Add(c, new HashSet<string>());

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
                        CompletedQuests.Add(colony, new HashSet<string>());

                    if (!NumberOfQuestsComplete.ContainsKey(colony))
                        NumberOfQuestsComplete.Add(colony, new Dictionary<string, long>());

                    if (!ActiveQuests.ContainsKey(colony))
                        ActiveQuests.Add(colony, new HashSet<string>());

                    bool ok = true;

                    if (!ActiveQuests[colony].Contains(quest.Key))
                        foreach (var pre in quest.Value.QuestPrerequisites)
                            if (!pre.MeetsPrerequisite(quest.Value, colony))
                                ok = false;
                            else
                            {
                                ActiveQuests[colony].Add(quest.Key);
                                PandaChat.Send(colony, _localizationHelper, "NewQuestAvailable", quest.Value.GetQuestTitle(colony, colony.Owners.FirstOrDefault()));
                            }

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

                            if (!NumberOfQuestsComplete[colony].ContainsKey(quest.Key))
                                NumberOfQuestsComplete[colony].Add(quest.Key, 1);
                            else
                                NumberOfQuestsComplete[colony][quest.Key] = NumberOfQuestsComplete[colony][quest.Key] + 1;

                            if (!quest.Value.CanRepeat(colony) && !CompletedQuests[colony].Contains(quest.Value.QuestKey))
                                CompletedQuests[colony].Add(quest.Value.QuestKey);
                        }
                    }
                }
            }
        }
    }
}
