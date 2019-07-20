using Chatting;
using NPC;
using Pandaros.API.Entities;
using Pandaros.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Pandaros.API.Entities.ColonistInventory;

namespace Pandaros.API.Items.Armor
{
    public class ArmorCommand : IChatCommand
    {
        localization.LocalizationHelper _localizationHelper = new localization.LocalizationHelper(GameInitializer.NAMESPACE, "Armor");

        public bool TryDoCommand(Players.Player player, string chat, List<string> split)
        {
            if (!chat.StartsWith("/armor", StringComparison.OrdinalIgnoreCase))
                return false;

            var colony = player.ActiveColony;
            var counts = new Dictionary<string, Dictionary<ArmorFactory.ArmorSlot, int>>();
            foreach (var npc in colony.Followers)
            {
                var inv = Get(npc);

                foreach (var item in inv.Armor)
                    if (!item.Value.IsEmpty())
                    {
                        var armor = ArmorFactory.ArmorLookup[item.Value.Id];

                        if (!counts.ContainsKey(armor.name))
                            counts.Add(armor.name, new Dictionary<ArmorFactory.ArmorSlot, int>());

                        if (!counts[armor.name].ContainsKey(armor.Slot))
                            counts[armor.name].Add(armor.Slot, 0);

                        counts[armor.name][armor.Slot]++;
                    }
            }

            var state = PlayerState.GetPlayerState(player);
            var psb = new StringBuilder();
            psb.Append("Player =>");

            foreach (var armor in state.Armor)
                if (armor.Value.IsEmpty())
                    psb.Append($" {armor.Key}: None |");
                else
                    psb.Append($" {armor.Key}: {ArmorFactory.ArmorLookup[armor.Value.Id].name} | ");

            PandaChat.Send(player, _localizationHelper, psb.ToString());

            foreach (var type in counts)
            {
                var sb = new StringBuilder();
                sb.Append($"{type.Key} =>");
                foreach (var slot in type.Value) sb.Append($" {slot.Key}: {slot.Value} |");

                PandaChat.Send(player, _localizationHelper, sb.ToString());
            }

            return true;
        }
    }

    [ModLoader.ModManager]
    public static class ArmorFactory
    {
        public enum ArmorSlot
        {
            Helm,
            Chest,
            Gloves,
            Legs,
            Boots,
            Shield
        }

        public static DateTime _nextUpdate = DateTime.MinValue;

        private static readonly Dictionary<ArmorSlot, int> _hitChance = new Dictionary<ArmorSlot, int>
        {
            {ArmorSlot.Helm, 10},
            {ArmorSlot.Chest, 55},
            {ArmorSlot.Gloves, 65},
            {ArmorSlot.Legs, 90},
            {ArmorSlot.Boots, 100}
        };

        private static readonly Dictionary<ArmorSlot, int> _hitChanceShield = new Dictionary<ArmorSlot, int>
        {
            {ArmorSlot.Helm, 10},
            {ArmorSlot.Chest, 30},
            {ArmorSlot.Gloves, 35},
            {ArmorSlot.Legs, 45},
            {ArmorSlot.Boots, 50},
            {ArmorSlot.Shield, 100}
        };

        private static readonly System.Random _rand = new System.Random();

        public static Array ArmorSlotEnum { get; } = Enum.GetValues(typeof(ArmorSlot));

