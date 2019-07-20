using Chatting;
using Pandaros.API.ColonyManagement;
using Pandaros.API.Entities;
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

            Normal = new GameDifficulty("Normal", 0f, 0f, 0f, 0f)
            {
                Rank = 0,
                ZombieQueenTargetTeleportHp = 100,
                BossHPPerColonist = 50,
                ZombieQueenTargetTeleportCooldownSeconds = 30,
                AdditionalChance = 0f,
                UnhappinessPerColonistDeath = 0f,
                UnhappyGuardsMultiplyRate = 0f,
                MonsterHPPerColonist = 0f,
                UnhappyColonistsBought = 0f
            };

        }

        public GameDifficulty()
        {
        }

        public GameDifficulty(string name, float foodMultiplier, float roamingJobActionEnergy, float monsterDr,
                              float  monsterDamage)
        {
            Name                   = name;
            FoodMultiplier         = foodMultiplier;
            GameDifficulties[name] = this;
            RoamingJobActionEnergy = roamingJobActionEnergy;
            MonsterDamageReduction = monsterDr;
            MonsterDamage          = monsterDamage;
        }

        public static Dictionary<string, GameDifficulty> GameDifficulties { get; }

        public static GameDifficulty Normal { get; }
        public static bool DifficultyCanBeChanged { get; } = true;

        public string Name { get; set; }
        public int Rank { get; set; }

        public float FoodMultiplier { get; set; }
        public float RoamingJobActionEnergy { get; set; }
        public float MonsterDamageReduction { get; set; }
        public float AdditionalChance { get; set; }
        public float MonsterDamage { get; set; }
        public float ZombieQueenTargetTeleportHp { get; set; } = 250;
        public float ZombieQueenTargetTeleportCooldownSeconds { get; set; } = 45;
        public float BossHPPerColonist { get; set; } = 30;
        public float MonsterHPPerColonist { get; set; } = 1;
        public double UnhappinessPerColonistDeath { get; set; } = 2;
        public float UnhappyGuardsMultiplyRate { get; set; } = 1;
        public float UnhappyColonistsBought { get; set; } = -1;

        public Dictionary<string, string> StringSettings { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, int> IntSettings { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, double> DoubleSettings { get; set; } = new Dictionary<string, double>();

        public void Print(Players.Player player)
        {
            PandaChat.Send(player, _localizationHelper, "FoodMultiplier", ChatColor.green, FoodMultiplier.ToString());
            PandaChat.Send(player, _localizationHelper, "MonsterDamage", ChatColor.green, MonsterDamage.ToString());
            PandaChat.Send(player, _localizationHelper, "MonsterDamageReduction", ChatColor.green, MonsterDamageReduction.ToString());
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
    public class GameDifficultyChatCommand : IChatCommand
    {
        private static string _Difficulty = GameInitializer.NAMESPACE + ".Difficulty";
        static localization.LocalizationHelper _localizationHelper = new localization.LocalizationHelper(GameInitializer.NAMESPACE, "GameDifficulty");

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnConstructWorldSettingsUI, GameInitializer.NAMESPACE + "Difficulty.AddSetting")]
        public static void AddSetting(Players.Player player, NetworkUI.NetworkMenu menu)
        {
            if (player.ActiveColony != null)
            {
                menu.Items.Add(new NetworkUI.Items.DropDown("Settlers Difficulty", _Difficulty, GameDifficulty.GameDifficulties.Keys.ToList()));
                var ps = ColonyState.GetColonyState(player.ActiveColony);
                menu.LocalStorage.SetAs(_Difficulty, ps.Difficulty.Rank);
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

            if (!GameDifficulty.DifficultyCanBeChanged)
                PandaChat.Send(player, _localizationHelper, "DifficultyChangeDisabled", ChatColor.green);

            return true;
        }

        public static bool ChangeDifficulty(Players.Player player, ColonyState state, string difficulty)
        {
            if (GameDifficulty.DifficultyCanBeChanged)
            {
                if (!GameDifficulty.GameDifficulties.ContainsKey(difficulty))
                {
                    UnknownCommand(player, difficulty);
                    return true;
                }

                var newDiff = GameDifficulty.GameDifficulties[difficulty];

                state.Difficulty = newDiff;
                state.Difficulty.Print(player);

                PandaChat.Send(player, _localizationHelper, "CurrentDifficulty", ChatColor.green, state.Difficulty.Name);

                NetworkUI.NetworkMenuManager.SendColonySettingsUI(player);
                return true;
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