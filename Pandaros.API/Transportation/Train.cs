using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pandaros.API.Models;
using Pandaros.API.Server;
using Pipliz;
using Shared;
using System.Collections.Generic;
using System.IO;
using Transport;
using UnityEngine;

namespace Pandaros.API.Transportation
{
    [ModLoader.ModManager]
    public static class Train
    {
        public static Dictionary<string, ICSType> TrainTypes { get; set; } = new Dictionary<string, ICSType>();
        public static Dictionary<string, AnimationManager.AnimatedObject> TrainAnimations { get; set; } = new Dictionary<string, AnimationManager.AnimatedObject>();
        public static Dictionary<string, List<TrainTransport>> TrainTransports { get; set; } = new Dictionary<string, List<TrainTransport>>();

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnShouldKeepChunkLoaded, GameInitializer.NAMESPACE + ".Jobs.Construction.SchematicBuilder.OnShouldKeepChunkLoaded")]
        public static void OnShouldKeepChunkLoaded(ChunkUpdating.KeepChunkLoadedData data)
        {
            foreach (var list in TrainTransports.Values)
                foreach (var iterator in list)
                {
                    var bounds = new Pipliz.BoundsInt(iterator.TrackPosition.Add(-30, -30, -30), iterator.TrackPosition.Add(30, 30, 30));
                    
                    if (Pipliz.BoundsInt.Intersects(bounds, data.CheckedChunk.Bounds))
                        data.Result = true;
                }
        }


        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameInitializer.NAMESPACE + ".Transportation.Train.Initialize", 5000)]
        private static void Initialize()
        {
            foreach (var train in TrainTypes.Values)
            {
                var animatedObject = AnimationManager.RegisterNewAnimatedObject(train.name, train.mesh, train.sideall);
                animatedObject.ObjSettings.colliders = new List<RotatedBounds>() { new RotatedBounds(Vector3.zero, train.TrainConfiguration.TrainBounds, Quaternion.identity) };
                animatedObject.ObjSettings.InterpolationLooseness = 1.5f;
                animatedObject.ObjSettings.sendUpdateRadius = 500;

                TrainAnimations[train.name] = animatedObject;
            }

            var worldFile = Path.Combine(GameInitializer.SAVE_LOC, "world.json");

            if (File.Exists(worldFile))
            {
                JObject rootObj = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(worldFile));

                if (rootObj.TryGetValue("transports", out JToken transports))
                {
                    if (transports.Type != JTokenType.Array)
                        return;

                    List<TransportSave> trainSaves = transports.ToObject<List<TransportSave>>();

                    foreach (var save in trainSaves)
                    {
                        if (TrainTransport.TryCreateFromSave(save, out var trainTransport))
                            TransportManager.RegisterTransport(trainTransport);
                    }
                }
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerClicked, GameInitializer.NAMESPACE + ".Transportation.Train.OnPlayerClicked")]
        private static void OnClicked(Players.Player sender, PlayerClickedData data)
        {
            if (data == null ||
                data.TypeSelected == default(ushort) ||
                data.ConsumedType != PlayerClickedData.EConsumedType.Not ||
                data.IsHoldingButton ||
                (data.ClickType != PlayerClickedData.EClickType.Right || data.OnBuildCooldown) ||
                (data.HitType != PlayerClickedData.EHitType.Block ||
                !TrainTypes.TryGetValue(ItemId.GetItemId(data.TypeSelected), out var cSType) || !sender.Inventory.TryRemove(data.TypeSelected, 1, -1, true)))
                return;

            data.ConsumedType = PlayerClickedData.EConsumedType.UsedAsTool;
            CreateTrain(cSType, data.GetExactHitPositionWorld());
        }

        public static TrainTransport CreateTrain(ICSType cSType, Vector3 spawnPosition)
        {
            TrainTransport trainTransport = new TrainTransport(spawnPosition, TrainAnimations[cSType.name], cSType);
            TransportManager.RegisterTransport(trainTransport);

            if (!string.IsNullOrEmpty(cSType.ConnectedBlock?.BlockType))
            {
                if (!TrainTransports.ContainsKey(cSType.ConnectedBlock.BlockType))
                    TrainTransports.Add(cSType.ConnectedBlock.BlockType, new List<TrainTransport>());

                TrainTransports[cSType.ConnectedBlock.BlockType].Add(trainTransport);
            }

            return trainTransport;
        }
    }
}
