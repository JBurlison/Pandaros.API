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
using System.Media;
using UnityEngine.UI;
using Pandaros.API.Gui;
using System.Net.Http.Headers;

namespace Pandaros.API.Questing
{
    public class QuestingSystem : IOnTimedUpdate, IOnConstructInventoryManageColonyUI, IOnLoadingColony, IOnSavingColony, IOnPlayerPushedNetworkUIButton
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

        public void OnPlayerPushedNetworkUIButton(ButtonPressCallbackData data)
        {
            if (!data.ButtonIdentifier.Contains(".QuestingMainMenu") || data.Player.ActiveColony == null)
                return;

            NetworkMenu menu = new NetworkMenu();
            menu.LocalStorage.SetAs("header", _localizationHelper.LocalizeOrDefault("Quests", data.Player));
            menu.Width = 1000;
            menu.Height = 600;
            menu.SpacingBetweenItems = 0;
            menu.ForceClosePopups = true;
           
            menu.Items.Add(new HorizontalRow(new List<(IItem, int)>()
            {
                (new ButtonCallback(GameInitializer.NAMESPACE + ".QuestingMainMenu", new LabelData(_localizationHelper.LocalizeOrDefault("ActiveQuests", data.Player))), 310),
                (new ButtonCallback(GameInitializer.NAMESPACE + ".QuestingMainMenuInactive", new LabelData(_localizationHelper.LocalizeOrDefault("RequrementsNotMet", data.Player))), 310),
                (new ButtonCallback(GameInitializer.NAMESPACE + ".QuestingMainMenuCompleted", new LabelData(_localizationHelper.LocalizeOrDefault("Done", data.Player))), 310)
            }));

            if (!ActiveQuests.ContainsKey(data.Player.ActiveColony))
                ActiveQuests.Add(data.Player.ActiveColony, new HashSet<string>());

            if (!CompletedQuests.ContainsKey(data.Player.ActiveColony))
                CompletedQuests.Add(data.Player.ActiveColony, new HashSet<string>());

            if (data.ButtonIdentifier == GameInitializer.NAMESPACE + ".QuestingMainMenu")
            {
                menu.Items.Add(new Label(new LabelData(_localizationHelper.LocalizeOrDefault("ActiveQuests", data.Player), UnityEngine.Color.white, UnityEngine.TextAnchor.LowerLeft, 30)));

                foreach (var questKey in ActiveQuests[data.Player.ActiveColony])
                {
                    if (QuestPool.TryGetValue(questKey, out var quest))
                    {
                        AddQuestDivider(menu);
                        menu.Items.Add(AddBackground(new HorizontalRow(new List<(IItem, int)>()
                            {
                                (new ItemIcon(quest.ItemIconName), 80),
                                (new Label(new LabelData(quest.GetQuestTitle(data.Player.ActiveColony, data.Player), UnityEngine.Color.white, UnityEngine.TextAnchor.LowerLeft, 26)), 920)
                            })));
                        menu.Items.Add(AddBackground(new EmptySpace(5)));
                        menu.Items.Add(AddBackground(new Label(new LabelData(quest.GetQuestText(data.Player.ActiveColony, data.Player), UnityEngine.Color.white))));

                        AddQuestDescriptionDivider(menu);
                        menu.Items.Add(AddBackground(new Label(new LabelData(_localizationHelper.LocalizeOrDefault("Objectives", data.Player), UnityEngine.Color.white, UnityEngine.TextAnchor.LowerLeft, 20))));

                        if (quest.QuestObjectives != null)
                            foreach (var req in quest.QuestObjectives)
                            {
                                var questObj = new List<ValueTuple<IItem, int>>();
                                questObj.Add(ValueTuple.Create<IItem, int>(new Label(new LabelData(req.Value.GetObjectiveProgressText(quest, data.Player.ActiveColony, data.Player), UnityEngine.Color.white)), 700));
                                questObj.Add(ValueTuple.Create<IItem, int>(new Label(new LabelData(Math.Round(req.Value.GetProgress(quest, data.Player.ActiveColony), 2) * 100 + "%", UnityEngine.Color.white)), 200));
                                menu.Items.Add(AddBackground(new HorizontalRow(questObj)));
                            }

                        AddQuestDescriptionDivider(menu);
                        menu.Items.Add(AddBackground(new Label(new LabelData(_localizationHelper.LocalizeOrDefault("Rewards", data.Player), UnityEngine.Color.white, UnityEngine.TextAnchor.LowerLeft, 20))));

                        if (quest.QuestRewards != null)
                            foreach (var reward in quest.QuestRewards)
                            {
                                var itemList = new List<ValueTuple<IItem, int>>();
                                itemList.Add((new ItemIcon(reward.ItemIconName), 100));
                                itemList.Add((new Label(new LabelData(reward.GetRewardText(quest, data.Player.ActiveColony, data.Player), UnityEngine.Color.white)), 800));
                                menu.Items.Add(AddBackground(new HorizontalRow(itemList)));
                            }

                        menu.Items.Add(AddBackground(new EmptySpace(5)));
                    }
                }
            }

            if (data.ButtonIdentifier.Contains(".QuestingMainMenuInactive"))
            {
                menu.Items.Add(new Label(new LabelData(_localizationHelper.LocalizeOrDefault("RequrementsNotMet", data.Player), UnityEngine.Color.white, UnityEngine.TextAnchor.LowerLeft, 30)));

                foreach (var qpq in QuestPool.OrderBy(key => key.Key))
                {
                    if (!ActiveQuests[data.Player.ActiveColony].Contains(qpq.Key) &&
                        !CompletedQuests[data.Player.ActiveColony].Contains(qpq.Key) &&
                        !qpq.Value.HideQuest)
                    {
                        var quest = qpq.Value;
                        AddQuestDivider(menu);
                        menu.Items.Add(AddBackground(new HorizontalRow(new List<(IItem, int)>()
                            {
                                (new ItemIcon(quest.ItemIconName), 80),
                                (new Label(new LabelData(quest.GetQuestTitle(data.Player.ActiveColony, data.Player), UnityEngine.Color.white, UnityEngine.TextAnchor.LowerLeft, 26)), 920)
                            })));

                        menu.Items.Add(AddBackground(new EmptySpace(5)));
                        menu.Items.Add(AddBackground(new Label(new LabelData(quest.GetQuestText(data.Player.ActiveColony, data.Player), UnityEngine.Color.white))));

                        AddQuestDescriptionDivider(menu);
                        if (quest.QuestPrerequisites != null)
                            foreach (var req in quest.QuestPrerequisites)
                            {
                                var questObj = new List<ValueTuple<IItem, int>>();
                                questObj.Add(ValueTuple.Create<IItem, int>(new Label(new LabelData(req.GetPrerequisiteText(quest, data.Player.ActiveColony, data.Player), UnityEngine.Color.white)), 700));
                                questObj.Add(ValueTuple.Create<IItem, int>(new Label(new LabelData(_localizationHelper.LocalizeOrDefault("Done", data.Player) + ": " + req.MeetsPrerequisite(quest, data.Player.ActiveColony), UnityEngine.Color.white)), 200));
                                menu.Items.Add(AddBackground(new HorizontalRow(questObj)));
                            }


                        AddQuestDescriptionDivider(menu);
                        menu.Items.Add(AddBackground(new Label(new LabelData(_localizationHelper.LocalizeOrDefault("Rewards", data.Player), UnityEngine.Color.white, UnityEngine.TextAnchor.LowerLeft, 20))));

                        foreach (var reward in quest.QuestRewards)
                            menu.Items.Add(AddBackground(new Label(new LabelData(reward.GetRewardText(quest, data.Player.ActiveColony, data.Player), UnityEngine.Color.white))));

                        menu.Items.Add(AddBackground(new EmptySpace(5)));
                    }
                }
            }

            if (data.ButtonIdentifier.Contains(".QuestingMainMenuCompleted"))
            {
                menu.Items.Add(new Label(new LabelData(_localizationHelper.LocalizeOrDefault("Done", data.Player), UnityEngine.Color.white, UnityEngine.TextAnchor.LowerLeft, 30)));

                foreach (var questKey in CompletedQuests[data.Player.ActiveColony])
                {
                    if (QuestPool.TryGetValue(questKey, out var quest))
                    {
                        AddQuestDivider(menu);
                        menu.Items.Add(AddBackground(new HorizontalRow(new List<(IItem, int)>()
                            {
                                (new ItemIcon(quest.ItemIconName), 80),
                                (new Label(new LabelData(quest.GetQuestTitle(data.Player.ActiveColony, data.Player), UnityEngine.Color.white, UnityEngine.TextAnchor.LowerLeft, 26)), 920)
                            })));

                        menu.Items.Add(AddBackground(new EmptySpace(5)));
                        menu.Items.Add(AddBackground(new Label(new LabelData(quest.GetQuestText(data.Player.ActiveColony, data.Player), UnityEngine.Color.white))));

                        AddQuestDescriptionDivider(menu);
                        menu.Items.Add(AddBackground(new Label(new LabelData(_localizationHelper.LocalizeOrDefault("Rewards", data.Player), UnityEngine.Color.white, UnityEngine.TextAnchor.LowerLeft, 20))));

                        if (quest.QuestRewards != null)
                            foreach (var reward in quest.QuestRewards)
                            {
                                var itemList = new List<ValueTuple<IItem, int>>();
                                itemList.Add((new ItemIcon(reward.ItemIconName), 100));
                                itemList.Add((new Label(new LabelData(reward.GetRewardText(quest, data.Player.ActiveColony, data.Player), UnityEngine.Color.white)), 800));
                                menu.Items.Add(AddBackground(new HorizontalRow(itemList)));
                            }

                        menu.Items.Add(AddBackground(new EmptySpace(5)));
                    }
                }
            }

            NetworkMenuManager.SendServerPopup(data.Player, menu);
        }