        public static Dictionary<ushort, IArmor> ArmorLookup { get; set; } = new Dictionary<ushort, IArmor>();

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnUpdate, GameInitializer.NAMESPACE + ".Armor.GetArmor")]
        public static void GetArmor()
        {
            if (_nextUpdate < DateTime.Now && World.Initialized)
            {
                Task.Run(() =>
                {
                    foreach (var p in Players.PlayerDatabase.Values.Where(c => c.ActiveColony != null))
                    {
                        var colony = p.ActiveColony;
                        var state = PlayerState.GetPlayerState(p);
                        var stockpile = colony.Stockpile;

                        /// Load up player first.
                        foreach (ArmorSlot slot in ArmorSlotEnum)
                        {
                            if (!state.Armor[slot].IsEmpty() && ArmorLookup.TryGetValue(state.Armor[slot].Id, out var existingArmor) && existingArmor.IsMagical)
                                continue;

                            var bestArmor = GetBestArmorFromStockpile(stockpile, slot, 0);

                            if (bestArmor != default(ushort))
                            {
                                if (!state.Armor.ContainsKey(slot))
                                    state.Armor.Add(slot, new ItemState());

                                // Check if we need one or if there is an upgrade.
                                if (state.Armor[slot].IsEmpty())
                                {
                                    stockpile.TryRemove(bestArmor);
                                    state.Armor[slot].Id = bestArmor;
                                    state.Armor[slot].Durability = ArmorLookup[bestArmor].Durability;
                                }
                                else
                                {
                                    var currentArmor = ArmorLookup[state.Armor[slot].Id];
                                    var stockpileArmor = ArmorLookup[bestArmor];

                                    if (stockpileArmor.ArmorRating > currentArmor.ArmorRating)
                                    {
                                        // Upgrade armor.
                                        stockpile.TryRemove(bestArmor);
                                        stockpile.Add(state.Armor[slot].Id);
                                        state.Armor[slot].Id = bestArmor;
                                        state.Armor[slot].Durability = stockpileArmor.Durability;
                                    }
                                }
                            }
                        }

                        foreach (var npc in colony.Followers)
                        {
                            if (npc.TryGetNPCGuardSettings(out var guardJobSettings))
                            {
                                var inv = Get(npc);
                                GetBestArmorForNPC(stockpile, npc, inv, 2);
                                Weapons.WeaponFactory.GetBestWeapon(npc, 2);
                            }
                        }

                        foreach (var npc in colony.Followers)
                        {
                            var inv = Get(npc);
                            GetBestArmorForNPC(stockpile, npc, inv, 4);
                            Weapons.WeaponFactory.GetBestWeapon(npc, 4);
                        }
                    }
                });

                _nextUpdate = DateTime.Now + TimeSpan.FromSeconds(30);
            }
        }

        public static void GetBestArmorForNPC(Stockpile stockpile, NPCBase npc, ColonistInventory inv, int limit)
        {
            foreach (ArmorSlot slot in ArmorSlotEnum)
            {
                if (!inv.Armor[slot].IsEmpty() && ArmorLookup[inv.Armor[slot].Id].IsMagical)
                    continue;

                var bestArmor = GetBestArmorFromStockpile(stockpile, slot, limit);

                if (bestArmor != default(ushort))
                {
                    if (!inv.Armor.ContainsKey(slot))
                        inv.Armor.Add(slot, new ItemState());

                    // Check if we need one or if there is an upgrade.
                    if (inv.Armor[slot].IsEmpty())
                    {
                        stockpile.TryRemove(bestArmor);
                        inv.Armor[slot].Id = bestArmor;
                        inv.Armor[slot].Durability = ArmorLookup[bestArmor].Durability;
                    }
                    else
                    {
                        var currentArmor = ArmorLookup[inv.Armor[slot].Id];
                        var stockpileArmor = ArmorLookup[bestArmor];

                        if (stockpileArmor.ArmorRating > currentArmor.ArmorRating)
                        {
                            // Upgrade armor.
                            stockpile.TryRemove(bestArmor);
                            stockpile.Add(inv.Armor[slot].Id);
                            inv.Armor[slot].Id = bestArmor;
                            inv.Armor[slot].Durability = stockpileArmor.Durability;
                        }
                    }
                }
            }
        }

        public static ushort GetBestArmorFromStockpile(Stockpile s, ArmorSlot slot, int limit)
        {
            var best = default(ushort);

            foreach (var armor in ArmorLookup.Where(a => a.Value.Slot == slot))
                if (s.Contains(armor.Key) && s.AmountContained(armor.Key) > limit)
                    if (best == default(ushort) || (!armor.Value.IsMagical && armor.Value.ArmorRating > ArmorLookup[best].ArmorRating))
                        best = armor.Key;

            return best;
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerHit, GameInitializer.NAMESPACE + ".Armor.OnPlayerHit")]
        [ModLoader.ModCallbackDependsOn(GameInitializer.NAMESPACE + ".Managers.MonsterManager.OnPlayerHit")]
        public static void OnPlayerHit(Players.Player player, ModLoader.OnHitData box)
        {
            var state = PlayerState.GetPlayerState(player);
            DeductArmor(box, state.Armor);
            state.IncrimentStat("Damage Taken", box.HitDamage);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnNPCHit, GameInitializer.NAMESPACE + ".Armor.OnNPCHit")]
        [ModLoader.ModCallbackDependsOn(GameInitializer.NAMESPACE + ".Managers.MonsterManager.OnNPCHit")]
        public static void OnNPCHit(NPCBase npc, ModLoader.OnHitData box)
        {
            var inv = Get(npc);
            DeductArmor(box, inv.Armor);
            inv.IncrimentStat("Damage Taken", box.HitDamage);
        }

        private static void DeductArmor(ModLoader.OnHitData box, EventedDictionary<ArmorSlot, ItemState> entityArmor)
        {
            if (box.ResultDamage > 0)
            {
                float armor = 0;
                bool missed = false;
                var weap = Weapons.WeaponFactory.GetWeapon(box);

                foreach (ArmorSlot armorSlot in ArmorSlotEnum)
                {
                    if (!entityArmor.ContainsKey(armorSlot))
                        entityArmor.Add(armorSlot, new ItemState());

                    if (!entityArmor[armorSlot].IsEmpty())
                    {
                        var item = ArmorLookup[entityArmor[armorSlot].Id];
                        armor += item.ArmorRating;

                        if (item.MissChance != 0 && item.MissChance > Pipliz.Random.NextFloat())
                        {
                            missed = true;
                            break;
                        }

                    }
                }

                if (!missed && armor != 0)
                {
                    box.ResultDamage = box.ResultDamage - box.ResultDamage * armor;

                    var hitLocation = _rand.Next(1, 100);

                    var dic = _hitChance;

                    if (!entityArmor[ArmorSlot.Shield].IsEmpty())
                        dic = _hitChanceShield;

                    foreach (var loc in dic)
                        if (!entityArmor[loc.Key].IsEmpty() && loc.Value >= hitLocation)
                        {
                            entityArmor[loc.Key].Durability--;

                            if (entityArmor[loc.Key].Durability <= 0)
                            {
                                entityArmor[loc.Key].Durability = 0;
                                entityArmor[loc.Key].Id = default(ushort);
                            }

                            break;
                        }
                }

                if (missed)
                    box.ResultDamage = 0;
            }
        }


    }
}