using colonyserver.Assets.UIGeneration;
using ModLoaderInterfaces;
using Pandaros.API.Extender;
using Pandaros.API.localization;
using Pipliz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.API.Gui
{
    public class Notifications
    {
        public static Dictionary<Players.Player, string[]> NotificationText { get; set; } = new Dictionary<Players.Player, string[]>();

        public static float NotificationWidth { get; set; } = 450;

        public static Vector3Int[] NotificationSpots { get; set; } = new []
        {
            new Vector3Int(NotificationWidth / 2, 60,0),
            new Vector3Int(NotificationWidth / 2, 0,0),
            new Vector3Int(NotificationWidth / 2,-60,0)
        };

        public int NextUpdateTimeMinMs { get; set; } = 1000;

        public int NextUpdateTimeMaxMs { get; set; } = 3000;

        public ServerTimeStamp NextUpdateTime { get; set; }

        public static void NotifyAll(localization.LocalizationHelper localizationHelper,
                           string message,
                           params string[] args)
        {
            foreach (var player in Players.PlayerDatabase.Values)
                IssueNotification(player, localizationHelper, message, args);
        }

        public static void IssueNotification(Colony colony, localization.LocalizationHelper localizationHelper,
                           string message,
                           params string[] args)
        {
            colony.ForEachOwner(player =>
            {
                IssueNotification(player, localizationHelper, message, args);
            });
        }

        public static void IssueNotification(Players.Player player, localization.LocalizationHelper localizationHelper,
                                string message,
                                params string[] args)
        {
            if (player.IsConnected())
            {
                var messageBuilt = string.Format(localizationHelper.LocalizeOrDefault(message, player), PandaChat.LocalizeArgs(player, localizationHelper, args));
                IssueNotification(player, messageBuilt);
            }
        }

        public static void IssueNotification(Players.Player player, string message)
        {
            if (!NotificationText.TryGetValue(player, out var notifications))
            {
                notifications = new string[3]
                    {
                        string.Empty,
                        string.Empty,
                        string.Empty
                    };
                NotificationText[player] = notifications;
            }

            notifications[2] = notifications[1];
            notifications[1] = notifications[0];
            notifications[0] = message;


            for (int i = 0; i < notifications.Length; i++)
                UIManager.AddorUpdateUILabel("Notification" + i, colonyshared.NetworkUI.UIGeneration.UIElementDisplayType.Global, notifications[i], NotificationSpots[i], colonyshared.NetworkUI.AnchorPresets.MiddleLeft, NotificationWidth, player, 17, colonyshared.NetworkUI.UIGeneration.FontType.Norse, "#e9fce3");
        }
    }
}
