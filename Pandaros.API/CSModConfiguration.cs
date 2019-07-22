using Chatting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pipliz.JSON;
using System;
using System.Collections.Generic;
using System.IO;

namespace Pandaros.API
{
    public static class APIConfiguration
    {
        public static CSModConfiguration CSModConfiguration { get; set; } = new CSModConfiguration(GameInitializer.NAMESPACE);

        public static bool DifficutlyCanBeChanged
        {
            get
            {
                return CSModConfiguration.GetorDefault(nameof(DifficutlyCanBeChanged), true);
            }
            set
            {
                CSModConfiguration.SetValue(nameof(DifficutlyCanBeChanged), value);
            }
        }

        public static GameDifficulty MinDifficulty
        {
            get
            {
                var diffStr = CSModConfiguration.GetorDefault(nameof(MinDifficulty), GameDifficulty.Normal.Name);

                if (GameDifficulty.GameDifficulties.ContainsKey(diffStr))
                    return GameDifficulty.GameDifficulties[diffStr];

                return GameDifficulty.Normal;
            }
            private set
            {
                CSModConfiguration.SetValue(nameof(MinDifficulty), value);
            }
        }

        public static void Reload()
        {
            if (File.Exists(CSModConfiguration.SaveFile) && JSON.Deserialize(CSModConfiguration.SaveFile, out var config))
            {
                CSModConfiguration.SettingsRoot = config;

                if (config.TryGetAs("GameDifficulties", out JSONNode diffs) && diffs.NodeType == NodeType.Array)
                    foreach (var diff in diffs.LoopArray())
                    {
                        var newDiff = diff.JsonDeerialize<GameDifficulty>();
                        GameDifficulty.GameDifficulties[newDiff.Name] = newDiff;
                    }
            }
        }

        public static void Save()
        {
            var diffs = new JSONNode(NodeType.Array);

            foreach (var diff in GameDifficulty.GameDifficulties.Values)
                diffs.AddToArray(diff.ToJson());

            CSModConfiguration.SettingsRoot.SetAs("GameDifficulties", diffs);

            CSModConfiguration.Save();
        }

    }

    public class CSModConfiguration
    {
        public string SaveFile { get; }
        public JSONNode SettingsRoot { get; set; }

        public CSModConfiguration(string configurationFileName)
        {
            SaveFile = $"{GameInitializer.SAVE_LOC}/{configurationFileName}.json";

            if (File.Exists(SaveFile))
                SettingsRoot = JSON.Deserialize(SaveFile);
            else
                SettingsRoot = new JSONNode();
        }

        public void Reload()
        {
            if (File.Exists(SaveFile) && JSON.Deserialize(SaveFile, out var config))
                SettingsRoot = config;
        }

        public void Save()
        {
            JSON.Serialize(SaveFile, SettingsRoot);
        }

        public bool HasSetting(string setting)
        {
            return SettingsRoot.HasChild(setting);
        }

        public T GetorDefault<T>(string key, T defaultVal)
        {
            if (!SettingsRoot.HasChild(key))
                SetValue(key, defaultVal);

            return SettingsRoot.GetAs<T>(key);
        }

        public void SetValue<T>(string key, T val)
        {
            SettingsRoot.SetAs<T>(key, val);
            Save();
        }
    }

}