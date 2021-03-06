﻿using Chatting;
using Jobs;
using ModLoaderInterfaces;
using NetworkUI;
using NetworkUI.Items;
using NPC;
using Pandaros.API.Entities;
using Pandaros.API.Extender;
using Pandaros.API.Items;
using Pandaros.API.Models;
using Pipliz;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Pandaros.API.Items.StaticItems;

namespace Pandaros.API.ColonyManagement
{
    public class ColonyManagementTool : CSType
    {
        public static string NAME = GameInitializer.NAMESPACE + ".ColonyManagementTool";
        public override string name => NAME;
        public override string icon => GameInitializer.ICON_PATH + "ColonyManager.png";
        public override bool? isPlaceable => false;
        public override int? maxStackSize => 1;
        public override List<string> categories { get; set; } = new List<string>()
        {
            "essential",
            "aaa"
        };
        public override StaticItem StaticItemSettings => new StaticItem() { Name = GameInitializer.NAMESPACE + ".ColonyManagementTool" };
    }

    public class JobCounts
    {
        public string Name { get; set; }
        public int AvailableCount { get; set; }
        public int TakenCount { get; set; }
        public List<IJob> AvailableJobs { get; set; } = new List<IJob>();
        public List<IJob> TakenJobs { get; set; } = new List<IJob>();
    }


    [ModLoader.ModManager]
    public class ColonyTool : IOnConstructInventoryManageColonyUI
    {
        public static List<string> _recruitCount = new List<string>()
        {
            "1",
            "5",
            "10",
            "Max"
        };

        public void OnConstructInventoryManageColonyUI(Players.Player player, NetworkMenu menu, (Table, Table) table)
        {
            if (player.ActiveColony != null)
                table.Item1.Rows.Add(new ButtonCallback(GameInitializer.NAMESPACE + ".ColonyToolMainMenu", new LabelData(_localizationHelper.LocalizeOrDefault("ColonyManagement", player)), 200));
        }

