using Monsters;
using NPC;
using Pandaros.API.ColonyManagement;
using Pandaros.API.Entities;
using Pandaros.API.Items;
using Pandaros.API.Models;
using Pipliz;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static AI.PathingManager;
using static AI.PathingManager.PathFinder;
using Random = Pipliz.Random;
using Time = Pipliz.Time;

namespace Pandaros.API.Monsters
{
    [ModLoader.ModManager]
    public class MonsterManager : IPathingThreadAction
    {
        private static double _justQueued;
        private static int _nextBossUpdateTime = int.MaxValue;
        private static MonsterManager _monsterManager = new MonsterManager();
        private static Queue<IPandaBoss> _pandaBossesSpawnQueue = new Queue<IPandaBoss>();
        private static localization.LocalizationHelper _localizationHelper = new localization.LocalizationHelper(GameInitializer.NAMESPACE, "Monsters");
        public static Dictionary<ColonyState, IPandaBoss> SpawnedBosses { get; private set; } = new Dictionary<ColonyState, IPandaBoss>();

        private static readonly List<IPandaBoss> _bossList = new List<IPandaBoss>();

        private static int _boss = -1;
        public static bool BossActive { get; private set; }

        public static int MinBossSpawnTimeSeconds { get; set; } = 900;

        public static int MaxBossSpawnTimeSeconds { get; set; } = 1800;

        public static event EventHandler<BossSpawnedEvent> BossSpawned;

        public static void AddBoss(IPandaBoss m)
        {
            lock (_bossList)
            {
                _bossList.Add(m);
            }
        }

        private static IPandaBoss GetMonsterType()
        {
            IPandaBoss t = null;

            if (_bossList.Count != 0)
            lock (_bossList)
            {
                var rand = _boss;

                while (rand == _boss)
                    rand = Random.Next(0, _bossList.Count);

                t     = _bossList[rand];
                _boss = rand;
            }

            return t;
        }

        public IPandaBoss CurrentPandaBoss { get; set; }

