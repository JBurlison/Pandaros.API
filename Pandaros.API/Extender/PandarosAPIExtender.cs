using NetworkUI;
using Pipliz;
using Pipliz.JSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Pandaros.API.Extender
{
    [ModLoader.ModManager]
    public static class PandarosAPIExtender
    {
        private static Dictionary<string, List<IPandarosExtention>> _settlersExtensions = new Dictionary<string, List<IPandarosExtention>>();
        private static List<IOnTimedUpdate> _timedUpdate = new List<IOnTimedUpdate>();

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnUpdate, GameInitializer.NAMESPACE + ".Extender.SettlersExtender.OnUpdate")]
        public static void OnUpdate()
        {
            foreach (var extension in _timedUpdate)
                try
                {
                    if (extension.NextUpdateTime < Time.SecondsSinceStartDouble)
                    {
                        extension.OnTimedUpdate();
                        extension.NextUpdateTime = Time.SecondsSinceStartDouble + Pipliz.Random.NextDouble(extension.NextUpdateTimeMin, extension.NextUpdateTimeMax);
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
            if (_settlersExtensions.TryGetValue(nameof(IOnSavingColonyExtender), out var pandarosExtentions))
                foreach (var extension in pandarosExtentions.Select(ex => ex as IOnSavingColonyExtender))
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
            if (_settlersExtensions.TryGetValue(nameof(IOnAddResearchablesExtender), out var pandarosExtentions))
                foreach (var extension in pandarosExtentions.Select(ex => ex as IOnAddResearchablesExtender))
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
            if (_settlersExtensions.TryGetValue(nameof(IOnLoadingColonyExtender), out var pandarosExtentions))
                foreach (var extension in pandarosExtentions.Select(ex => ex as IOnLoadingColonyExtender))
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
            if (_settlersExtensions.TryGetValue(nameof(IAfterWorldLoadExtender), out var pandarosExtentions))
                foreach (var extension in pandarosExtentions.Select(ex => ex as IAfterWorldLoadExtender))
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

            if (_settlersExtensions.TryGetValue(nameof(IAfterModsLoadedExtention), out var pandarosExtentions))
                foreach (var extension in pandarosExtentions.Select(ex => ex as IAfterModsLoadedExtention))
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
        public static void AfterItemTypesDefined()
        {
            if (_settlersExtensions.TryGetValue(nameof(IAfterItemTypesDefinedExtender), out var pandarosExtentions))
                foreach (var extension in pandarosExtentions.Select(ex => ex as IAfterItemTypesDefinedExtender))
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
            if (_settlersExtensions.TryGetValue(nameof(IAfterSelectedWorldExtender), out var pandarosExtentions))
                foreach (var extension in pandarosExtentions.Select(ex => ex as IAfterSelectedWorldExtender))
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
            if (_settlersExtensions.TryGetValue(nameof(IOnColonyCreatedExtender), out var pandarosExtentions))
                foreach (var extension in pandarosExtentions.Select(ex => ex as IOnColonyCreatedExtender))
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
            if (_settlersExtensions.TryGetValue(nameof(IAddItemTypesExtender), out var pandarosExtentions))
                foreach (var extension in pandarosExtentions.Select(ex => ex as IAddItemTypesExtender))
                    try
                {
                    extension.AddItemTypes(itemTypes);
                }
                catch (Exception ex)
                {
                    APILogger.LogError(ex);
                }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnConstructInventoryManageColonyUI, GameInitializer.NAMESPACE + ".Extender.SettlersExtender.OnConstructInventoryManageColonyUI")]
        public static void OnConstructInventoryManageColonyUI(Players.Player player, NetworkMenu networkMenu)
        {
            if (_settlersExtensions.TryGetValue(nameof(IOnConstructInventoryManageColonyUIExtender), out var pandarosExtentions))
                foreach (var extension in pandarosExtentions.Select(ex => ex as IOnConstructInventoryManageColonyUIExtender))
                    try
                    {
                        extension.OnConstructInventoryManageColonyUI(player, networkMenu);
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
                            foreach (var vals in _settlersExtensions.Values)
                                foreach (var e in vals)
                                    if (!string.IsNullOrEmpty(e.InterfaceName) && e.InterfaceName == iface.Name && !type.IsInterface)
                                    {
                                        var constructor = type.GetConstructor(Type.EmptyTypes);

                                        if (constructor != null)
                                            e.LoadedAssembalies.Add(type);
                                        else
                                            APILogger.LogToFile("Warning: No empty constructor for " + type.Name);
                                    }

                            if (!string.IsNullOrEmpty(iface.Name) && nameof(IOnTimedUpdate) == iface.Name && !type.IsInterface)
                            {
                                var constructor = type.GetConstructor(Type.EmptyTypes);

                                if (constructor != null && Activator.CreateInstance(type) is IOnTimedUpdate onUpdateCallback)
                                {
                                    APILogger.LogToFile("OnTimedUpdateLoaded: {0}", onUpdateCallback.GetType().Name);
                                    _timedUpdate.Add(onUpdateCallback);
                                }
                            }
                        }

                        foreach (var vals in _settlersExtensions.Values)
                            foreach (var e in vals)
                                if (e.ClassType != null && type.Equals(e.ClassType))
                                    e.LoadedAssembalies.Add(type);
                    }
                }
                catch (Exception)
                {
                    // Do not log it is not the correct type.
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
                            if (isExtention && Activator.CreateInstance(type) is IPandarosExtention extension)
                            {
                                foreach (var iface in ifaces)
                                    if (iface.Name != nameof(IPandarosExtention))
                                        try
                                        {
                                            if (!_settlersExtensions.ContainsKey(iface.Name))
                                                _settlersExtensions.Add(iface.Name, new List<IPandarosExtention>());

                                            if (!_settlersExtensions[iface.Name].Any(f => f.GetType().Name == extension.GetType().Name))
                                                _settlersExtensions[iface.Name].Add(extension);
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
        }
    }
}