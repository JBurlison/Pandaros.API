﻿using NetworkUI;
using NetworkUI.Items;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pandaros.API.Models;
using Pipliz;
using Pipliz.JSON;
using Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Pandaros.API.Help
{
    [ModLoader.ModManager]
    public static class UIManager
    {
        public static JSONNode LoadedMenus { get; private set; } = new JSONNode();
        private static localization.LocalizationHelper _localizationHelper = new localization.LocalizationHelper(GameInitializer.NAMESPACE, "Wiki");
        public static List<OpenMenuSettings> OpenMenuItems { get; private set; } = new List<OpenMenuSettings>();
        public static Dictionary<ushort, Recipes.Recipe> ItemRecipe = new Dictionary<ushort, Recipes.Recipe>();

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerClicked, GameInitializer.NAMESPACE + ".Help.HelpMenuItem.OpenMenu")]
        public static void OpenMenu(Players.Player player, PlayerClickedData playerClickData)
        {
            if (player == null)
                return;

            foreach (var item in OpenMenuItems)
            {
                if (playerClickData.ClickType != item.ActivateClickType)
                    continue;

                if (ItemTypes.IndexLookup.TryGetIndex(item.ItemName, out var menuItem) &&
                    playerClickData.TypeSelected == menuItem)
                {
                    SendMenu(player, item.UIUrl);
                }
            }
        }
        

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnAssemblyLoaded, GameInitializer.NAMESPACE + ".Managers.OnAssemblyLoaded")]
        [ModLoader.ModCallbackDependsOn(GameInitializer.NAMESPACE + ".OnAssemblyLoaded")]
        public static void OnAssemblyLoaded(string path)
        {
            var settings = GameInitializer.GetJSONSettingPaths(GameInitializer.NAMESPACE + ".MenuFile");
            JObject jObject = new JObject();

            foreach (var info in settings)
                try
                {
                    foreach (var jsonNode in info.Value)
                    {
                        try
                        {
                            jObject.Merge(JsonConvert.DeserializeObject<JObject>(File.ReadAllText(info.Key + "/" + jsonNode)), new JsonMergeSettings() { MergeArrayHandling = MergeArrayHandling.Union, MergeNullValueHandling = MergeNullValueHandling.Ignore, PropertyNameComparison = StringComparison.InvariantCultureIgnoreCase });
                        }
                        catch (Exception ex)
                        {
                            APILogger.LogError(ex, "Error loading settings node " + jsonNode);
                        }
                    }
                }
                catch (Exception ex)
                {
                    APILogger.LogError(ex, "Error loading settings file " + info.Key + " values " + string.Join(", ", info.Value.ToArray()));
                }

            LoadedMenus = JSON.DeserializeString(JsonConvert.SerializeObject(jObject));
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad, GameInitializer.NAMESPACE + ".Managers.LoadRecipes")]
        public static void LoadRecipes()
        {
            foreach(var recipe in ServerManager.RecipeStorage.Recipes.Values)
            {
                if(recipe != null && recipe.Results != null)
                    foreach(var results in recipe.Results)
                    {
                        if(!ItemRecipe.ContainsKey(results.Type))
                            ItemRecipe.Add(results.Type, recipe);
                    }
            }
        }

        public static void SendMenu(Players.Player player, string reference)
        {
            string url = reference;

            if (reference.Contains("_"))
                url = reference.Substring(reference.IndexOf("_") + 1);

            var splitUrl = url.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            JSONNode uiNode = LoadedMenus;

            for (int i = 0; i < splitUrl.Length; i++)
            {
                if(!uiNode.TryGetAs(splitUrl[i], out uiNode))
                    break;
            }

            if (uiNode != LoadedMenus)
                SendMenu(player, uiNode);
        }

        public static void SendMenu(Players.Player player, JSONNode json)
        {
            NetworkMenu menu = new NetworkMenu();
            menu.ForceClosePopups = true;

            json.TryGetAsOrDefault("header", out string header, "Title");
            menu.LocalStorage.SetAs("header", header);

            if (json.HasChild("width"))
                menu.Width = json.GetAs<int>("width");
            else
                menu.Width = 1000;

            if (json.HasChild("height"))
                menu.Height = json.GetAs<int>("height");
            else
                menu.Height = 700;

            foreach (JSONNode item in (json.GetAs<JSONNode>("Items")).LoopArray())
                if (LoadItem(item, ref menu, player, out var menuItem))
                    menu.Items.Add(menuItem);

            NetworkMenuManager.SendServerPopup(player, menu);
        }

        //Ref menu is added for change LocalStorage -> avoid client error
        public static bool LoadItem(JSONNode item, ref NetworkMenu menu, Players.Player player, out List<IItem> menuItem)
        {
            string itemType = item.GetAs<string>("type").Trim().ToLower();
            bool found = false;
            menuItem = null;

            switch(itemType)
            {
                case "patchnotes":
                    if (item.TryGetAsOrDefault<string>("mod", out string mod, ""))
                    {
                        var info = default(JSONNode);

                        foreach (var modJson in GameInitializer.AllModInfos.Values)
                        {
                            if (modJson.TryGetAs("name", out string modName) && string.Equals(mod, modName, StringComparison.InvariantCultureIgnoreCase))
                            {
                                info = modJson;
                                break;
                            }
                        }

                        if (info.TryGetAs<JSONNode>("patchnotes", out var patchNotesJson))
                        {
                            int i = 0;
                            menuItem = new List<IItem>();

                            foreach (var node in patchNotesJson.LoopArray())
                            {
                                i++;

                                node.TryGetAsOrDefault("version", out string version, "Undefined");
                                menuItem.Add(new Label(new LabelData("Version " + version, UnityEngine.TextAnchor.MiddleLeft, 24, LabelData.ELocalizationType.None)));

                                if (node.TryGetAs<JSONNode>("notes", out var versionNotesJson))
                                    foreach (var note in versionNotesJson.LoopArray())
                                        menuItem.Add(new Label(new LabelData("    * " + note.ToString().Replace("\"", ""), UnityEngine.TextAnchor.MiddleLeft, 14, LabelData.ELocalizationType.None)));

                                if (i > 10)
                                    break;
                            }
                        }
                    }
                    else
                        APILogger.Log(ChatColor.red, "found patchnotes wiki item but mod property was not found");

                    found = true;
                    break;
                case "label":
                    {
                        menuItem = new List<IItem>() { new Label(GetLabelData(item)).ApplyPosition(item) };
                        found = true;
                    }
                    break;

                case "space":
                    {
                        item.TryGetAsOrDefault<int>("height", out int height, 10);
                        menuItem = new List<IItem>() { new EmptySpace(height).ApplyPosition(item) };
                        found = true;
                    }
                    break;

                case "line":
                    {
                        UnityEngine.Color color = UnityEngine.Color.black;

                        if(item.HasChild("color"))
                            color = GetColor(item.GetAs<string>("color"));

                        item.TryGetAsOrDefault<int>("height", out int height, 4);
                        item.TryGetAsOrDefault<int>("width", out int width, -1);

                        menuItem = new List<IItem>() { new Line(color, height, width).ApplyPosition(item) };
                        found = true;
                    }
                    break;

                case "icon":
                    {
                        item.TryGetAsOrDefault<string>("name", out string icon, "missingerror");
                        menuItem = new List<IItem>() { new ItemIcon(icon).ApplyPosition(item) };
                        found = true;
                    }
                    break;

                case "jobrecipies":
                    {
                        if (item.TryGetAs("job", out string job))
                        {
                            List<Recipes.Recipe> recipes = new List<Recipes.Recipe>();
                            menuItem = new List<IItem>();

                            if (ServerManager.RecipeStorage.RecipesPerLimitType.TryGetValue(job, out var recipesDefault))
                                recipes.AddRange(recipesDefault);

                            foreach (var recipe in recipes.OrderBy(r => r.Name))
                               menuItem.Add(RecipeLines(menu, player, recipe, item));

                            found = true;
                        }

                    }
                    break;

                case "item":
                    {
                        if (!item.HasChild("name"))
                        {
                            APILogger.Log("Item: Not name defined");
                            return found;
                        }

                        item.TryGetAs<string>("name", out string name);

                        if (!ItemTypes.IndexLookup.TryGetIndex(name, out ushort index))
                        {
                            APILogger.Log("Item: Not item found with name: " + name);
                            return found;
                        }

                        if (!Localization.TryGetSentence(player.LastKnownLocale, _localizationHelper.GetLocalizationKey("ItemDetails." + name), out var extendedDetail))
                            extendedDetail = "";
                        else
                            extendedDetail = Environment.NewLine + extendedDetail;

                        if (Localization.TryGetType(player.LastKnownLocale, index, out string localeName) && Localization.TryGetTypeUse(player.LastKnownLocale, index, out var description))
                        {
                            menuItem = new List<IItem>() { new HorizontalSplit(new ItemIcon(index), new Label(new LabelData(localeName + Environment.NewLine + description + extendedDetail)), 30, .3f).ApplyPosition(item) };
                            found = true;
                        }
                    }
                    break;

                case "itemrecipe":
                    {
                        if (!item.HasChild("name"))
                        {
                            APILogger.Log("ItemRecipe: Not name defined");
                            return found;
                        }

                        item.TryGetAs<string>("name", out string name);

                        if (!ItemTypes.IndexLookup.TryGetIndex(name, out ushort index))
                        {
                            APILogger.Log("ItemRecipe: Not item found with name: " + name );
                            return found;
                        }

                        if (!ItemRecipe.TryGetValue(index, out var recipe))
                        {
                            APILogger.Log("ItemRecipe: Not recipe found for: " + name );
                            return found;
                        }

                        menuItem = new List<IItem>();
                        
                        if (Localization.TryGetType(player.LastKnownLocale, index, out string localeName))
                        {
                            menuItem.Add(new Label(localeName + ":").ApplyPosition(item));
                        }
                        else
                            menuItem.Add(new Label(name + ":").ApplyPosition(item));

                        menuItem.Add(RecipeLines(menu, player, recipe, item));
                        found = true;
                    }
                    break;

                case "dropdown":
                    {
                        string id;

                        if(item.HasChild("id"))
                        {
                            id = item.GetAs<string>("id");
                        }
                        else
                        {
                            id = "dropdown";
                            APILogger.Log("Dropdown without ID defined, default: dropdown");
                        }

                        List<string> options = new List<string>();

                        if(item.HasChild("options"))
                        {
                            JSONNode optionsj = item.GetAs<JSONNode>("options");

                            foreach(var option in optionsj.LoopArray())
                                options.Add(option.GetAs<string>());
                        }
                        else
                        {
                            options.Add("No options available");
                            APILogger.Log(string.Format("dropdown {0} without options", id));
                        }

                        item.TryGetAsOrDefault<int>("height", out int height, 30);
                        item.TryGetAsOrDefault<int>("marginHorizontal", out int marginHorizontal, 4);
                        item.TryGetAsOrDefault<int>("marginVertical", out int marginVertical, 2);

                        // if label dropdown else dropdownNOLABEL
                        if(item.TryGetChild("label", out JSONNode labelj))
                        {
                            LabelData label = GetLabelData(labelj);
                            menuItem = new List<IItem>() { new DropDown(label.text, id, options).ApplyPosition(item) };
                        }
                        else
                        {
                            menuItem = new List<IItem>() { new DropDownNoLabel(id, options, height).ApplyPosition(item) };
                        }

                        menu.LocalStorage.SetAs(id, 0);
                        found = true;
                    }
                    break;

                case "toggle":
                    {
                        string id;

                        if(item.HasChild("id"))
                        {
                            id = item.GetAs<string>("id");
                        }
                        else
                        {
                            id = "toggle";
                            APILogger.Log("Toggle without ID defined, default: toggle");
                        }

                        item.TryGetAsOrDefault<int>("height", out int height, 25);
                        item.TryGetAsOrDefault<int>("toggleSize", out int toggleSize, 20);

                        // if label toggle else togglenolabel
                        if (item.TryGetChild("label", out JSONNode labelj))
                        {
                            LabelData label = GetLabelData(labelj);
                            menuItem = new List<IItem>() { new Toggle(label, id, height, toggleSize).ApplyPosition(item) };
                        }
                        else
                        {
                            menuItem = new List<IItem>() { new ToggleNoLabel(id, toggleSize).ApplyPosition(item) };
                        }

                        found = true;
                        menu.LocalStorage.SetAs(id, false);
                    }
                    break;

                case "button":
                    {
                        string id;

                        if(item.HasChild("id"))
                        {
                            id = item.GetAs<string>("id");
                        }
                        else
                        {
                            id = "button";
                            APILogger.Log("Button without ID defined, default: button");
                        }

                        item.TryGetAsOrDefault<int>("width", out int width, -1);
                        item.TryGetAsOrDefault<int>("height", out int height, 25);

                        if(item.TryGetChild("label", out JSONNode labelj))
                        {
                            LabelData label = GetLabelData(labelj);
                            menuItem = new List<IItem>() { new ButtonCallback(id, label, width, height).ApplyPosition(item) };
                        }
                        else
                        {
                            APILogger.Log(string.Format("Button {0} without label", id));
                            menuItem = new List<IItem>() { new ButtonCallback(id, new LabelData("Key label not defined"), width, height).ApplyPosition(item) };
                        }

                        found = true;
                    }
                    break;

                case "link":
                    {
                        string url;

                        if(item.HasChild("url"))
                        {
                            url = GameInitializer.NAMESPACE + ".link_" + item.GetAs<string>("url");
                        }
                        else
                        {
                            APILogger.Log("Link without URL defined");
                            return found;
                        }

                        item.TryGetAsOrDefault<int>("width", out int width, -1);
                        item.TryGetAsOrDefault<int>("height", out int height, 25);

                        if(item.TryGetChild("label", out JSONNode labelj))
                        {
                            LabelData label = GetLabelData(labelj);
                            menuItem = new List<IItem>() { new ButtonCallback(url, label, width, height).ApplyPosition(item) };
                        }
                        else
                        {
                            APILogger.Log(string.Format("Link {0} without label", url));
                            menuItem = new List<IItem>() { new ButtonCallback(url, new LabelData("Key label not defined"), width, height).ApplyPosition(item) };
                        }

                        found = true;
                    }
                    break;

                case "table":
                    {
                        item.TryGetAsOrDefault<int>("row_height", out int height, 30);

                        if (item.TryGetAs("rows", out JSONNode rows) && rows.ChildCount != 0)
                        {
                            menuItem = new List<IItem>();
                            found = true;

                            foreach (JSONNode row in rows.LoopArray())
                            {
                                List<ValueTuple<IItem, int>> items = new List<ValueTuple<IItem, int>>();
                                var width = menu.Width / row.ChildCount;

                                foreach (JSONNode col in row.LoopArray())
                                {
                                    if (LoadItem(col, ref menu, player, out var newMenuItems))
                                        foreach (var newItem in newMenuItems)
                                            if (col.TryGetAs("col_width", out int setWidth))
                                                items.Add(ValueTuple.Create(newItem, setWidth));
                                            else
                                                items.Add(ValueTuple.Create(newItem, width));
                                }

                                menuItem.Add(new HorizontalRow(items, height).ApplyPosition(item));
                            }
                        }
                    }
                    break;


                default:
                    {
                        APILogger.Log(string.Format("It doesn't exist an item of type: {0}", itemType));
                    }
                    break;
            }

            return found;
        }

        public static IItem ApplyPosition(this IItem newItem, JSONNode item)
        {
            if (item.TryGetAs("position", out int pos))
                return new HorizontalSplit(new EmptySpace(), newItem, 0, 0, HorizontalSplit.ESplitType.Relative, 0, pos);
            else
                return newItem;
        }

        private static List<IItem> RecipeLines(NetworkMenu menu, Players.Player player, Recipes.Recipe recipe, JSONNode item)
        {
            List<IItem> menuItems = new List<IItem>();

            menuItems.Add(new Label(new LabelData(recipe.Name, UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter, 30, LabelData.ELocalizationType.Type)).ApplyPosition(item));
            menuItems.Add(new Label(new LabelData(_localizationHelper.GetLocalizationKey("Requirements"), UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleLeft, 24)).ApplyPosition(item));

            List<ValueTuple<IItem, int>> headerItems = new List<ValueTuple<IItem, int>>();

            headerItems.Add(ValueTuple.Create<IItem, int>(new Label(new LabelData("")), 70));
            headerItems.Add(ValueTuple.Create<IItem, int>(new Label(new LabelData(_localizationHelper.GetLocalizationKey("Item"))), 150));
            headerItems.Add(ValueTuple.Create<IItem, int>(new Label(new LabelData(_localizationHelper.GetLocalizationKey("Amount"))), 100));

            menuItems.Add(new HorizontalRow(headerItems).ApplyPosition(item));

            foreach (var req in recipe.Requirements)
            {
                if (req == null)
                    continue;

                string reqName = ItemTypes.IndexLookup.GetName(req.Type);

                ItemIcon icon = new ItemIcon(reqName);
                if (Localization.TryGetType(player.LastKnownLocale, req.Type, out string localeReqName))
                    reqName = localeReqName;

                Label labelName = new Label(new LabelData(reqName));
                Label labelAmount = new Label(new LabelData(req.Amount.ToString()));

                List<ValueTuple<IItem, int>> items = new List<ValueTuple<IItem, int>>();
                items.Add(ValueTuple.Create<IItem, int>(icon, 70));
                items.Add(ValueTuple.Create<IItem, int>(labelName, 150));
                items.Add(ValueTuple.Create<IItem, int>(labelAmount, 100));

                menuItems.Add(new HorizontalRow(items).ApplyPosition(item));
            }

            menuItems.Add(new Label(new LabelData(_localizationHelper.GetLocalizationKey("Results"), UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleLeft, 24)).ApplyPosition(item));

            headerItems = new List<ValueTuple<IItem, int>>();
            headerItems.Add(ValueTuple.Create<IItem, int>(new Label(new LabelData("")), 70));
            headerItems.Add(ValueTuple.Create<IItem, int>(new Label(new LabelData(_localizationHelper.GetLocalizationKey("Item"))), 150));
            headerItems.Add(ValueTuple.Create<IItem, int>(new Label(new LabelData(_localizationHelper.GetLocalizationKey("Amount"))), 100));
            headerItems.Add(ValueTuple.Create<IItem, int>(new Label(new LabelData(_localizationHelper.GetLocalizationKey("Chance"))), 100));

            menuItems.Add(new HorizontalRow(headerItems).ApplyPosition(item));

            foreach (var req in recipe.Results)
            {
                string reqName = ItemTypes.IndexLookup.GetName(req.Type);

                ItemIcon icon = new ItemIcon(reqName);
                if (Localization.TryGetType(player.LastKnownLocale, req.Type, out string localeReqName))
                    reqName = localeReqName;

                Label labelName = new Label(new LabelData(reqName));
                Label labelAmount = new Label(new LabelData(req.Amount.ToString()));
                Label chance = new Label(new LabelData(req.Chance * 100 + "%"));
                List<ValueTuple<IItem, int>> items = new List<ValueTuple<IItem, int>>();
                items.Add(ValueTuple.Create<IItem, int>(icon, 70));
                items.Add(ValueTuple.Create<IItem, int>(labelName, 150));
                items.Add(ValueTuple.Create<IItem, int>(labelAmount, 100));
                items.Add(ValueTuple.Create<IItem, int>(chance, 100));

                menuItems.Add(new HorizontalRow(items).ApplyPosition(item));

                if (Localization.TryGetTypeUse(player.LastKnownLocale, req.Type, out var description))
                    menuItems.Add(new Label(new LabelData(description)).ApplyPosition(item));

                if (Localization.TryGetSentence(player.LastKnownLocale, _localizationHelper.GetLocalizationKey("ItemDetails." + ItemId.GetItemId(req.Type).Name), out var extendedDetail))
                    menuItems.Add(new Label(new LabelData(extendedDetail)).ApplyPosition(item));
            }

            menuItems.Add(new Line(UnityEngine.Color.black, 1));

            return menuItems;
        }

        public static LabelData GetLabelData(JSONNode json)
        {
            json.TryGetAsOrDefault("text", out string text, "Text key not found");

            UnityEngine.Color color = UnityEngine.Color.black;
            UnityEngine.TextAnchor alignement = UnityEngine.TextAnchor.MiddleLeft;

            if(json.HasChild("color"))
                color = GetColor(json.GetAs<string>("color"));

            if (json.TryGetAs("alignement", out string alignementStr))
                Enum.TryParse(alignementStr, true, out alignement);

            json.TryGetAsOrDefault("fontsize", out int fontSize, 18);

            LabelData.ELocalizationType localizationType = LabelData.ELocalizationType.Sentence;

            if (json.TryGetAs("localizationType", out string localizationString) && Enum.TryParse(localizationString, true, out LabelData.ELocalizationType newLocalization))
                localizationType = newLocalization;

            return new LabelData(text, color, alignement, fontSize, localizationType);
        }

        public static UnityEngine.Color GetColor(string color)
        {
            switch(color.Trim().ToLower())
            {
                case "cyan":
                return UnityEngine.Color.cyan;

                case "green":
                return UnityEngine.Color.green;

                case "red":
                return UnityEngine.Color.red;

                case "black":
                return UnityEngine.Color.black;

                case "yellow":
                return UnityEngine.Color.yellow;

                case "blue":
                return UnityEngine.Color.blue;

                case "magenta":
                return UnityEngine.Color.magenta;

                case "gray":
                return UnityEngine.Color.gray;

                case "white":
                return UnityEngine.Color.white;

                case "clear":
                return UnityEngine.Color.clear;

                case "grey":
                return UnityEngine.Color.grey;

                default:
                return UnityEngine.Color.black;
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerPushedNetworkUIButton, GameInitializer.NAMESPACE + ".UIManager.PressLink")]
        public static void PressButton(ButtonPressCallbackData data)
        {
            if(data.ButtonIdentifier.StartsWith(GameInitializer.NAMESPACE + ".link_"))
            {
                string url = data.ButtonIdentifier.Substring(data.ButtonIdentifier.IndexOf("_") + 1);

                SendMenu(data.Player, url);
            }
        }
    }
}
