using AI;
using Difficulty;
using System.Linq;
using NPC;
using Pipliz;
using System.Collections.Generic;
using UnityEngine;
using static BlockEntities.Implementations.BannerTracker;
using Vector3Int = Pipliz.Vector3Int;
using Monsters;
using Pandaros.API.Monsters.DistributionCalculators;
using Pandaros.API.Monsters.MonsterSpawnCalculators;
using Pandaros.API.Entities;

namespace Pandaros.API.Monsters
{
	[ModLoader.ModManager]
	public class PandaMonsterSpawner : IMonsterSpawner, PathingManager.IPathingThreadAction
	{
		public static Dictionary<string, IMonsterDistributionCalculator> DistributionCalculators { get; set; } = new Dictionary<string, IMonsterDistributionCalculator>();
		public static List<IMonsterSpawnCalculator> SpawnCalcuators { get; set; } = new List<IMonsterSpawnCalculator>();
		public static IMonsterDistributionCalculator DefaultDistributionCalculator { get; set; } = new DefaultDistributionCalculator();
		public static IMonsterSpawnCalculator DefaultSpawnCalculator { get; set; } = new GetZombieCCalculator();
		public static List<IPandaZombie> PandaZombies { get; set; } = new List<IPandaZombie>(); // TODO

		public static IMonsterDistributionCalculator GetDistributionCalculator(string name)
		{
			if (!DistributionCalculators.TryGetValue(name, out var calculator))
				calculator = DefaultDistributionCalculator;

			return calculator;
		}

		protected Dictionary<Colony, double> NextZombieSpawnTimes = new Dictionary<Colony, double>();
		protected List<Colony> coloniesRequiringZombies = new List<Colony>();

		protected System.Random pathingRandomSource = new System.Random();

		protected PathCache pathCache = new PathCache();
		protected int pathCacheUpdateCounter = VERIFY_PATHCACHE_EVERY_X_UPDATES;

		const double SIEGEMODE_COOLDOWN_SECONDS = 3.0;
		const double MONSTERS_DELAY_THRESHOLD_SECONDS = 1.0;
		const int MAX_SPAWN_TRIES_PER_BANNER = 40;
		const double PATH_CACHE_REUSE_COOLDOWN_MIN = 1.5;
		const double PATH_CACHE_REUSE_COOLDOWN_MAX = 3.5;

		const int VERIFY_PATHCACHE_EVERY_X_UPDATES = 6000; // 2 minutes IRL

		protected bool isQueuedForPathing;
		protected Pipliz.Collections.Threadsafe.ThreadedSegmentedQueue<QueuedSpawn> QueuedSpawns = new Pipliz.Collections.Threadsafe.ThreadedSegmentedQueue<QueuedSpawn>(256);
		protected Pipliz.Collections.Threadsafe.ThreadedSegmentedQueue<QueuedSpawnResult> QueuedSpawnResults = new Pipliz.Collections.Threadsafe.ThreadedSegmentedQueue<QueuedSpawnResult>(256);

		protected struct QueuedSpawn
		{
			public QueuedSpawnPayload Payload;

			public Vector3Int goalPosition;
			public int goalSafeRadius;
			public float maxWalkDistance;
		}

		protected struct QueuedSpawnResult
		{
			public QueuedSpawnPayload Payload;

			public Path path;
			public EResult result;

			public enum EResult : byte
			{
				None,
				SuccessPath,
				FailPath,
				GoodFailSpawn,
				BadFailSpawn,
			}
		}

		protected struct QueuedSpawnPayload
		{
			public Banner banner;
			public int recycleFrequency;
			public NPCType typeToSpawn;
		}

