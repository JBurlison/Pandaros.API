using Assets.ColonyPointUpgrades;
using ModLoaderInterfaces;
using NetworkUI;
using NetworkUI.Items;
using Pipliz;
using Pipliz.JSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Pandaros.API.Extender
{
    [ModLoader.ModManager]
    public class PandarosAPIExtender : ModLoaderInterfaces.IOnLoadModJSONFiles, IOnRegisterUpgrades
    {
        public static Dictionary<string, List<IPandarosExtention>> SettlersExtensions { get; set; } = new Dictionary<string, List<IPandarosExtention>>();
        public static List<IOnTimedUpdate> TimedUpdate { get; set; } = new List<IOnTimedUpdate>();

        /// <summary>
        ///     Gets list of callbacks by type of extention.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>Empty if not found or if found, list of extentions</returns>
        public static IEnumerable<T> GetCallbacks<T>() where T : IPandarosExtention
        {
            if (SettlersExtensions.TryGetValue(typeof(T).Name, out var pandarosExtentions))
                return pandarosExtentions.Select(ex => (T)ex);
            
            return Enumerable.Empty<T>();
        }

        [ModLoader.ModCallback(GameInitializer.NAMESPACE + ".Extender.SettlersExtender.OnLoadModJSONFiles")]
        public void OnLoadModJSONFiles(List<ModLoader.LoadModJSONFileContext> contexts)
        {
            foreach (var extension in GetCallbacks<IOnLoadModJSONFilesExtender>())
                try
                {
                    extension.OnLoadModJSONFiles(contexts);
                }
                catch (Exception ex)
                {
                    APILogger.LogError(ex);
                }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnSendAreaHighlights, GameInitializer.NAMESPACE + ".Extender.SettlersExtender.OnSendAreaHighlights")]
        public static void OnSendAreaHighlights(Players.Player player, List<AreaJobTracker.AreaHighlight> list, List<ushort> showWhileHoldingTypes)
        {
            foreach (var extension in GetCallbacks<IOnSendAreaHighlightsExtender>())
                try
                {
                    extension.OnSendAreaHighlights(player, list, showWhileHoldingTypes);
                }
                catch (Exception ex)
                {
                    APILogger.LogError(ex);
                }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnTryChangeBlock, GameInitializer.NAMESPACE + ".Extender.SettlersExtender.OnTryChangeBlock")]
        public static void OnTryChangeBlock(ModLoader.OnTryChangeBlockData tryChangeBlockData)
        {
            foreach (var extension in GetCallbacks<IOnTryChangeBlockExtender>())
                try
                {
                    extension.OnTryChangeBlock(tryChangeBlockData);
                }
                catch (Exception ex)
                {
                    APILogger.LogError(ex);
                }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnChangedBlock, GameInitializer.NAMESPACE + ".Extender.SettlersExtender.OnChangedBlock")]
        public static void OnChangedBlock(ModLoader.OnTryChangeBlockData tryChangeBlockData)
        {
            foreach (var extension in GetCallbacks<IOnChangedBlockExtender>())
                try
                {
                    extension.OnChangedBlock(tryChangeBlockData);
                }
                catch (Exception ex)
                {
                    APILogger.LogError(ex);
                }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnUpdate, GameInitializer.NAMESPACE + ".Extender.SettlersExtender.OnUpdate")]
        public static void OnUpdate()
        {
            foreach (var extension in TimedUpdate)
                try
                {
                    if (extension.NextUpdateTime == default(ServerTimeStamp))
                        extension.NextUpdateTime = ServerTimeStamp.Now;

                    if (extension.NextUpdateTime.IsPassed)
                    {
                        extension.NextUpdateTime = ServerTimeStamp.Now.Add(Pipliz.Random.Next(extension.NextUpdateTimeMinMs, extension.NextUpdateTimeMaxMs));
                        extension.OnTimedUpdate();
                    }
                }
                catch (Exception ex)
                {
                    APILogger.LogError(ex);
                }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnSavingColony, GameInitializer.NAMESPACE + ".Extender.SettlersExtender.OnSavingColony")]
        public static void OnSavingColony(Colony c, JSONNode n)
        {
            foreach (var extension in GetCallbacks<IOnSavingColonyExtender>())
                try
                {
                    extension.OnSavingColony(c, n);
                }
                catch (Exception ex)
                {
                    APILogger.LogError(ex);
                }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnAddResearchables, GameInitializer.NAMESPACE + ".Extender.SettlersExtender.OnAddResearchables")]
        public static void Register()
        {
            foreach (var extension in GetCallbacks<IOnAddResearchablesExtender>())
                try
                {
                    extension.OnAddResearchables();
                }
                catch (Exception ex)
                {
                    APILogger.LogError(ex);
                }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnLoadingColony, GameInitializer.NAMESPACE + ".Extender.SettlersExtender.OnLoadingColony")]
        public static void OnLoadingColony(Colony c, JSONNode n)
        {
            foreach (var extension in GetCallbacks<IOnLoadingColonyExtender>())
                try
                {
                    extension.OnLoadingColony(c, n);
                }
                catch (Exception ex)
                {
                    APILogger.LogError(ex);
                }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad,   GameInitializer.NAMESPACE + ".Extender.SettlersExtender.AfterWorldLoad")]
        [ModLoader.ModCallbackProvidesFor(GameInitializer.NAMESPACE + ".Managers.MonsterManager.AfterWorldLoad")]
        public static void AfterWorldLoad()
        {
            foreach (var extension in GetCallbacks<IAfterWorldLoadExtender>())
                try
                {
                    extension.AfterWorldLoad();
                }
                catch (Exception ex)
                {
                    APILogger.LogError(ex);
                }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterModsLoaded, GameInitializer.NAMESPACE + ".Extender.SettlersExtender.AfterModsLoaded")]
        public static void AfterModsLoaded(List<ModLoader.ModDescription> list)
        {
            LoadExtenstions(list);
            LoadImplementation(list);

            foreach (var extension in GetCallbacks<IAfterModsLoadedExtention>())
                try
                {
                    extension.AfterModsLoaded(list);
                }
                catch (Exception ex)
                {
                    APILogger.LogError(ex);
                }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameInitializer.NAMESPACE + ".Extender.SettlersExtender.AfterItemTypesDefined")]
        [ModLoader.ModCallbackProvidesFor("pipliz.server.loadresearchables")]
        [ModLoader.ModCallbackProvidesFor("pipliz.server.loadnpctypes")]
        [ModLoader.ModCallbackProvidesFor("blockentitycallback.autoloaders")]
        public static void AfterItemTypesDefined()
        {
            foreach (var extension in GetCallbacks<IAfterItemTypesDefinedExtender>())
                try
                {
                    extension.AfterItemTypesDefined();
                }
                catch (Exception ex)
                {
                    APILogger.LogError(ex);
                }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterSelectedWorld, GameInitializer.NAMESPACE + ".Extender.SettlersExtender.AfterSelectedWorld")]
        [ModLoader.ModCallbackProvidesFor("pipliz.server.registertexturemappingtextures")]
        public static void AfterSelectedWorld()
        {
            foreach (var extension in GetCallbacks<IAfterSelectedWorldExtender>())
                try
                {
                    extension.AfterSelectedWorld();
                }
                catch (Exception ex)
                {
                    APILogger.LogError(ex);
                }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnCreatedColony, GameInitializer.NAMESPACE + ".Extender.SettlersExtender.OnCreatedColony")]
        public static void OnCreatedColony(Colony c)
        {
            foreach (var extension in GetCallbacks<IOnColonyCreatedExtender>())
                try
                {
                    extension.ColonyCreated(c);
                }
                catch (Exception ex)
                {
                    APILogger.LogError(ex);
                }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AddItemTypes, GameInitializer.NAMESPACE + ".Extender.SettlersExtender.AddItemTypes")]
        [ModLoader.ModCallbackDependsOn("pipliz.server.applymoditempatches")]
        public static void AddItemTypes(Dictionary<string, ItemTypesServer.ItemTypeRaw> itemTypes)
        {
            foreach (var extension in GetCallbacks<IAddItemTypesExtender>())
                try
                {
                    extension.AddItemTypes(itemTypes);
                }
                catch (Exception ex)
                {
                    APILogger.LogError(ex);
                }
        }

        public void OnRegisterUpgrades(UpgradesManager upgrades)
        {
            
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnAssemblyLoaded, GameInitializer.NAMESPACE + ".Extender.SettlersExtender.OnConstructInventoryManageColonyUI")]
        [ModLoader.ModCallbackDependsOn("initialize_network_callbacks")]
        public static void Initialize(string file)
        {
            NetworkMenuManager.OnConstructInventoryManageColonyUI.AddCallback(OnConstructInventoryManageColonyUI, new ModLoader.ModCallbackDescription(GameInitializer.NAMESPACE + ".Extender.SettlersExtender.OnConstructInventoryManageColonyUI.Content"));
        }

        public static void OnConstructInventoryManageColonyUI(Players.Player player, NetworkMenu networkMenu, (Table, Table) table)
        {
            foreach (var extension in GetCallbacks<IOnConstructInventoryManageColonyUIExtender>())
                try
                {
                    extension.OnConstructInventoryManageColonyUI(player, networkMenu, table);
                }
                catch (Exception ex)
                {
                    APILogger.LogError(ex);
                }
        }

        private static void LoadImplementation(List<ModLoader.ModDescription> list)
        {
            foreach (var mod in list.Where(m => m.HasAssembly && !string.IsNullOrEmpty(m.assemblyPath) && !m.assemblyPath.Contains("Pipliz\\modInfo.json")))
                try
                {
                    // Get all Types available in the assembly in an array
                    var typeArray = mod.LoadedAssembly.GetTypes();

                    // Walk through each Type and list their Information
                    foreach (var type in typeArray)
                    {
                        var ifaces = type.GetInterfaces();

                        foreach (var iface in ifaces)
                        {
                            foreach (var vals in SettlersExtensions.Values)
                                foreach (var e in vals)
                                    if (!string.IsNullOrEmpty(e.InterfaceName) && e.InterfaceName == iface.Name && !type.IsInterface)
                                    {
                                        var constructor = type.GetConstructor(Type.EmptyTypes);

                                        if (constructor != null && !e.LoadedAssembalies.Contains(type))
                                            e.LoadedAssembalies.Add(type);
                                        else if (constructor == null)
                                            APILogger.LogToFile("Warning: No empty constructor for " + type.Name);
                                    }

                            if (!string.IsNullOrEmpty(iface.Name) && nameof(IOnTimedUpdate) == iface.Name && !type.IsInterface)
                            {
                                var constructor = type.GetConstructor(Type.EmptyTypes);

                                if (constructor != null && Activator.CreateInstance(type) is IOnTimedUpdate onUpdateCallback)
                                {
                                    APILogger.LogToFile("OnTimedUpdateLoaded: {0}", onUpdateCallback.GetType().Name);
                                    TimedUpdate.Add(onUpdateCallback);
                                }
                            }
                        }

                        foreach (var vals in SettlersExtensions.Values)
                            foreach (var e in vals)
                                if (e.ClassType != null && type.Equals(e.ClassType))
                                    e.LoadedAssembalies.Add(type);
                    }
                }
                catch (Exception)
                {
                    // Do not log it is not the correct type.
                }

            foreach (var iface in SettlersExtensions.Keys)
            {
                var impl = SettlersExtensions[iface];

                foreach (var t in impl)
                {
                    List<(double, Type)> pri = new List<(double, Type)>();

                    foreach (var subType in t.LoadedAssembalies)
                    {
                        var at = subType.GetCustomAttribute(typeof(LoadPriorityAttribute)) as LoadPriorityAttribute;

                        if (at != null)
                        {
                            pri.Add((at.Priority, subType));
                        }
                        else
                            pri.Add((0, subType));
                    }

                    t.LoadedAssembalies.Clear();
                    t.LoadedAssembalies.AddRange(pri.OrderBy(o => o.Item1).Select(o => o.Item2).ToList());
                }
            }
        }

        private static void LoadExtenstions(List<ModLoader.ModDescription> list)
        {
            foreach (var mod in list.Where(m => m.HasAssembly))
                try
                {
                    // Get all Types available in the assembly in an array
                    var typeArray = mod.LoadedAssembly.GetTypes();

                    // Walk through each Type and list their Information
                    foreach (var type in typeArray)
                    {
                        var ifaces = type.GetInterfaces();
                        var isExtention = ifaces.Any(f => f.Name == nameof(IPandarosExtention));

                        try
                        {
                            if (isExtention && type.GetConstructors().Any(c => c.GetParameters() == null || c.GetParameters().Length == 0) && Activator.CreateInstance(type) is IPandarosExtention extension)
                            {
                                foreach (var iface in ifaces)
                                    if (iface.Name != nameof(IPandarosExtention))
                                        try
                                        {
                                            if (!SettlersExtensions.ContainsKey(iface.Name))
                                                SettlersExtensions.Add(iface.Name, new List<IPandarosExtention>());

                                            if (!SettlersExtensions[iface.Name].Any(f => f.GetType().Name == extension.GetType().Name))
                                                SettlersExtensions[iface.Name].Add(extension);
                                        }
                                        catch (MissingMethodException)
                                        {
                                            // do nothing, we tried to load a interface.
                                        }
                                        catch (Exception ex)
                                        {
                                            APILogger.LogError(ex, $"Error loading interface {iface.Name} on type {type.Name}");
                                        }
                            }
                        }
                        catch (Exception) { }
                    }
                }
                catch (Exception)
                {
                    // Do not log it is not the correct type.
                }

           
            foreach (var iface in new List<string>(SettlersExtensions.Keys))
            {
                var impl = SettlersExtensions[iface];
                List<(double, IPandarosExtention)> pri = new List<(double, IPandarosExtention)>();

                foreach (var t in impl)
                {
                    var at = t.GetType().GetCustomAttribute(typeof(LoadPriorityAttribute)) as LoadPriorityAttribute;

                    if (at != null)
                    {
                        pri.Add((at.Priority, t));
                    }
                    else
                        pri.Add((0, t));
                }

                SettlersExtensions[iface] = pri.OrderBy(o => o.Item1).Select(o => o.Item2).ToList();
            }
        }
    }
}