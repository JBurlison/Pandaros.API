﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pandaros.API.Items;
using Pipliz.JSON;
using UnityEngine;
using static Pandaros.API.Items.StaticItems;

namespace Pandaros.API.Models
{
    public enum ItemRarity
    {
        Common = 0,
        Uncommon = 1,
        Rare = 2,
        Artifact = 3
    }

    public interface ICSType : INameable
    {
        bool? blocksPathing { get; set; }
        List<string> categories { get; set; }
        Colliders colliders { get; set; }
        string color { get; set; }
        JObject customData { get; set; }
        int? destructionTime { get; set; }
        string icon { get; set; }
        bool? isDestructible { get; set; }
        bool? isFertile { get; set; }
        bool? isPlaceable { get; set; }
        bool? isRotatable { get; set; }
        bool? isSolid { get; set; }
        bool? requiresFertileBelow { get; set; }
        int? maxStackSize { get; set; }
        float? luxuryHours { get; set; }
        string mesh { get; set; }
        int lightingTransparency { get; set; }
        SerializableVector3 meshRotationEuler { get; set; }
        SerializableVector3 meshOffset { get; set; }
        SerializableVector3 meshScale { get; set; }
        bool? needsBase { get; set; }
        float? foodValue { get; set; }
        float? happiness { get; set; }
        float? dailyFoodFractionOptimal { get; set; }
        string onPlaceAudio { get; set; }
        List<OnRemove> onRemove { get; set; }
        string onRemoveAmount { get; set; }
        string onRemoveAudio { get; set; }
        string onRemoveChance { get; set; }
        string onRemoveType { get; set; }
        string parentType { get; set; }
        [JsonProperty("rotatablex-")]
        string rotatablexn { get; set; }
        [JsonProperty("rotatablex+")]
        string rotatablexp { get; set; }
        [JsonProperty("rotatablez-")]
        string rotatablezn { get; set; }
        [JsonProperty("rotatablez+")]
        string rotatablezp { get; set; }
        string sideall { get; set; }
        [JsonProperty("sidex-")]
        string sidexn { get; set; }
        [JsonProperty("sidex+")]
        string sidexp { get; set; }
        [JsonProperty("sidey-")]
        string sideyn { get; set; }
        [JsonProperty("sidey+")]
        string sideyp { get; set; }
        [JsonProperty("sidez-")]
        string sidezn { get; set; }
        [JsonProperty("sidez+")]
        string sidezp { get; set; }
        ItemRarity Rarity { get; set; }
        StaticItem StaticItemSettings { get; set; }
        OpenMenuSettings OpensMenuSettings { get; set; }
        ConnectedBlock ConnectedBlock { get; set; }
        TrainConfiguration TrainConfiguration { get; set; }
        TrainStationSettings TrainStationSettings { get; set; }
    }
}