		public virtual void Update()
		{
			if (World.FramesSinceInitialization < 2 || isQueuedForPathing || !ChunkQueue.CompletedInitialLoad)
			{
				return;
			}

			if (--pathCacheUpdateCounter <= 0)
			{
				pathCacheUpdateCounter = VERIFY_PATHCACHE_EVERY_X_UPDATES;
				pathCache.VerifyGoals();
			}

			QueuedSpawnResult result;
			while (QueuedSpawnResults.TryDequeue(out result))
			{
				Colony colony = result.Payload.banner?.Colony;
				if (colony == null)
				{
					continue;
				}

				switch (result.result)
				{
					case QueuedSpawnResult.EResult.BadFailSpawn:
					case QueuedSpawnResult.EResult.FailPath:
						NextZombieSpawnTimes[colony] = NextZombieSpawnTimes[colony] - colony.DifficultySetting.GetZombieSpawnCooldown(colony);
						colony.OnZombieSpawn(false);
						break;
					case QueuedSpawnResult.EResult.GoodFailSpawn:
						colony.OnZombieSpawn(true);
						break;
					case QueuedSpawnResult.EResult.SuccessPath:
						if (result.Payload.recycleFrequency > 1)
						{
							pathCache.AddCopy(result.path);
						}
						SpawnZombie(result.Payload.typeToSpawn, result.path, colony, true);
						break;
				}
			}


			Data data;
			data.ColoniesRequiringZombies = coloniesRequiringZombies;
			data.NextZombieSpawnTimes = NextZombieSpawnTimes;
			ServerManager.BlockEntityTracker.BannerTracker.Foreach(ForeachBanner, ref data);

			for (int i = 0; i < coloniesRequiringZombies.Count; i++)
			{
				Colony colony = coloniesRequiringZombies[i];
				IColonyDifficultySetting difficulty = colony.DifficultySetting;
				float cooldown = difficulty.GetZombieSpawnCooldown(colony);

				double nextZombieSpawn = Extensions.GetValueOrDefault(NextZombieSpawnTimes, colony, ServerTime.SecondsSinceStart);

				while (true)
				{
					QueueSpawnZombie(colony, difficulty, cooldown);
					nextZombieSpawn += cooldown;
					if (nextZombieSpawn > ServerTime.SecondsSinceStart || colony.InSiegeMode)
					{
						break;
					}
				}

				NextZombieSpawnTimes[colony] = nextZombieSpawn;
			}

			coloniesRequiringZombies.Clear();

			if (QueuedSpawns.Count > 0)
			{
				isQueuedForPathing = true;
				ServerManager.PathingManager.QueueAction(this);
			}
		}

		public void RegisterMonsterSpawnedElsewhere(IMonster monster, bool notifyColonyForSiegeMode = true)
		{
			ThreadManager.AssertIsMainThread();
			ModLoader.Callbacks.OnMonsterSpawned.Invoke(monster);
			MonsterTracker.Add(monster);
			Colony goalColony = monster.OriginalGoal;
			if (goalColony != null)
			{
				IColonyDifficultySetting difficulty = goalColony.DifficultySetting;
				float cooldown = difficulty.GetZombieSpawnCooldown(goalColony);
				NextZombieSpawnTimes[goalColony] = Extensions.GetValueOrDefault(NextZombieSpawnTimes, goalColony, ServerTime.SecondsSinceStart) + cooldown;

				if (notifyColonyForSiegeMode)
				{
					goalColony.OnZombieSpawn(true);
				}
			}
		}

		public virtual void PathingThreadAction(PathingManager.PathingContext context)
		{
			int maxRadius = ServerManager.ServerSettings.Banner.MaximumZombieSpawnRadius;

			QueuedSpawn spawn;
			while (QueuedSpawns.TryDequeue(out spawn))
			{
				Vector3Int position;

				QueuedSpawnResult result;
				result.Payload = spawn.Payload;
				result.path = null;

				var getSpawnResult = TryGetSpawnLocation(context, spawn.goalPosition, spawn.goalSafeRadius, maxRadius, spawn.maxWalkDistance, out position);
				switch (getSpawnResult)
				{
					case ESpawnResult.Success:
						{
							Path path;
							if (context.Pathing.TryFindPath(ref context, position, spawn.goalPosition, out path, 2 * 1000 * 1000) == PathFinder.EPathFindingResult.Success)
							{
								result.path = path;
								result.result = QueuedSpawnResult.EResult.SuccessPath;
							}
							else
							{
								result.result = QueuedSpawnResult.EResult.FailPath;
							}
						}
						break;
					case ESpawnResult.NotLoaded:
					case ESpawnResult.Impossible:
						result.result = QueuedSpawnResult.EResult.GoodFailSpawn;
						break;
					default:
					case ESpawnResult.Fail:
						result.result = QueuedSpawnResult.EResult.BadFailSpawn;
						break;
				}

				QueuedSpawnResults.Enqueue(result);
			}

			isQueuedForPathing = false;
		}

		struct Data
		{
			public Dictionary<Colony, double> NextZombieSpawnTimes;
			public List<Colony> ColoniesRequiringZombies;
		}

