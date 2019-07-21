using Chatting;
using Pandaros.API.Entities;
using Pipliz;
using Pipliz.JSON;
using System;
using System.Collections.Generic;

namespace Pandaros.API.Monsters
{
    [ModLoader.ModManager]
    public class BossesChatCommand : IChatCommand
    {
        private static string _Bosses = GameInitializer.NAMESPACE + ".Bosses";
        private static localization.LocalizationHelper _localizationHelper = new localization.LocalizationHelper(GameInitializer.NAMESPACE, "Bosses");

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnConstructWorldSettingsUI, GameInitializer.NAMESPACE + "Bosses.AddSetting")]
        public static void AddSetting(Players.Player player, NetworkUI.NetworkMenu menu)
        {
            if (player.ActiveColony != null)
            {
                menu.Items.Add(new NetworkUI.Items.DropDown("Pandaros.API Bosses", _Bosses, new List<string>() { "Disabled", "Enabled" }));
                var ps = ColonyState.GetColonyState(player.ActiveColony);
                menu.LocalStorage.SetAs(_Bosses, Convert.ToInt32(ps.BossesEnabled));
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerChangedNetworkUIStorage, GameInitializer.NAMESPACE + "Bosses.ChangedSetting")]
        public static void ChangedSetting(ValueTuple<Players.Player, JSONNode, string> data)
        {
            if (data.Item1.ActiveColony != null)

                switch (data.Item3)
                {
                    case "server_popup":
                        var ps = ColonyState.GetColonyState(data.Item1.ActiveColony);

                        if (ps != null && data.Item2.GetAsOrDefault(_Bosses, Convert.ToInt32(ps.BossesEnabled)) != Convert.ToInt32(ps.BossesEnabled))
                        {
                            ps.BossesEnabled = data.Item2.GetAsOrDefault(_Bosses, Convert.ToInt32(ps.BossesEnabled)) != 0;
                            PandaChat.Send(data.Item1, _localizationHelper, "BossesToggled", ChatColor.green, ps.BossesEnabled ? _localizationHelper.LocalizeOrDefault("on", data.Item1) : _localizationHelper.LocalizeOrDefault("off", data.Item1));
                        }

                        break;
                }
        }

        public bool TryDoCommand(Players.Player player, string chat, List<string> split)
        {
            if (!chat.StartsWith("/bosses", StringComparison.OrdinalIgnoreCase))
                return false;

            if (player == null || player.ID == NetworkID.Server)
                return true;


            if (player.ActiveColony == null)
                PandaChat.Send(player, _localizationHelper, "NoColony", ChatColor.red);

            var array = new List<string>();
            CommandManager.SplitCommand(chat, array);
            var state  = ColonyState.GetColonyState(player.ActiveColony);

            if (array.Count == 1)
            {
                PandaChat.Send(player, _localizationHelper, "BossesToggled", ChatColor.green, state.BossesEnabled ? _localizationHelper.LocalizeOrDefault("on", player) : _localizationHelper.LocalizeOrDefault("off", player));
                return true;
            }

            if (array.Count == 2)
            {
                if (array[1].ToLower().Trim() == "on" || array[1].ToLower().Trim() == "true")
                    state.BossesEnabled = true;
                else
                    state.BossesEnabled = false;

                PandaChat.Send(player, _localizationHelper, "BossesToggled", ChatColor.green, state.BossesEnabled ? _localizationHelper.LocalizeOrDefault("on", player) : _localizationHelper.LocalizeOrDefault("off", player));
            }

            NetworkUI.NetworkMenuManager.SendColonySettingsUI(player);
            PandaChat.Send(player, _localizationHelper, "AdminDisabledBosses", ChatColor.red);
            

            return true;
        }
    }
}