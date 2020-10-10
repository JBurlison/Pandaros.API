using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.API.localization
{
    public class LocalizationHelper
    {
        public string Prefix { get; set; }
        public string Namespace { get; set; }

        public LocalizationHelper(string modNamespace, string prefix)
        {
            Prefix = prefix;
            Namespace = modNamespace;
        }

        public string LocalizeOrDefault(string key, Players.Player p, params string[] args)
        {
            return string.Format(LocalizeOrDefault(key, p), PandaChat.LocalizeArgs(p, this, args));
        }

        public string LocalizeOrDefault(string key, Players.Player p)
        {
            if (p.ConnectionState != Players.EConnectionState.Connected)
                return key;

            if (ItemTypes.TryGetType(key, out ItemTypes.ItemType itemType) &&
                Localization.TryGetType(p.LastKnownLocale, itemType, out var localizedTypeName))
                return localizedTypeName;
            else
            {
                var fullKey = GetLocalizationKey(key);
                var newVal = Localization.GetSentence(p.LastKnownLocale, fullKey);

                if (newVal == fullKey)
                {
                    if (Localization.TryGetSentence(p.LastKnownLocale, key, out newVal) || Localization.TryGetSentence(p.LastKnownLocale, ColonyBuiltIn.NPCLocalizationPrefix + key, out newVal))
                        return newVal;
                    else
                        return key;
                }
                else
                    return newVal;
            }

            
        }

        public string GetLocalizationKey(string key)
        {
            return string.Concat(Namespace, ".", Prefix, ".", key);
        }
    }
}