		static void ForeachBanner(Vector3Int position, Banner banner, ref Data data)
		{
			Colony colony = banner.Colony;
			
			if (colony == null)
			{
				return;
			}

			if (colony.FollowerCount == 0)
			{
				colony.OnZombieSpawn(true);
				return;
			}

			IColonyDifficultySetting difficultyColony = colony.DifficultySetting;
			ColonyState colonyState = ColonyState.GetColonyState(colony);

			if (!difficultyColony.ShouldSpawnZombies(colony) || !colonyState.MonstersEnabled)
			{
				colony.OnZombieSpawn(true);
				return;
			}

			double nextZombieSpawnTime = Extensions.GetValueOrDefault(data.NextZombieSpawnTimes, colony, 0.0);
			if (nextZombieSpawnTime > ServerTime.SecondsSinceStart)
			{
				return;
			}

			if (colony.InSiegeMode)
			{
				if (ServerTime.SecondsSinceStart - colony.LastSiegeModeSpawn < SIEGEMODE_COOLDOWN_SECONDS)
				{
					return;
				}
				else
				{
					colony.LastSiegeModeSpawn = ServerTime.SecondsSinceStart;
				}
			}

			if (ServerTime.SecondsSinceStart - nextZombieSpawnTime > MONSTERS_DELAY_THRESHOLD_SECONDS)
			{
				// lagging behind, or no cooldown set: teleport closer to current time
				data.NextZombieSpawnTimes[colony] = ServerTime.SecondsSinceStart - MONSTERS_DELAY_THRESHOLD_SECONDS;
			}

			data.ColoniesRequiringZombies.AddIfUnique(colony);
		}

		public void QueueSpawnZombie(Colony colony, IColonyDifficultySetting difficulty)
		{
			QueueSpawnZombie(colony, difficulty, difficulty.GetZombieSpawnCooldown(colony));
		}

		public void QueueSpawnZombie(Colony colony, IColonyDifficultySetting difficulty, float cooldown)
		{
			int recycleFrequency = GetPathRecycleFrequency(1f / cooldown);
			NPCType typeToSpawn = GetTypeToSpawn(colony);
			Banner banner = colony.GetRandomBanner();
			if (banner == null)
			{
				return;
			}
			float maxPathDistance = difficulty.GetMaxPathDistance(colony, typeToSpawn, banner.SafeRadius);
			QueueSpawnZombie(banner, typeToSpawn, recycleFrequency, maxPathDistance);
		}

		public void QueueSpawnZombie(
			Banner banner,
			NPCType typeToSpawn,
			int RecycleFrequency = 1,
			float maxSpawnWalkDistance = DifficultyManager.MONSTERS_MAX_SPAWN_DISTANCE_START
		)
		{
			Path path;
			if (RecycleFrequency > 1 && pathCache.TryGetPath(RecycleFrequency, banner.Position, maxSpawnWalkDistance, out path))
			{
				SpawnZombie(typeToSpawn, path, banner.Colony, false);
				return;
			}

			QueuedSpawn spawn;
			spawn.Payload.banner = banner;
			spawn.Payload.recycleFrequency = RecycleFrequency;
			spawn.Payload.typeToSpawn = typeToSpawn;

			spawn.goalPosition = banner.Position;
			spawn.goalSafeRadius = banner.SafeRadius;
			spawn.maxWalkDistance = maxSpawnWalkDistance;
			QueuedSpawns.Enqueue(spawn);
		}

		public void SpawnZombie(NPCType typeToSpawn, Path path, Colony colony, bool notifyColonyForSiegeMode = true)
		{
			IMonster monster = new Zombie(typeToSpawn, path, colony);
			ModLoader.Callbacks.OnMonsterSpawned.Invoke(monster);
			MonsterTracker.Add(monster);
			if (notifyColonyForSiegeMode)
			{
				colony.OnZombieSpawn(true);
			}
		}

