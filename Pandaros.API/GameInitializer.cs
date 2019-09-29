using Chatting;
using Pandaros.API.AI;
using Pandaros.API.ColonyManagement;
using Pandaros.API.Items;
using Pandaros.API.Items.Armor;
using Pandaros.API.Jobs.Roaming;
using Pandaros.API.Monsters;
using Pandaros.API.Server;
using Pipliz.JSON;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;

namespace Pandaros.API
{
    [ModLoader.ModManager]
    public static class GameInitializer
    {
        public static readonly ReadOnlyCollection<string> BLOCK_ROTATIONS = new ReadOnlyCollection<string>(new List<string>() { "x+", "x-", "z+", "z-" });

        public static string ICON_PATH = "gamedata/mods/Pandaros/API/icons/";
        public static string MESH_PATH = "gamedata/mods/Pandaros/API/Meshes/";
        public const string NAMESPACE = "Pandaros.API";
        public const string SETTLER_INV = "Pandaros.API.Inventory";
        public const string ALL_SKILLS = "Pandaros.API.ALLSKILLS";
        public static string NPC_PATH = "gamedata/textures/materials/npc/";
        public static string MOD_FOLDER = @"gamedata/mods/Pandaros/API";
        public static string MODS_FOLDER = @"";
        public static string GAMEDATA_FOLDER = @"";
        public static string GAME_ROOT = @"";
        public static string SAVE_LOC = "";
        public static readonly Version MOD_VER = new Version(0, 1, 4, 0);
        public static bool RUNNING { get; private set; }
        public static bool WorldLoaded { get; private set; }
        public static Colony StubColony { get; private set; }
        public static JSONNode ModInfo { get; private set; }
        public static Dictionary<string, JSONNode> AllModInfos { get; private set; } = new Dictionary<string, JSONNode>(StringComparer.InvariantCultureIgnoreCase);

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterSelectedWorld, NAMESPACE + ".AfterSelectedWorld")]
        public static void AfterSelectedWorld()
        {
            WorldLoaded                 = true;
            SAVE_LOC                    = GAMEDATA_FOLDER + "savegames/" + ServerManager.WorldName + "/";
            StubColony = Colony.CreateStub(-99998);

            APIConfiguration.Save();
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnAssemblyLoaded, NAMESPACE + ".OnAssemblyLoaded")]
        public static void OnAssemblyLoaded(string path)
        {
            MOD_FOLDER = Path.GetDirectoryName(path);
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            APILogger.Log("Found mod in {0}", MOD_FOLDER);

            GAME_ROOT = path.Substring(0, path.IndexOf("gamedata")).Replace("/", "/");
            GAMEDATA_FOLDER = path.Substring(0, path.IndexOf("gamedata") + "gamedata".Length).Replace("/", "/") + "/";
            MODS_FOLDER = GAMEDATA_FOLDER + "mods/";
            ICON_PATH = Path.Combine(MOD_FOLDER, "icons").Replace("\\", "/") + "/";
            MESH_PATH = Path.Combine(MOD_FOLDER, "Meshes").Replace("\\", "/") + "/";
            ModInfo = JSON.Deserialize(MOD_FOLDER + "/modInfo.json")[0];

            List<string> allinfos = new List<string>();
            DirSearch(MODS_FOLDER, "*modInfo.json", allinfos);

            foreach (var info in allinfos)
            {
                var modJson = JSON.Deserialize(info)[0];

                if (modJson.TryGetAs("enabled", out bool isEnabled) && isEnabled)
                {
                    APILogger.Log("ModInfo Found: {0}", info);
                    AllModInfos[new FileInfo(info).Directory.FullName] = modJson;
                }
            }

            GenerateBuiltinBlocks();
        }

        private static void GenerateBuiltinBlocks()
        {
            if (File.Exists(MOD_FOLDER + "/ColonyBuiltin.cs"))
                File.Delete(MOD_FOLDER + "/ColonyBuiltin.cs");

            using (var fs = File.OpenWrite(MOD_FOLDER + "/ColonyBuiltin.cs"))
            using (var sr = new StreamWriter(fs))
            {
                sr.WriteLine("using Pandaros.API.Models;");
                sr.WriteLine();
                sr.WriteLine("namespace Pandaros.API");
                sr.WriteLine("{");
                sr.WriteLine("  public static class ColonyBuiltIn");
                sr.WriteLine("  {");
                sr.WriteLine("      public static class Research");
                sr.WriteLine("      {");

                foreach (var node in JSON.Deserialize(GAMEDATA_FOLDER + "science.json").LoopArray())
                    if (node.TryGetAs("key", out string scienceKey))
                        sr.WriteLine($"          public const string {scienceKey.Substring(scienceKey.LastIndexOf('.') + 1).ToUpper()} = \"{scienceKey}\";");

                sr.WriteLine("      }");
                sr.WriteLine();

                sr.WriteLine("      public static class NpcTypes");
                sr.WriteLine("      {");

                foreach (var node in JSON.Deserialize(GAMEDATA_FOLDER + "npcTypes.json").LoopArray())
                    if (node.TryGetAs("keyName", out string npcType))
                        sr.WriteLine($"          public const string {npcType.Substring(npcType.LastIndexOf('.') + 1).ToUpper()} = \"{npcType}\";");

                sr.WriteLine("      }");
                sr.WriteLine();

                sr.WriteLine("      public static class ItemTypes");
                sr.WriteLine("      {");

                foreach (var node in JSON.Deserialize(GAMEDATA_FOLDER + "generateblocks.json").LoopArray())
                    if (node.TryGetAs("generateType", out string genType) && genType == "rotateBlock" && node.TryGetAs("typeName", out string itemName))
                    {
                        sr.WriteLine($"          public static readonly ItemId {itemName.Replace('+', 'p').Replace('-', 'n').ToUpper()} = ItemId.GetItemId(\"{itemName}\");");

                        foreach (var rotation in BLOCK_ROTATIONS)
                            sr.WriteLine($"          public static readonly ItemId {(itemName + rotation).Replace('+', 'p').Replace('-', 'n').ToUpper()} = ItemId.GetItemId(\"{(itemName + rotation)}\");");
                    }

                foreach (var node in JSON.Deserialize(GAMEDATA_FOLDER + "types.json").LoopObject())
                        sr.WriteLine($"          public static readonly ItemId {node.Key.Replace('+', 'p').Replace('-', 'n').ToUpper()} = ItemId.GetItemId(\"{node.Key}\");");

                sr.WriteLine("      }");
                sr.WriteLine("  }");
                sr.WriteLine("}");
            }
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            APILogger.Log(args.Name);
            try
            {
                if (args.Name.Contains("System.Xml.Linq"))
                    return Assembly.LoadFile(MOD_FOLDER + "/System.Xml.Linq.dll");

                if (args.Name.Contains("System.ComponentModel.DataAnnotations"))
                    return Assembly.LoadFile(MOD_FOLDER + "/System.ComponentModel.DataAnnotations.dll");

                if (args.Name.Contains("System.Numerics"))
                    return Assembly.LoadFile(MOD_FOLDER + "/System.Numerics.dll");

                if (args.Name.Contains("System.Runtime.Serialization"))
                    return Assembly.LoadFile(MOD_FOLDER + "/System.Runtime.Serialization.dll");

                if (args.Name.Contains("System.Transactions"))
                    return Assembly.LoadFile(MOD_FOLDER + "/System.Transactions.dll");

                if (args.Name.Contains("System.Data.SQLite"))
                    return Assembly.LoadFile(MOD_FOLDER + "/System.Data.SQLite.dll");

                if (args.Name.Contains("System.Data"))
                    return Assembly.LoadFile(MOD_FOLDER + "/System.Data.dll");
            }
            catch (Exception ex)
            {
                APILogger.LogError(ex);
            }

            return null;
        }

        public static void DirSearch(string sDir, string searchPattern, List<string> found)
        {
            try
            {
                foreach (string d in Directory.GetDirectories(sDir))
                {
                    foreach (string f in Directory.GetFiles(d, searchPattern))
                        found.Add(f);
                  
                    DirSearch(d, searchPattern, found);
                }
            }
            catch (System.Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterStartup, NAMESPACE + ".AfterStartup")]
        public static void AfterStartup()
        {
            RUNNING = true;
            CommandManager.RegisterCommand(new GameDifficultyChatCommand());
            CommandManager.RegisterCommand(new ArmorCommand());
            CommandManager.RegisterCommand(new BossesChatCommand());  
#if Debug
            ChatCommands.CommandManager.RegisterCommand(new Research.PandaResearchCommand());
#endif
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnQuit, NAMESPACE + ".OnQuitLate")]
        public static void OnQuitLate()
        {
            RUNNING     = false;
            WorldLoaded = false;
        }

        public static Dictionary<string, List<JSONNode>> GetJSONSettings(string fileType)
        {
            Dictionary<string, List<JSONNode>> retval = new Dictionary<string, List<JSONNode>>();

            try
            {
                foreach (var info in GameInitializer.AllModInfos)
                    if (info.Value.TryGetAs(GameInitializer.NAMESPACE + ".jsonFiles", out JSONNode jsonFilles))
                    {
                        foreach (var jsonNode in jsonFilles.LoopArray())
                        {
                            if (jsonNode.TryGetAs("fileType", out string jsonFileType))
                            {
                                if (jsonFileType == fileType)
                                {
                                    if (!retval.ContainsKey(info.Key))
                                        retval.Add(info.Key, new List<JSONNode>());

                                    retval[info.Key].Add(jsonNode);
                                    APILogger.LogToFile("Getting json configurations {0} from file {1}", fileType, info.Key);
                                }
                            }
                            else
                            {
                                APILogger.Log(ChatColor.red, "Unable to read fileType from file {0}", info.Value);
                            }
                        }
                    }
            }
            catch (Exception ex)
            {
                APILogger.LogError(ex);
            }

            return retval;
        }

        public static Dictionary<string, List<string>> GetJSONSettingPaths(string fileType)
        {
            Dictionary<string, List<string>> retval = new Dictionary<string, List<string>>();

            try
            {
                foreach (var info in GameInitializer.AllModInfos)
                    if (info.Value.TryGetAs(GameInitializer.NAMESPACE + ".jsonFiles", out JSONNode jsonFilles))
                    {
                        foreach (var jsonNode in jsonFilles.LoopArray())
                        {
                            if (jsonNode.TryGetAs("fileType", out string jsonFileType))
                            {
                                if (jsonFileType == fileType)
                                    if (jsonNode.TryGetAs("relativePath", out string itemsPath))
                                    {
                                        if (!retval.ContainsKey(info.Key))
                                            retval.Add(info.Key, new List<string>());

                                        retval[info.Key].Add(itemsPath);
                                        APILogger.LogToFile("Getting json configurations {0} from file {1}", fileType, info.Key);
                                    }
                                    else
                                    {
                                        APILogger.Log(ChatColor.red, "Unable to read relativePath for fileType {0} from file {1}", itemsPath, info.Key);
                                    }
                            }
                            else
                            {
                                APILogger.Log(ChatColor.red, "Unable to read fileType from file {0}", info.Key);
                            }
                        }
                    }
            }
            catch (Exception ex)
            {
                APILogger.LogError(ex);
            }

            return retval;
        }
    }
}
