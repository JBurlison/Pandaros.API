using Chatting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pipliz.JSON;
using System;
using System.Collections.Generic;
using System.IO;

namespace Pandaros.API
{
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