		public ESpawnResult TryGetSpawnLocation(PathingManager.PathingContext context, Vector3Int bannerPosition, int minSpawnRadius, int maxSpawnRadius, float maxSpawnWalkDistance, out Vector3Int positionFinal)
		{
			int maxWalkDistance = Math.RoundToInt(maxSpawnWalkDistance);
			maxSpawnRadius = Math.Min(maxWalkDistance, maxSpawnRadius);

			if (minSpawnRadius >= maxSpawnRadius)
			{
				positionFinal = Vector3Int.invalidPos;
				return ESpawnResult.Impossible;
			}

			int foundPathPosButSafes = 0;

			lock (pathingRandomSource)
			{
				for (int spawnTry = 0; spawnTry < MAX_SPAWN_TRIES_PER_BANNER; spawnTry++)
				{
					if (TrySamplePosition(true, out Vector3Int possiblePosition))
					{
						if (TestPositionFinal(possiblePosition, out positionFinal))
						{
							return ESpawnResult.Success;
						}
					}
				}

				if (foundPathPosButSafes > 3)
				{
					// so we did find multiple position we can spawn at, but they were blocked by other banners than the one we're spawning around
					// in that case retry but without limiting the walking distance in a 'diamond' pattern, just take the axis aligned max spawn radius
					for (int spawnTry = 0; spawnTry < MAX_SPAWN_TRIES_PER_BANNER; spawnTry++)
					{
						if (TrySamplePosition(false, out Vector3Int possiblePosition))
						{
							if (TestPositionFinal(possiblePosition, out positionFinal))
							{
								return ESpawnResult.Success;
							}
						}
					}
				}
			}

			positionFinal = Vector3Int.invalidPos;
			return ESpawnResult.Fail;

			bool TestPositionFinal(Vector3Int possiblePosition, out Vector3Int positionFinalTest)
			{
				if (context.NavWorld.TryGetClosestAIPosition(
					possiblePosition,
					NavWorld.EAIClosestPositionSearchType.ChunkAndDirectNeighbours,
					out positionFinalTest
				))
				{
					if (ServerManager.BlockEntityTracker.BannerTracker.IsSafeZone(positionFinalTest, out Vector3Int foundBanner))
					{
						if (foundBanner != bannerPosition)
						{
							foundPathPosButSafes++;
						}
					}
					else
					{
						return true;
					}
				}
				return false;
			}

			bool TrySamplePosition(bool checkWalkingDistance, out Vector3Int position)
			{
				for (int i = 0; i < 1000; i++)
				{
					Vector3Int offset = new Vector3Int
					{
						x = pathingRandomSource.Next(-maxSpawnRadius, maxSpawnRadius),
						y = pathingRandomSource.Next(-maxSpawnRadius, maxSpawnRadius),
						z = pathingRandomSource.Next(-maxSpawnRadius, maxSpawnRadius)
					};

					if (offset.MaxPartAbs <= minSpawnRadius)
					{
						continue; // fais safe zone check
					}

					if (checkWalkingDistance && Math.Abs(offset.x) + Math.Abs(offset.z) > maxWalkDistance)
					{
						continue; // fails walk check
					}

					position = bannerPosition + offset;
					return true;
				}

				position = default;
				return false;
			}
		}

		[ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad, GameInitializer.NAMESPACE + ".Monsters.register")]
		[ModLoader.ModCallbackDependsOn("pipliz.server.monsterspawner.register")]
		static void Register()
		{
			MonsterTracker.MonsterSpawner = new PandaMonsterSpawner();
		}

