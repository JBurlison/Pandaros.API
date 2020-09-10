using ModLoaderInterfaces;
using NetworkUI;
using NetworkUI.Items;
using Pandaros.API.Entities;
using Pandaros.API.Extender;
using Pandaros.API.localization;
using Pandaros.API.Tutorials.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pandaros.API.Tutorials
{
    public class TutorialFactory : IOnTimedUpdate, IOnPlayerPushedNetworkUIButton, IOnPlayerConnectedLate
    {
        static LocalizationHelper _localizationHelper = new LocalizationHelper(GameInitializer.NAMESPACE, "Tutorial");
        public static Dictionary<string, ITutorial> Tutorials { get; set; } = new Dictionary<string, ITutorial>();
        public int NextUpdateTimeMinMs => 5000;

        public int NextUpdateTimeMaxMs => 10000;

        ServerTimeStamp IOnTimedUpdate.NextUpdateTime { get; set; }


        public void OnPlayerConnectedLate(Players.Player p)
        {
            CheckforPlayerTutorials(p);
        }

        public static bool TutorialRun(PlayerState playerState, string tutorialName)
        {
            return playerState.Tutorials.TryGetValue(tutorialName, out bool tutorialRun) && tutorialRun;
        }

        public static void SetTutorialRun(PlayerState playerState, string tutorialName, bool run = true)
        {
            playerState.Tutorials[tutorialName] = run;
        }

        public void OnTimedUpdate()
        {
            foreach (var p in Players.PlayerDatabase.Values.Where(pk => pk.IsConnected()))
                CheckforPlayerTutorials(p);
        }

        private static void CheckforPlayerTutorials(Players.Player p)
        {
            var ps = PlayerState.GetPlayerState(p);

            foreach (var tut in Tutorials.Values)
            {
                bool hasCompleted = false;

                if (ps.Tutorials.TryGetValue(tut.Name, out var complete) && complete)
                    hasCompleted = true;

                if (!hasCompleted && (tut.Prerequisites.Count == 0 || tut.Prerequisites.All(pre => pre.MeetsCondition(p))))
                {
                    var menu = tut.ShowTutorial(p);
                    menu.Items.Add(new ButtonCallback(GameInitializer.NAMESPACE + ".cancelalltorials", new LabelData(_localizationHelper.LocalizeOrDefault("CancelTutorials", p)), 200, 30, ButtonCallback.EOnClickActions.ClosePopup));
                    NetworkMenuManager.SendServerPopup(p, menu);
                    break;
                }
            }
        }

        public void OnPlayerPushedNetworkUIButton(ButtonPressCallbackData data)
        {
            if (data.ButtonIdentifier != GameInitializer.NAMESPACE + ".cancelalltorials")
                return;

            var ps = PlayerState.GetPlayerState(data.Player);

            foreach (var tut in Tutorials.Keys)
                ps.Tutorials[tut] = true;
        }
    }
}
