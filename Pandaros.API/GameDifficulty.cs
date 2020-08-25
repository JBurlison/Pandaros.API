using Chatting;
using ModLoaderInterfaces;
using NetworkUI;
using NetworkUI.Items;
using Pandaros.API.ColonyManagement;
using Pandaros.API.Entities;
using Pandaros.API.Extender;
using Pipliz;
using Pipliz.JSON;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pandaros.API
{
    [ModLoader.ModManager]
    public class GameDifficulty
    {
        static localization.LocalizationHelper _localizationHelper = new localization.LocalizationHelper(GameInitializer.NAMESPACE, "GameDifficulty");

        static GameDifficulty()
        {
            GameDifficulties = new Dictionary<string, GameDifficulty>(StringComparer.OrdinalIgnoreCase);

            Normal = new GameDifficulty("Normal", 0, 0f, 0f)
            {
                Rank = 0,
                BossHPPerColonist = 50,
                MonsterHPPerColonist = 0f,
                RoamingJobActionEnergy = 0f
            };

            Easy = new GameDifficulty("Easy", 1, 0f, 10f)
            {
                Rank = 1,
                BossHPPerColonist = 50,
                MonsterHPPerColonist = .2f,
                RoamingJobActionEnergy = .5f
            };

            Medium = new GameDifficulty("Medium", 2, 0f, 50f)
            {
                Rank = 2,
                BossHPPerColonist = 70,
                MonsterHPPerColonist = .5f,
                RoamingJobActionEnergy = 0f
            };

            Hard = new GameDifficulty("Hard", 3, -0.1f, 70f)
            {
                Rank = 3,
                BossHPPerColonist = 80,
                MonsterHPPerColonist = 1f,
                RoamingJobActionEnergy = -.2f
            };

            Insane = new GameDifficulty("Insane", 4, -0.2f, 80f)
            {
                Rank = 4,
                BossHPPerColonist = 100,
                MonsterHPPerColonist = 2f,
                RoamingJobActionEnergy = -.5f
            };
        }

        public GameDifficulty()
        {
        }

        public GameDifficulty(string name, int rank, float monsterDr, float  monsterDamage)
        {
            Name                   = name;
            GameDifficulties[name] = this;
            MonsterDamageReduction = monsterDr;
            MonsterDamage          = monsterDamage;
        }

        public static Dictionary<string, GameDifficulty> GameDifficulties { get; }


        public static GameDifficulty Normal { get; }
        public static GameDifficulty Easy { get; }
        public static GameDifficulty Medium { get; }
        public static GameDifficulty Hard { get; }
        public static GameDifficulty Insane { get; }

        public string Name { get; set; }
        public int Rank { get; set; }

        public float MonsterDamageReduction { get; set; }
        public float MonsterDamage { get; set; }
        public float BossHPPerColonist { get; set; } = 30;
        public float MonsterHPPerColonist { get; set; } = 0;
        public float RoamingJobActionEnergy { get; set; }

        public Dictionary<string, string> StringSettings { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, int> IntSettings { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, double> DoubleSettings { get; set; } = new Dictionary<string, double>();

        public string GetorDefault(string key, string defaultVal)
        {
            if (!StringSettings.ContainsKey(key))
                SetValue(key, defaultVal);

            return StringSettings[key];
        }

        public void SetValue(string key, string val)
        {
            StringSettings[key] = val;
        }

        public double GetorDefault(string key, double defaultVal)
        {
            if (!DoubleSettings.ContainsKey(key))
                SetValue(key, defaultVal);

            return DoubleSettings[key];
        }

        public void SetValue(string key, double val)
        {
            DoubleSettings[key] = val;
        }

        public int GetorDefault(string key, int defaultVal)
        {
            if (!IntSettings.ContainsKey(key))
                SetValue(key, defaultVal);

            return IntSettings[key];
        }

        public void SetValue(string key, int val)
        {
            IntSettings[key] = val;
        }

        public JSONNode ToJson()
        {
            return this.JsonSerialize();
        }

        public override string ToString()
        {
            return Name;
        }
    }

    [ModLoader.ModManager]
    public class GameDifficultyChatCommand : IChatCommand, IOnConstructInventoryManageColonyUI
    {
        private static string _Difficulty = GameInitializer.NAMESPACE + ".Difficulty";
        static localization.LocalizationHelper _localizationHelper = new localization.LocalizationHelper(GameInitializer.NAMESPACE, "GameDifficulty");

        public void OnConstructInventoryManageColonyUI(Players.Player player, NetworkMenu networkMenu, (Table, Table) table)
        {
            if (player.ActiveColony != null)
            {
                networkMenu.Items.Add(new NetworkUI.Items.DropDown("Pandaros.API Difficulty", _Difficulty, GameDifficulty.GameDifficulties.Keys.ToList()));
                var ps = ColonyState.GetColonyState(player.ActiveColony);
                networkMenu.LocalStorage.SetAs(_Difficulty, ps.Difficulty.Rank);
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerChangedNetworkUIStorage, GameInitializer.NAMESPACE + "Difficulty.ChangedSetting")]
        public static void ChangedSetting(ValueTuple<Players.Player, JSONNode, string> data)
        {
            if (data.Item1.ActiveColony != null)
                switch (data.Item3)
                {
                    case "server_popup":
                        var ps = ColonyState.GetColonyState(data.Item1.ActiveColony);

                        if (ps != null && data.Item2.GetAsOrDefault(_Difficulty, ps.Difficulty.Rank) != ps.Difficulty.Rank)
                        {
                            var difficulty = GameDifficulty.GameDifficulties.FirstOrDefault(kvp => kvp.Value.Rank == data.Item2.GetAsOrDefault(_Difficulty, ps.Difficulty.Rank)).Key;

                            if (difficulty != null)
                                ChangeDifficulty(data.Item1, ps, difficulty);
                        }

                        break;
                }
        }

        public bool TryDoCommand(Players.Player player, string chat, List<string> split)
        {
            if (!chat.StartsWith("/difficulty", StringComparison.OrdinalIgnoreCase) ||
                   !chat.StartsWith("/dif", StringComparison.OrdinalIgnoreCase))
                return false;

            if (player == null || player.ID == NetworkID.Server || player.ActiveColony == null)
                return true;

            var array = new List<string>();
            CommandManager.SplitCommand(chat, array);

            var state = ColonyState.GetColonyState(player.ActiveColony);

            if (array.Count == 1)
            {
                PandaChat.Send(player, _localizationHelper, "CurrentDifficulty", ChatColor.green, state.Difficulty.Name);
                return true;
            }

            if (array.Count < 2)
            {
                UnknownCommand(player, chat);
                return true;
            }

            if (array.Count == 2)
            {
                var difficulty = array[1].Trim();

                return ChangeDifficulty(player, state, difficulty);
            }

            if (!APIConfiguration.DifficutlyCanBeChanged)
                PandaChat.Send(player, _localizationHelper, "DifficultyChangeDisabled", ChatColor.green);

            return true;
        }

        public static bool ChangeDifficulty(Players.Player player, ColonyState state, string difficulty)
        {
            if (APIConfiguration.DifficutlyCanBeChanged)
            {
                if (!GameDifficulty.GameDifficulties.ContainsKey(difficulty))
                {
                    UnknownCommand(player, difficulty);
                    return true;
                }

                var newDiff = GameDifficulty.GameDifficulties[difficulty];

                if (newDiff.Rank >= APIConfiguration.MinDifficulty.Rank)
                {
                    state.Difficulty = newDiff;

                    PandaChat.Send(player, _localizationHelper, "CurrentDifficulty", ChatColor.green, state.Difficulty.Name);

                    NetworkUI.NetworkMenuManager.SendColonySettingsUI(player);
                    return true;
                }

                NetworkUI.NetworkMenuManager.SendColonySettingsUI(player);
                PandaChat.Send(player, _localizationHelper, "DisabledBelow", ChatColor.green, APIConfiguration.MinDifficulty.Name);
            }

            return true;
        }

        private static void UnknownCommand(Players.Player player, string command)
        {
            PandaChat.Send(player, _localizationHelper, "UnknownCommand", ChatColor.white, command);
            PossibleCommands(player, ChatColor.white);
        }

        public static void PossibleCommands(Players.Player player, ChatColor color)
        {
            if (player.ActiveColony != null)
            {
                PandaChat.Send(player, _localizationHelper, "CurrentDifficulty", color, ColonyState.GetColonyState(player.ActiveColony).Difficulty.Name);
                PandaChat.Send(player, _localizationHelper, "PossibleCommands", color);

                var diffs = string.Empty;

                foreach (var diff in GameDifficulty.GameDifficulties)
                    diffs += diff.Key + " | ";

                PandaChat.Send(player, _localizationHelper, "/difficulty " + diffs.Substring(0, diffs.Length - 2), color);
            }
        }
    }
}