		[ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad, GameInitializer.NAMESPACE + ".Monsters.fetchnpctypes")]
		[ModLoader.ModCallbackDependsOn("pipliz.server.monsterspawner.fetchnpctypes")]
		static void Fetch()
		{
			GetZombieACalculator.MonsterAA = NPCType.GetByKeyNameOrDefault("pipliz.monsteraa");
			GetZombieACalculator.MonsterAB = NPCType.GetByKeyNameOrDefault("pipliz.monsterab");
			GetZombieACalculator.MonsterAC = NPCType.GetByKeyNameOrDefault("pipliz.monsterac");

			GetZombieBCalculator.MonsterBA = NPCType.GetByKeyNameOrDefault("pipliz.monsterba");
			GetZombieBCalculator.MonsterBB = NPCType.GetByKeyNameOrDefault("pipliz.monsterbb");
			GetZombieBCalculator.MonsterBC = NPCType.GetByKeyNameOrDefault("pipliz.monsterbc");

			GetZombieCCalculator.MonsterCA = NPCType.GetByKeyNameOrDefault("pipliz.monsterca");
			GetZombieCCalculator.MonsterCB = NPCType.GetByKeyNameOrDefault("pipliz.monstercb");
			GetZombieCCalculator.MonsterCC = NPCType.GetByKeyNameOrDefault("pipliz.monstercc");

			DistributionCalculators[nameof(ZombieADistribution)] = new ZombieADistribution();
			DistributionCalculators[nameof(ZombieBDistribution)] = new ZombieBDistribution();
			DistributionCalculators[nameof(ZombieCDistribution)] = new ZombieCDistribution();

			SpawnCalcuators.Add(new GetZombieACalculator());
			SpawnCalcuators.Add(new GetZombieBCalculator());
			SpawnCalcuators.Add(new GetZombieCCalculator());
		}

		public static int GetPathRecycleFrequency(float monstersPerSecond)
		{
			return Mathf.CeilToInt(Mathf.Clamp(monstersPerSecond * 4f, 1f, 10f));
		}

		public NPCType GetTypeToSpawn(Colony c)
		{
			Vector2 distribution = DefaultDistributionCalculator.GetMonsterDistribution(c);
			float rand = Pipliz.Random.NextFloat(0f, 1f);

			foreach (var item in SpawnCalcuators)
				if (item.ShouldSpawn(c, distribution, rand))
				{
					return item.GetMonster(c);
				}

			return DefaultSpawnCalculator.GetMonster(c);
		}

		protected class PathCache
		{
			Dictionary<Vector3Int, PathCacheGoal> PathCaches = new Dictionary<Vector3Int, PathCacheGoal>();

			public bool TryGetPath(int maxRecycled, Vector3Int goal, float maxSpawnWalkDistance, out Path path)
			{
				if (PathCaches.TryGetValue(goal, out PathCacheGoal cache))
				{
					return cache.TryGetPath(maxRecycled, maxSpawnWalkDistance, out path);
				}
				path = null;
				return false;
			}

			public void AddCopy(Path path)
			{
				if (!PathCaches.TryGetValue(path.Goal, out PathCacheGoal cache))
				{
					PathCaches[path.Goal] = cache = new PathCacheGoal() { storage = new Pipliz.Collections.BinaryHeap<double, (Path, int)>(10) };
				}
				cache.Add(path.DeepCopy());
			}

			public void VerifyGoals()
			{
				foreach (Vector3Int noBanner in PathCaches.Keys.Where(key => !ServerManager.BlockEntityTracker.BannerTracker.IsBanner(key)).ToList())
				{
					PathCaches.Remove(noBanner);
				}
			}

			public struct PathCacheItem
			{
				public int TimesUsed;
				public Path Path;
			}

			struct PathCacheGoal
			{
				public Pipliz.Collections.BinaryHeap<double, (Path, int)> storage;

				public bool TryGetPath(int maxRecycled, float maxSpawnWalkDistance, out Path path)
				{
					while (true)
					{
						if (storage.Count == 0)
						{
							path = default;
							return false;
						}

						double minKey = storage.PeekKeyAtIndex(0);
						double timeSinceUsable = ServerTime.SecondsSinceStart - minKey;

						if (timeSinceUsable < minKey)
						{
							path = default;
							return false;
						}

						(Path cachedPath, int useCount) = storage.ExtractValueMin();

						if (timeSinceUsable > PATH_CACHE_REUSE_COOLDOWN_MAX * 10)
						{
							Path.Return(cachedPath);
							continue;
						}

						Vector3Int pathStart = cachedPath.Start;
						Vector3Int pathGoal = cachedPath.Goal;

						if (Math.Abs(pathStart.x - pathGoal.x) + Math.Abs(pathStart.z - pathGoal.z) > maxSpawnWalkDistance)
						{
							Path.Return(cachedPath);
							continue;
						}

						if (useCount >= maxRecycled)
						{
							path = cachedPath;
						}
						else
						{
							path = cachedPath.DeepCopy();
							storage.Add(ServerTime.SecondsSinceStart + GetCooldown(), (cachedPath, useCount + 1));
						}
						return true;
					}
				}

				public void Add(Path path)
				{
					storage.Add(ServerTime.SecondsSinceStart + GetCooldown(), (path, 1));
				}

				static double GetCooldown()
				{
					return Pipliz.Random.NextDouble(PATH_CACHE_REUSE_COOLDOWN_MIN, PATH_CACHE_REUSE_COOLDOWN_MAX);
				}
			}
		}

		public enum ESpawnResult
		{
			Success,
			NotLoaded,
			Fail,
			Impossible
		}
	}
}