        public void PathingThreadAction(PathingContext context)
        {
            if (BossActive)
            {
                foreach (var colony in ServerManager.ColonyTracker.ColoniesByID.Values)
                {
                    var bannerGoal = colony.Banners.FirstOrDefault();

                    if (bannerGoal == null)
                        continue;

                    var cs = ColonyState.GetColonyState(colony);

                    if (cs.BossesEnabled &&
                        cs.ColonyRef.OwnerIsOnline() &&
                        colony.FollowerCount > CurrentPandaBoss.MinColonists)
                    {
                        if (CurrentPandaBoss != null && !SpawnedBosses.ContainsKey(cs))
                        {
                            Vector3Int positionFinal;
                            switch (((MonsterSpawner)MonsterTracker.MonsterSpawner).TryGetSpawnLocation(context, bannerGoal.Position, bannerGoal.SafeRadius, 200, 500f, out positionFinal))
                            {
                                case MonsterSpawner.ESpawnResult.Success:
                                    if (context.Pathing.TryFindPath(positionFinal, bannerGoal.Position, out var path, 2000000000) == EPathFindingResult.Success)
                                    {
                                        var pandaboss = (IPandaBoss)CurrentPandaBoss.GetNewInstance(path, colony);
                                        _pandaBossesSpawnQueue.Enqueue(pandaboss);
                                        SpawnedBosses.Add(cs, pandaboss);
                                    }

                                    break;
                                case MonsterSpawner.ESpawnResult.NotLoaded:
                                case MonsterSpawner.ESpawnResult.Impossible:
                                    colony.OnZombieSpawn(true);
                                    break;
                                case MonsterSpawner.ESpawnResult.Fail:
                                    CantSpawnBoss(cs);
                                    break;
                            }
                        }
                    }
                }
            }
        }


        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnUpdate, GameInitializer.NAMESPACE + ".Managers.MonsterManager.Update")]
        public static void OnUpdate()
        {
            if (!World.Initialized)
                return;

            var secondsSinceStartDouble = Time.SecondsSinceStartDouble;

            if (World.Initialized)
            {
                if (!TimeCycle.IsDay && 
                    !BossActive &&
                    _nextBossUpdateTime <= secondsSinceStartDouble)
                {
                    BossActive = true;
                    var bossType   = GetMonsterType();

                    if (bossType != null)
                    {
                        _monsterManager.CurrentPandaBoss = bossType;
                        ServerManager.PathingManager.QueueAction(_monsterManager);
                        _justQueued = secondsSinceStartDouble + 5;

                        if (Players.CountConnected != 0)
                            APILogger.Log(ChatColor.yellow, $"Boss Active! Boss is: {bossType.name}");
                    }
                    else
                    {
                        BossActive = false;
                        GetNextBossSpawnTime();
                    }
                }

                if (BossActive && _justQueued < secondsSinceStartDouble) 
                {
                    var   turnOffBoss   = true;

                    if (_pandaBossesSpawnQueue.Count > 0)
                    {
                        var pandaboss = _pandaBossesSpawnQueue.Dequeue();
                        var cs = ColonyState.GetColonyState(pandaboss.OriginalGoal);
                        BossSpawned?.Invoke(MonsterTracker.MonsterSpawner, new BossSpawnedEvent(cs, pandaboss));

                        ModLoader.Callbacks.OnMonsterSpawned.Invoke(pandaboss);
                        MonsterTracker.Add(pandaboss);
                        cs.ColonyRef.OnZombieSpawn(true);
                        cs.FaiedBossSpawns = 0;
                        PandaChat.Send(cs, _localizationHelper, $"[{pandaboss.name}] {pandaboss.AnnouncementText}", ChatColor.red);

                        if (!string.IsNullOrEmpty(pandaboss.AnnouncementAudio))
                            cs.ColonyRef.ForEachOwner(o => AudioManager.SendAudio(o.Position, pandaboss.AnnouncementAudio));
                    }

                    foreach (var colony in ServerManager.ColonyTracker.ColoniesByID.Values)
                    {
                        var bannerGoal = colony.Banners.FirstOrDefault();
                        var cs = ColonyState.GetColonyState(colony);

                        if (bannerGoal != null &&
                            cs.BossesEnabled &&
                            cs.ColonyRef.OwnerIsOnline())
                        {

                            if (SpawnedBosses.ContainsKey(cs) &&
                                    SpawnedBosses[cs].IsValid &&
                                    SpawnedBosses[cs].CurrentHealth > 0)
                            {
                                if (colony.TemporaryData.GetAsOrDefault("BossIndicator", 0) < Time.SecondsSinceStartInt)
                                {
                                    Indicator.SendIconIndicatorNear(new Vector3Int(SpawnedBosses[cs].Position),
                                                                    SpawnedBosses[cs].ID,
                                                                    new IndicatorState(1, ItemId.GetItemId(GameInitializer.NAMESPACE + ".Poisoned").Id,
                                                                                        false, false));

                                    colony.TemporaryData.SetAs("BossIndicator", Time.SecondsSinceStartInt + 1);
                                }

                                turnOffBoss = false;
                            }
                        }


                        if (turnOffBoss)
                        {
                            if (Players.CountConnected != 0 && SpawnedBosses.Count != 0)
                            {
                                APILogger.Log(ChatColor.yellow, $"All bosses cleared!");
                                var boss = SpawnedBosses.FirstOrDefault().Value;
                                PandaChat.SendToAll($"[{boss.name}] {boss.DeathText}", _localizationHelper, ChatColor.red);
                            }

                            BossActive = false;
                            SpawnedBosses.Clear();
                            GetNextBossSpawnTime();
                        }
                    }
                }
            }
        }

        private static void CantSpawnBoss(ColonyState cs)
        {
            cs.FaiedBossSpawns++;

            if (cs.FaiedBossSpawns > 10)
                PandaChat.SendThrottle(cs, _localizationHelper, "NoBanner", ChatColor.red, ColonistManager.PenalizeFood(cs.ColonyRef, 0.15f) * 100 + "%");

            cs.ColonyRef.OnZombieSpawn(false);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad, GameInitializer.NAMESPACE + ".Managers.MonsterManager.AfterWorldLoad")]
        public static void AfterWorldLoad()
        {
            GetNextBossSpawnTime();
        }

        private static void GetNextBossSpawnTime()
        {
            _nextBossUpdateTime = Time.SecondsSinceStartInt + Random.Next(MinBossSpawnTimeSeconds, MaxBossSpawnTimeSeconds);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerHit, GameInitializer.NAMESPACE + ".Managers.MonsterManager.OnPlayerHit")]
        public static void OnPlayerHit(Players.Player player, ModLoader.OnHitData d)
        {
            if (d.ResultDamage > 0 && d.HitSourceType == ModLoader.OnHitData.EHitSourceType.Monster && player.ActiveColony != null && !(d.HitSourceObject is IPandaZombie))
            {
                var state = ColonyState.GetColonyState(player.ActiveColony);
                d.ResultDamage += state.Difficulty.MonsterDamage;
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnNPCHit, GameInitializer.NAMESPACE + ".Managers.MonsterManager.OnNPCHit")]
        public static void OnNPCHit(NPCBase npc, ModLoader.OnHitData d)
        {
            if (d.ResultDamage > 0 && d.HitSourceType == ModLoader.OnHitData.EHitSourceType.Monster && !(d.HitSourceObject is IPandaZombie))
            {
                var state = ColonyState.GetColonyState(npc.Colony);
                d.ResultDamage += state.Difficulty.MonsterDamage;
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnMonsterHit, GameInitializer.NAMESPACE + ".Managers.MonsterManager.OnMonsterHit")]
        public static void OnMonsterHit(IMonster monster, ModLoader.OnHitData d)
        {
            var cs         = ColonyState.GetColonyState(monster.OriginalGoal);
            var pandaArmor = monster as IPandaArmor;
            var pamdaDamage     = d.HitSourceObject as IPandaDamage;
            var skilled = 0f;

            if (pandaArmor != null && Random.NextFloat() <= pandaArmor.MissChance)
            {
                d.ResultDamage = 0;
                return;
            }

            if (pamdaDamage == null && d.HitSourceType == ModLoader.OnHitData.EHitSourceType.NPC)
            {
                var npc = d.HitSourceObject as NPCBase;
                var inv = ColonistInventory.Get(npc);
                ColonistManager.IncrimentSkill(npc);
                skilled = inv.GetSkillModifier();

                if (inv.Weapon != null && Items.Weapons.WeaponFactory.WeaponLookup.TryGetValue(inv.Weapon.Id, out var wep))
                    pamdaDamage = wep;
            }

            if (pandaArmor != null && pamdaDamage != null)
            {
                d.ResultDamage = Items.Weapons.WeaponFactory.CalcDamage(pandaArmor, pamdaDamage);
            }
            else if (pandaArmor != null)
            {
                d.ResultDamage = DamageType.Physical.CalcDamage(pandaArmor.ElementalArmor, d.ResultDamage);

                if (pandaArmor.AdditionalResistance.TryGetValue(DamageType.Physical, out var flatResist))
                    d.ResultDamage = d.ResultDamage - d.ResultDamage * flatResist;
            }

            double skillRoll = Pipliz.Random.Next() + skilled;

            if (skillRoll < skilled)
                d.ResultDamage += d.ResultDamage;

            d.ResultDamage = d.ResultDamage - d.ResultDamage * cs.Difficulty.MonsterDamageReduction;

            if (d.ResultDamage < 0)
                d.ResultDamage = 0;

            if (d.HitSourceType == ModLoader.OnHitData.EHitSourceType.NPC)
            {
                var npc = d.HitSourceObject as NPCBase;
                var inv = ColonistInventory.Get(npc);
                inv.IncrimentStat("Damage Done", d.ResultDamage);

                if (skillRoll < skilled)
                    inv.IncrimentStat("Double Damage Hits");
            }

            if (d.ResultDamage >= monster.CurrentHealth)
            {
                monster.OnRagdoll();
                var rewardMonster = monster as IPandaZombie;
                string monsterType = "zombie";

                if (rewardMonster != null)
                    monsterType = rewardMonster.MosterType;

                if (monster.OriginalGoal.OwnerIsOnline())
                {
                    if (!string.IsNullOrEmpty(monsterType) &&
                        LootTables.Lookup.TryGetValue(monsterType, out var lootTable))
                    {
                        float luck = 0;

                        if (d.HitSourceObject is ILucky luckSrc)
                            luck = luckSrc.Luck;
                        else if ((d.HitSourceType == ModLoader.OnHitData.EHitSourceType.PlayerClick ||
                                d.HitSourceType == ModLoader.OnHitData.EHitSourceType.PlayerProjectile) &&
                                d.HitSourceObject is Players.Player player)
                        {
                            var ps = PlayerState.GetPlayerState(player);

                            foreach (var armor in ps.Armor)
                                if (Items.Armor.ArmorFactory.ArmorLookup.TryGetValue(armor.Value.Id, out var a))
                                    luck += a.Luck;

                            if (Items.Weapons.WeaponFactory.WeaponLookup.TryGetValue(ps.Weapon.Id, out var w))
                                luck += w.Luck;
                        }
                        else if (d.HitSourceType == ModLoader.OnHitData.EHitSourceType.NPC &&
                                d.HitSourceObject is NPCBase nPC)
                        {
                            var inv = ColonistInventory.Get(nPC);

                            foreach (var armor in inv.Armor)
                                if (Items.Armor.ArmorFactory.ArmorLookup.TryGetValue(armor.Value.Id, out var a))
                                    luck += a.Luck;

                            if (Items.Weapons.WeaponFactory.WeaponLookup.TryGetValue(inv.Weapon.Id, out var w))
                                luck += w.Luck;
                        }

                        var roll = lootTable.GetDrops(luck);

                        foreach (var item in roll)
                            monster.OriginalGoal.Stockpile.Add(item.Key, item.Value);
                    }
                }
            }
        }

        public static Dictionary<int, IMonster> GetAllMonsters()
        {
            return typeof(MonsterTracker).GetField("allMonsters", BindingFlags.Static | BindingFlags.NonPublic)
                                         .GetValue(null) as Dictionary<int, IMonster>;
        }
    }
}