        private static void AddQuestDivider(NetworkMenu menu)
        {
           menu.Items.Add(new EmptySpace(10));
            //menu.Items.Add(new Line(UnityEngine.Color.white, 2, 800, 100));
           menu.Items.Add(AddBackground(new EmptySpace(5)));
        }

        private static void AddQuestDescriptionDivider(NetworkMenu menu)
        {
            menu.Items.Add(AddBackground(new EmptySpace(15)));
            menu.Items.Add(AddBackground(new Line(new UnityEngine.Color(191, 171, 140), 2, 200, 0)));
            menu.Items.Add(AddBackground(new EmptySpace(10)));
        }

        private static IItem AddBackground(IItem item)
        {
            return new BackgroundColor(item, -1, 30, 0, 0, 4, 0, new UnityEngine.Color32(96, 79, 73, 255));
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
                foreach (var colony in ServerManager.ColonyTracker.ColoniesByID.Values)
                {
                    if (colony == null)
                        return;

                    if (!CompletedQuests.ContainsKey(colony))
                        CompletedQuests.Add(colony, new HashSet<string>());

                    if (!NumberOfQuestsComplete.ContainsKey(colony))
                        NumberOfQuestsComplete.Add(colony, new Dictionary<string, long>());

                    if (!ActiveQuests.ContainsKey(colony))
                        ActiveQuests.Add(colony, new HashSet<string>());

                    bool ok = true;

                    if (!ActiveQuests[colony].Contains(quest.Key))
                    {
                        if (quest.Value.QuestPrerequisites != null)
                        {
                            foreach (var pre in quest.Value.QuestPrerequisites)
                                if (!pre.MeetsPrerequisite(quest.Value, colony))
                                    ok = false;
                        }

                        if (CompletedQuests[colony].Contains(quest.Key))
                        {
                            if (!quest.Value.CanRepeat(colony))
                                ok = false;
                        }

                        if (ok)
                        {
                            ActiveQuests[colony].Add(quest.Key);

                            if (!string.IsNullOrEmpty(quest.Value.QuestAvailableSoundKey))
                            {
                                foreach (var p in colony.Owners)
                                    if (p.IsConnected())
                                        AudioManager.SendAudio(p.Position, quest.Value.QuestAvailableSoundKey);
                            }

                            PandaChat.Send(colony, _localizationHelper, "NewQuestAvailable", quest.Value.GetQuestTitle(colony, colony.Owners.FirstOrDefault()));
                            Notifications.IssueNotification(colony, _localizationHelper, "NewQuestAvailable", quest.Value.GetQuestTitle(colony, colony.Owners.FirstOrDefault()));
                        }
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
                            PandaChat.Send(colony, _localizationHelper, "QuestComplete", quest.Value.GetQuestTitle(colony, colony.Owners.FirstOrDefault()));
                            Notifications.IssueNotification(colony, _localizationHelper, "QuestComplete", quest.Value.GetQuestTitle(colony, colony.Owners.FirstOrDefault()));

                            if (!string.IsNullOrEmpty(quest.Value.QuestCompleteSoundKey))
                            {
                                foreach (var p in colony.Owners)
                                    if (p.IsConnected())
                                        AudioManager.SendAudio(p.Position, quest.Value.QuestCompleteSoundKey);
                            }

                            if (quest.Value.QuestRewards != null)
                                foreach (var reward in quest.Value.QuestRewards)
                                    reward.IssueReward(quest.Value, colony);

                            if (!NumberOfQuestsComplete[colony].ContainsKey(quest.Key))
                                NumberOfQuestsComplete[colony].Add(quest.Key, 1);
                            else
                                NumberOfQuestsComplete[colony][quest.Key] = NumberOfQuestsComplete[colony][quest.Key] + 1;

                            if (!quest.Value.CanRepeat(colony) && !CompletedQuests[colony].Contains(quest.Value.QuestKey))
                            {
                                CompletedQuests[colony].Add(quest.Value.QuestKey);
                                ActiveQuests[colony].Remove(quest.Value.QuestKey);
                            }
                        }
                    }
                }
            }
        }
    }
}