        static readonly Pandaros.API.localization.LocalizationHelper _localizationHelper = new localization.LocalizationHelper(GameInitializer.NAMESPACE, "colonytool");

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerClicked, GameInitializer.NAMESPACE + ".ColonyManager.ColonyTool.OpenMenu")]
        public static void OpenMenu(Players.Player player, PlayerClickedData playerClickData)
        {
            //Only launch on RIGHT click
            if (player == null || playerClickData.ClickType != PlayerClickedData.EClickType.Right || player.ActiveColony == null)
                return;
           
            if (ItemTypes.IndexLookup.TryGetIndex(GameInitializer.NAMESPACE + ".ColonyManagementTool", out var toolItem) &&
                playerClickData.TypeSelected == toolItem)
            {
                Dictionary<string, JobCounts> jobCounts = GetJobCounts(player.ActiveColony);
                NetworkMenuManager.SendServerPopup(player, BuildMenu(player, jobCounts, false, string.Empty, 0));
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerPushedNetworkUIButton, GameInitializer.NAMESPACE + ".ColonyManager.ColonyTool.PressButton")]
        public static void PressButton(ButtonPressCallbackData data)
        {
            if ((!data.ButtonIdentifier.Contains(".RecruitButton") &&
                !data.ButtonIdentifier.Contains(".FireButton") &&
                !data.ButtonIdentifier.Contains(".MoveFired") &&
                !data.ButtonIdentifier.Contains(".ColonyToolMainMenu") &&
                !data.ButtonIdentifier.Contains(".KillFired") &&
                !data.ButtonIdentifier.Contains(".CallToArms")) || data.Player.ActiveColony == null)
                return;

            Dictionary<string, JobCounts> jobCounts = GetJobCounts(data.Player.ActiveColony);

            if (data.ButtonIdentifier.Contains(".ColonyToolMainMenu"))
            {
                NetworkMenuManager.SendServerPopup(data.Player, BuildMenu(data.Player, jobCounts, false, string.Empty, 0));
            }
            else if (data.ButtonIdentifier.Contains(".FireButton"))
            {
                foreach (var job in jobCounts)
                    if (data.ButtonIdentifier.Contains(job.Key))
                    {
                        var recruit = data.Storage.GetAs<int>(job.Key + ".Recruit");
                        var count = GetCountValue(recruit);
                        var menu = BuildMenu(data.Player, jobCounts, true, job.Key, count);

                        menu.LocalStorage.SetAs(GameInitializer.NAMESPACE + ".FiredJobName", job.Key);
                        menu.LocalStorage.SetAs(GameInitializer.NAMESPACE + ".FiredJobCount", count);

                        NetworkMenuManager.SendServerPopup(data.Player, menu);
                        break;
                    }
            }
            else if (data.ButtonIdentifier.Contains(".KillFired"))
            {
                var firedJob = data.Storage.GetAs<string>(GameInitializer.NAMESPACE + ".FiredJobName");
                var count = data.Storage.GetAs<int>(GameInitializer.NAMESPACE + ".FiredJobCount");

                foreach (var job in jobCounts)
                {
                    if (job.Key == firedJob)
                    {
                        if (count > job.Value.TakenCount)
                            count = job.Value.TakenCount;

                        for (int i = 0; i < count; i++)
                        {
                            var npc = job.Value.TakenJobs[i].NPC;
                            npc.ClearJob();
                            npc.OnDeath();
                        }

                        break;
                    }
                }

                data.Player.ActiveColony.SendCommonData();
                jobCounts = GetJobCounts(data.Player.ActiveColony);
                NetworkMenuManager.SendServerPopup(data.Player, BuildMenu(data.Player, jobCounts, false, string.Empty, 0));
            }
            else if (data.ButtonIdentifier.Contains(".MoveFired"))
            {
                var firedJob = data.Storage.GetAs<string>(GameInitializer.NAMESPACE + ".FiredJobName");
                var count = data.Storage.GetAs<int>(GameInitializer.NAMESPACE + ".FiredJobCount");

                foreach (var job in jobCounts)
                    if (data.ButtonIdentifier.Contains(job.Key))
                    {
                        if (count > job.Value.AvailableCount)
                            count = job.Value.AvailableCount;

                        if (jobCounts.TryGetValue(firedJob, out var firedJobCounts))
                        {
                            for (int i = 0; i < count; i++)
                            {
                                if (firedJobCounts.TakenCount > i)
                                {
                                    var npc = firedJobCounts.TakenJobs[i].NPC;
                                    npc.ClearJob();
                                    npc.TakeJob(job.Value.AvailableJobs[i]);
                                }
                                else
                                    break;
                            }
                        }

                        data.Player.ActiveColony.SendCommonData();
                        break;
                    }

                jobCounts = GetJobCounts(data.Player.ActiveColony);
                NetworkMenuManager.SendServerPopup(data.Player, BuildMenu(data.Player, jobCounts, false, string.Empty, 0));
            }
            else if (data.ButtonIdentifier.Contains(".RecruitButton"))
            {
                foreach (var job in jobCounts)
                    if (data.ButtonIdentifier.Contains(job.Key))
                    {
                        var recruit = data.Storage.GetAs<int>(job.Key + ".Recruit");
                        var count = GetCountValue(recruit);

                        if (count > job.Value.AvailableCount)
                            count = job.Value.AvailableCount;

                        for (int i = 0; i < count; i++)
                        {
                           
                            if (data.Player.ActiveColony.Followers.Count >= data.Player.ActiveColony.ColonistCap)
                            {
                                Chat.Send(data.Player, Localization.GetSentence(data.Player.LastKnownLocale, "colonymanagement.recruitfailedcap"));
                                break;
                            }
                            if (data.Player.ActiveColony.CalculateBedCount() <= data.Player.ActiveColony.Followers.Count)
                            {
                                Chat.Send(data.Player, Localization.GetSentence(data.Player.LastKnownLocale, "colonymanagement.recruitfailedbeds"));
                                break;
                            }
                            if (data.Player.ActiveColony.Followers.Count >= ServerManager.ServerSettings.NPCs.FreeNPCs && !data.Player.ActiveColony.Stockpile.TryTakeMeal(ServerManager.ServerSettings.NPCs.RecruitmentCost))
                            {
                                Chat.Send(data.Player, Localization.GetSentence(data.Player.LastKnownLocale, "colonymanagement.recruitfailednomeals"));
                                break;
                            }

                            var newGuy = new NPCBase(data.Player.ActiveColony, data.Player.ActiveColony.GetClosestBanner(new Vector3Int(data.Player.Position)).Position);
                            data.Player.ActiveColony.RegisterNPC(newGuy);
                            ColonistInventory.Get(newGuy);
                            NPCTracker.Add(newGuy);
                            ModLoader.Callbacks.OnNPCRecruited.Invoke(newGuy);

                            if (newGuy.IsValid)
                            {
                                newGuy.TakeJob(job.Value.AvailableJobs[i]);
                            }
                        }


                        data.Player.ActiveColony.SendCommonData();

                        jobCounts = GetJobCounts(data.Player.ActiveColony);
                        NetworkMenuManager.SendServerPopup(data.Player, BuildMenu(data.Player, jobCounts, false, string.Empty, 0));
                    }
            }
        }

        public static int GetCountValue(int countIndex)
        {
            var value = _recruitCount[countIndex];
            int retval = int.MaxValue;

            if (int.TryParse(value, out int count))
                retval = count;

            return retval;
        }

        public static NetworkMenu BuildMenu(Players.Player player, Dictionary<string, JobCounts> jobCounts, bool fired, string firedName, int firedCount)
        {
            NetworkMenu menu = new NetworkMenu();
            menu.LocalStorage.SetAs("header", _localizationHelper.LocalizeOrDefault("ColonyManagement", player));
            menu.Width = 1000;
            menu.Height = 600;
            menu.ForceClosePopups = true;

            if (fired)
            {
                var count = firedCount.ToString();

                if (firedCount == int.MaxValue)
                    count = "all";

                menu.Items.Add(new ButtonCallback(GameInitializer.NAMESPACE + ".KillFired", new LabelData($"{_localizationHelper.LocalizeOrDefault("Kill", player)} {count} {_localizationHelper.LocalizeOrDefault("Fired", player)} {firedName}", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter)));
            }

            menu.Items.Add(new Line());


            List<ValueTuple<IItem, int>> header = new List<ValueTuple<IItem, int>>();

            header.Add(ValueTuple.Create<IItem, int>(new Label(new LabelData(_localizationHelper.GetLocalizationKey("Job"), UnityEngine.Color.white)), 140));

            if (!fired)
                header.Add(ValueTuple.Create<IItem, int>(new Label(new LabelData("", UnityEngine.Color.white)), 140));

            header.Add(ValueTuple.Create<IItem, int>(new Label(new LabelData(_localizationHelper.GetLocalizationKey("Working"), UnityEngine.Color.white)), 140));
            header.Add(ValueTuple.Create<IItem, int>(new Label(new LabelData(_localizationHelper.GetLocalizationKey("NotWorking"), UnityEngine.Color.white)), 140));
            header.Add(ValueTuple.Create<IItem, int>(new Label(new LabelData("", UnityEngine.Color.white)), 140));
            header.Add(ValueTuple.Create<IItem, int>(new Label(new LabelData("", UnityEngine.Color.white)), 140));

            menu.Items.Add(new HorizontalRow(header));
            int jobCount = 0;

            foreach (var jobKvp in jobCounts)
            {
                if (fired && jobKvp.Value.AvailableCount == 0)
                    continue;

                jobCount++;
                List<ValueTuple<IItem, int>> items = new List<ValueTuple<IItem, int>>();

                items.Add(ValueTuple.Create<IItem, int>(new Label(new LabelData(_localizationHelper.LocalizeOrDefault(jobKvp.Key.Replace(" ", ""), player), UnityEngine.Color.white)), 140));

                if (!fired)
                    items.Add(ValueTuple.Create<IItem, int>(new ButtonCallback(jobKvp.Key + ".JobDetailsButton", new LabelData(_localizationHelper.GetLocalizationKey("Details"), UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter), 140), 140));

                items.Add(ValueTuple.Create<IItem, int>(new Label(new LabelData(jobKvp.Value.TakenCount.ToString(), UnityEngine.Color.white)), 140));
                items.Add(ValueTuple.Create<IItem, int>(new Label(new LabelData(jobKvp.Value.AvailableCount.ToString(), UnityEngine.Color.white)), 140));

                if (fired)
                {
                    items.Add(ValueTuple.Create<IItem, int>(new ButtonCallback(jobKvp.Key + ".MoveFired", new LabelData(_localizationHelper.GetLocalizationKey("MoveFired"), UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleLeft), 140), 140));
                }
                else
                {
                    items.Add(ValueTuple.Create<IItem, int>(new DropDown(new LabelData(_localizationHelper.GetLocalizationKey("Amount"), UnityEngine.Color.white), jobKvp.Key + ".Recruit", _recruitCount), 170));
                    items.Add(ValueTuple.Create<IItem, int>(new ButtonCallback(jobKvp.Key + ".RecruitButton", new LabelData(_localizationHelper.GetLocalizationKey("Recruit"), UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter), 100), 100));
                    items.Add(ValueTuple.Create<IItem, int>(new ButtonCallback(jobKvp.Key + ".FireButton", new LabelData(_localizationHelper.GetLocalizationKey("Fire"), UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter), 100), 100));
                    
                }

                menu.LocalStorage.SetAs(jobKvp.Key + ".Recruit", 0);

                menu.Items.Add(new HorizontalRow(items));
            }

            if (jobCount == 0)
                menu.Items.Add(new Label(new LabelData(_localizationHelper.LocalizeOrDefault("NoJobs", player), UnityEngine.Color.white)));

            return menu;
        }

        public static Dictionary<string, JobCounts> GetJobCounts(Colony colony)
        {
            Dictionary<string, JobCounts> jobCounts = new Dictionary<string, JobCounts>();
            var jobs = colony?.JobFinder?.JobsData.PerJobData;
            var npcs = colony?.Followers;

            if (jobs != null)
                foreach (var job in jobs)
                {
                    if (NPCType.NPCTypes.TryGetValue(job.Key, out var nPCTypeSettings))
                    {
                        if (!jobCounts.ContainsKey(nPCTypeSettings.KeyName))
                            jobCounts.Add(nPCTypeSettings.KeyName, new JobCounts() { Name = nPCTypeSettings.KeyName });

                        for (var i = 0; i < job.Value.JobCount; i++)
                        {
                            if (job.Value.Jobs[i].NPC == null)
                            {
                                jobCounts[nPCTypeSettings.KeyName].AvailableCount++;
                                jobCounts[nPCTypeSettings.KeyName].AvailableJobs.Add(job.Value.Jobs[i]);
                            }
                        }
                    }
                }


            if (npcs != null)
                foreach (var npc in npcs)
                {
                    if (npc.Job != null && npc.Job.IsValid && NPCType.NPCTypes.TryGetValue(npc.Job.NPCType, out var nPCTypeSettings))
                    {
                        if (!jobCounts.ContainsKey(nPCTypeSettings.KeyName))
                            jobCounts.Add(nPCTypeSettings.KeyName, new JobCounts() { Name = nPCTypeSettings.KeyName });

                        jobCounts[nPCTypeSettings.KeyName].TakenCount++;
                        jobCounts[nPCTypeSettings.KeyName].TakenJobs.Add(npc.Job);
                    }
                }

            var l = jobCounts.OrderBy(key => key.Key);

            return l.ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);
        }
    }
}

