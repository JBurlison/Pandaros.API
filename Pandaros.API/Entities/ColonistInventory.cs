using Newtonsoft.Json;
using NPC;
using Pandaros.API.AI;
using Pandaros.API.ColonyManagement;
using Pandaros.API.Items.Armor;
using Pandaros.API.Models;
using Pipliz.JSON;
using System;
using System.Collections.Generic;

namespace Pandaros.API.Entities
{
    public class ColonistInventory
    {
        public ColonistInventory(NPCBase id)
        {
            SettlerId   = id.ID;
            NPC = id;
            ColonistsName = NameGenerator.GetName();
            SetupArmor();
        }

        public ColonistInventory(JSONNode baseNode, NPCBase nPC)
        {
            if (baseNode.TryGetAs<int>(nameof(SettlerId), out var settlerId))
            {
                NPC = nPC;
                SetupArmor();
                SettlerId = settlerId;

                baseNode.TryGetAs<string>(nameof(ColonistsName), out var name);
                ColonistsName = name;

                if (baseNode.TryGetAs(nameof(BonusProcs), out JSONNode skills))
                    foreach (var skill in skills.LoopObject())
                        if (ushort.TryParse(skill.Key, out ushort item))
                        BonusProcs[item] = skill.Value.GetAs<long>();

                if (baseNode.TryGetAs(nameof(Stats), out JSONNode itterations))
                    foreach (var skill in itterations.LoopObject())
                        Stats[skill.Key] = skill.Value.GetAs<double>();

                foreach (ArmorFactory.ArmorSlot armorType in ArmorFactory.ArmorSlotEnum)
                    Armor[armorType].FromJsonNode(armorType.ToString(), baseNode);
            }
        }

        public double MagicItemUpdateTime { get; set; } = Pipliz.Time.SecondsSinceStartDouble + Pipliz.Random.Next(1, 10);
        public double HealingItemUpdateTime { get; set; } = Pipliz.Time.SecondsSinceStartDouble + Pipliz.Random.Next(1, 10);
        public int SettlerId { get; set; }

        public double PunchCooldown { get; set; }

        [JsonIgnore]
        public NPCBase NPC { get; private set; }

        public string ColonistsName { get; set; }

        public double UnemployedLeaveTime { get; set; }

        public Dictionary<ushort, long> BonusProcs { get; set; } = new Dictionary<ushort, long>();

        public Dictionary<string, double> Stats { get; set; } = new Dictionary<string, double>();

        public EventedDictionary<ArmorFactory.ArmorSlot, ItemState> Armor { get; set; } =  new EventedDictionary<ArmorFactory.ArmorSlot, ItemState>();

        public ItemState Weapon { get; set; } = new ItemState();

        public void IncrimentStat(string name, double count = 1)
        {
            if (!Stats.ContainsKey(name))
                Stats.Add(name, 0);

            Stats[name] += count;
        }

        private void SetupArmor()
        {
            foreach (ArmorFactory.ArmorSlot armorType in ArmorFactory.ArmorSlotEnum)
                Armor.Add(armorType, new ItemState());

            Armor.OnDictionaryChanged += Armor_OnDictionaryChanged;
        }

        public void AddBonusProc(ushort item, long count = 1)
        {
            if (!BonusProcs.ContainsKey(item))
                BonusProcs.Add(item, 0);

            BonusProcs[item] += count;
        }

        public float GetSkillModifier()
        {
            var totalSkill = SkillChance.GetSkillChance(NPC.Colony);

            if (NPC.CustomData.TryGetAs(GameInitializer.ALL_SKILLS, out float allSkill))
                totalSkill = allSkill;

            foreach (var armor in Armor)
                if (Items.Armor.ArmorFactory.ArmorLookup.TryGetValue(armor.Value.Id, out var a))
                    totalSkill += a.Skilled;

            if (Items.Weapons.WeaponFactory.WeaponLookup.TryGetValue(Weapon.Id, out var w))
                totalSkill += w.Skilled;

            return totalSkill;
        }

        // TODO: apply armor
        private void Armor_OnDictionaryChanged(object sender, DictionaryChangedEventArgs<ArmorFactory.ArmorSlot, ItemState> e)
        {
            switch (e.EventType)
            {
                case DictionaryEventType.AddItem:
                    
                    break;

                case DictionaryEventType.ChangeItem:

                    break;

                case DictionaryEventType.RemoveItem:

                    break;
            }
        }

        public JSONNode ToJsonNode()
        {
            var baseNode = new JSONNode();

            try
            {
                baseNode[nameof(SettlerId)] = new JSONNode(SettlerId);
                baseNode[nameof(ColonistsName)] = new JSONNode(ColonistsName);

                var skills = new JSONNode();

                foreach (var job in BonusProcs)
                    skills[job.Key.ToString()] = new JSONNode(job.Value);

                baseNode[nameof(BonusProcs)] = skills;

                var statsNode = new JSONNode();

                foreach (var job in Stats)
                    statsNode[job.Key] = new JSONNode(job.Value);

                baseNode[nameof(Stats)] = statsNode;

                foreach (ArmorFactory.ArmorSlot armorType in ArmorFactory.ArmorSlotEnum)
                    baseNode[armorType.ToString()] = Armor[armorType].ToJsonNode();
            }
            catch (Exception ex)
            {
                APILogger.LogError(ex);
            }

            return baseNode;
        }

        public static ColonistInventory Get(NPCBase npc)
        {
            ColonistInventory inv = null;

            if (npc == null)
                return inv;

            if (npc.CustomData == null)
                npc.CustomData = new JSONNode();

            try
            {
                if (!npc.CustomData.TryGetAs(GameInitializer.SETTLER_INV, out inv) || inv == null)
                {
                    inv = new ColonistInventory(npc);
                    npc.CustomData.SetAs(GameInitializer.SETTLER_INV, inv);
                }
            }
            catch (Exception ex)
            {
                APILogger.LogError(ex);
            }

            if (inv == null)
            {
                inv = new ColonistInventory(npc);
            }

            return inv;
        }

        
    }
}