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
    public class TutorialFactory : IOnTimedUpdate
    {
        static LocalizationHelper _localizationHelper = new LocalizationHelper(GameInitializer.NAMESPACE, "Tutorial");
        public static Dictionary<string, ITutorial> Tutorials { get; set; } = new Dictionary<string, ITutorial>();
        public int NextUpdateTimeMinMs => 5000;

        public int NextUpdateTimeMaxMs => 10000;

        ServerTimeStamp IOnTimedUpdate.NextUpdateTime { get; set; }


        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerConnectedLate, GameInitializer.NAMESPACE + ".Help.TutorialManager.OnPlayerConnectedLate")]
        public static void OnPlayerConnectedLate(Players.Player p)
        {
            var ps = PlayerState.GetPlayerState(p);

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
            {
                var ps = PlayerState.GetPlayerState(p);

                foreach (var tut in Tutorials.Values)
                {
                    bool hasCompleted = false;

                    if (ps.Tutorials.TryGetValue(tut.Name, out var complete) && complete)
                        hasCompleted = true;

                    if (!hasCompleted && tut.Prerequisites.All(pre => pre.MeetsCondition(p)))
                    {
                        NetworkMenuManager.SendServerPopup(p, tut.ShowTutorial(p));
                    }
                }
            }
        }
    }
}
