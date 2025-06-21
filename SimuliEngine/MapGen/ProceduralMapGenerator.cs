using SimuliEngine.Tiles;
using SimuliEngine.World;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SimuliEngine.MapGen
{
    public class ProceduralMapGenerator: ILoggable
    {
        public (int, int) Size { get; private set; }
        public ProceduralMapGeneratorConfig Config { get; private set; } = ProceduralMapGeneratorConfig.DefaultConfig();

        public ProceduralMapGenerator(int sizeX, int sizeY)
        {
            Size = (sizeX, sizeY);
        }

        public ProceduralMapGenerator(int sizeX, int sizeY, ProceduralMapGeneratorConfig config)
        {
            Config = config;
            Size = (sizeX, sizeY);
        }

        protected List<MapGenerationStep> _steps = new List<MapGenerationStep>();

        protected MapGenerationMemory _memo = new MapGenerationMemory();

        public WorldState Generate()
        {
            var initial = new WorldState(Size.Item1, Size.Item2);
            var seed = Config.GetValueOrDefault<int?>("seed", null);
            var random = seed.HasValue ? new Random(seed.Value) : new Random();
            AddSmartSteps(random, initial);
            AddGenerateFluctuationsSteps(random, initial);
            _steps.Add(new MapGenerationStep("GenerateSpawnPositions", GenerateSpawnPositions));
            initial = RunGeneration(_steps, random, initial);
            return initial;
        }

        private WorldState RunGeneration(List<MapGenerationStep> steps, Random rand, WorldState initial)
        {
            _memo = new MapGenerationMemory();
            foreach (var step in steps)
            {
                initial = step.Execute(rand, initial);
            }
            return initial;
        }

        private void AddGenerateFluctuationsSteps(Random random, WorldState initial)
        {
            _steps.Add(new MapGenerationStep("TempFluctuations", GenerateTempFluctuations));
        }

        private WorldState GenerateTempFluctuations(Random random, WorldState initial)
        {
            var points = Config.GetValueOrDie<int>("fluctuations.temp.points");
            var baseTemp = Config.GetValueOrDie<float>("fluctuations.temp.base");
            var spikesTemp = Config.GetValueOrDie<float>("fluctuations.temp.spikes");

            // fill arrays with base temperature
            initial.Temperature.FillIgnoringBuffer(baseTemp);
            initial.Temperature.BeginProcessing();
            for (int i = 0; i < points; i++)
            {
                var (x, y) = random.RandomPoint(Size.Item1, Size.Item2);
                initial.Temperature[x, y] = spikesTemp;
            }
            initial.Temperature.FinalizeProcessing();
            return initial;
        }

        private void AddGenerateTerrainSteps(Random random, WorldState initial)
        {
            initial.TileTypeMap.Fill(new TileType.Space());
            _steps.Add(new MapGenerationStep("BasicTerrain", GenerateBasicTerrain));
            _steps.Add(new MapGenerationStep("GenerateSpecialObjects", GenerateSpecialObjects));
        }

        private void AddSmartSteps(Random random, WorldState initial)
        {
            initial.TileTypeMap.Fill(new TileType.Space());
            var center = (Size.Item1 / 2, Size.Item2 / 2);
            var centerSector = 3;
            var initialCrawelerPosition = (random.Next(-Size.Item1 / centerSector, Size.Item1 / centerSector), random.Next(-Size.Item2 / centerSector, Size.Item2 / centerSector));
            var loggable = (ILoggable)this;

            _steps.Add(new MapGenerationStep("GenerateCrawlerRoot", (rand, world) =>
            {
                world.TileTypeMap[initialCrawelerPosition.Item1 + center.Item1, initialCrawelerPosition.Item2 + center.Item2] = new TileType.Wall();
                return world;
            }));
            _steps.Add(new MapGenerationStep("GenerateLargeSpaces", (rand, world) =>
            {
                var spacesCount = (Size.Item1 * Size.Item2) * Config.GetValueOrDie<double>("spaces.spawn_rate");
                var spacesRetries = Config.GetValueOrDie<double>("spaces.retries");
                var minDIstanceFromCenter = Config.GetValueOrDie<double>("spaces.min_distance_from_center");
                var sizeMin = Config.GetValueOrDie<int>("spaces.size.min");
                var sizeMax = Config.GetValueOrDie<int>("spaces.size.max");
                var multiZoneOffsetMax = Config.GetValueOrDie<int>("spaces.multi_zone_offset_max");
                for (int i = 0; i < spacesCount; i++)
                {
                    loggable.Log($"Generating large space {i + 1}/{spacesCount} with retries {spacesRetries} and size range ({sizeMin}, {sizeMax})");
                    for (int retries = 0; retries <= spacesRetries; retries++)
                    {
                        var (xx, yx) = rand.RandomPoint(Size.Item1, Size.Item2);
                        if (Utils.Distance((xx, yx), center) < minDIstanceFromCenter)
                        {
                            continue; // too close to center
                        }
                        var start = (0, 0);
                        var end = (rand.Next(sizeMin, sizeMax), rand.Next(sizeMin, sizeMax));
                        var zone = PointCluster.CreateFromRectWithWalls(start, end);
                        var type = rand.RandomOf(
                            ("single", Config.GetValueOrDie<double>("spaces.single_rate")),
                            ("double", Config.GetValueOrDie<double>("spaces.double_rate")),
                            ("triple", Config.GetValueOrDie<double>("spaces.triple_rate"))
                            );
                        loggable.Log($"Generating large space with mode: {type}; end: {end}, offset: {(xx / 2 + 1, yx / 2 + 1)}");
                        if (type != "single")
                        {
                            var end2 = (rand.Next(sizeMin, sizeMax), rand.Next(sizeMin, sizeMax));
                            var shuffle = rand.RandomPoint(multiZoneOffsetMax, multiZoneOffsetMax);
                            var zone2 = PointCluster.CreateFromRectWithWalls(start, end2).Offset(shuffle);
                            zone = zone.MergeRemovingInnerWallsWith(zone2);
                        }
                        if (type == "triple")
                        {
                            var end3 = (rand.Next(sizeMin, sizeMax) / 2 + 1, rand.Next(sizeMin, sizeMax) / 2 + 1);
                            var shuffle = rand.RandomPoint(multiZoneOffsetMax, multiZoneOffsetMax);
                            var zone3 = PointCluster.CreateFromRectWithWalls(start, end3).Offset(shuffle);
                            zone = zone.MergeRemovingInnerWallsWith(zone3);
                        }
                        zone = zone.Offset((xx, yx));
                        if (!zone.IsInBounds(Size.Item1, Size.Item2))
                        {
                            loggable.Log($"Out of bounds: {zone.BoundingBoxSize()}; at: {zone.BoundingBox()}");
                            continue; // out of bounds
                        }
                        if (_memo.LargeSpaces.Any(z => z.IntersectsIgnoringWalls(zone)))
                        {
                            loggable.Log("Intersects with existing zone");
                            continue; // intersects with existing zone
                        }
                        PutRegionWithWalls(zone, world);
                        loggable.Log("Success!");
                        _memo.LargeSpaces.Add(zone);
                        break; // successfully placed the zone
                    }
                }
                return world;
            }));
        }

        private WorldState PutRegionWithWalls(PointCluster cluster, WorldState state)
        {
            foreach(var wall in cluster.Walls)
            {
                if (state.TileTypeMap[wall.Item1, wall.Item2] is TileType.Space)
                {
                    state.TileTypeMap[wall.Item1, wall.Item2] = new TileType.Wall();
                }
            }
            return state;
        }

        private WorldState GenerateSpecialObjects(Random random, WorldState initial)
        {
            GenerateChargingStations(random, initial);
            return initial;
        }

        private void GenerateChargingStations(Random random, WorldState initial)
        {
            var enabled = Config.GetValueOrDie<bool>("special.charging.enabled");
            if (!enabled)
            {
                return;
            }
            var count = Config.GetValueOrDie<int>("special.charging.count");
            PutObjectsGenAtFreeSpace(random, initial, () => new TileType.InteractiveObject(new ChargingStation(), false, (0, 0)), count);
        }

        private void PutObjectsGenAtFreeSpaceWithRotation(Random random, WorldState initial, Func<TileType> tileTypeGen, int count)
        {
            for (int i = 0; i < count; i++)
            {
                PutObjectGenAtFreeSpaceWithRotation(random, initial, tileTypeGen);
            }
        }

        private void PutObjectsGenAtFreeSpace(Random random, WorldState initial, Func<TileType> tileTypeGen, int count)
        {
            for (int i = 0; i < count; i++)
            {
                PutObjectGenAtFreeSpace(random, initial, tileTypeGen);
            }
        }

        private void PutObjectInstanceAtFreeSpace(Random random, WorldState initial, TileType tileType)
        {
            var goodPosition = false;
            while (!goodPosition)
            {
                var (xx, yx) = random.RandomPoint(Size.Item1, Size.Item2);
                if (initial.TileTypeMap[xx, yx] is TileType.Space)
                {
                    initial.TileTypeMap[xx, yx] = tileType;
                    goodPosition = true;
                }
            }
        }

        private void PutObjectGenAtFreeSpace(Random random, WorldState initial, Func<TileType> tileTypeGen)
        {
            var goodPosition = false;
            while (!goodPosition)
            {
                var (xx, yx) = random.RandomPoint(Size.Item1, Size.Item2);
                if (initial.TileTypeMap[xx, yx] is TileType.Space)
                {
                    initial.TileTypeMap[xx, yx] = tileTypeGen();
                    goodPosition = true;
                }
            }
        }

        private void PutObjectGenAtFreeSpaceWithRotation(Random random, WorldState initial, Func<TileType> tileTypeGen)
        {
            var goodPosition = false;
            while (!goodPosition)
            {
                var (xx, yx) = random.RandomPoint(Size.Item1, Size.Item2);
                if (initial.TileTypeMap[xx, yx] is TileType.Space)
                {
                    initial.TileTypeMap[xx, yx] = tileTypeGen();
                    goodPosition = true;
                }
            }
        }

        private WorldState GenerateBasicTerrain(Random random, WorldState initial)
        {
            var wallChance = Config.GetValueOrDie<float>("terrain.wall.chance");
            for (int x = 0; x < Size.Item1; x++)
            {
                for (int y = 0; y < Size.Item2; y++)
                {
                    if (random.NextDouble() < wallChance)
                    {
                        initial.TileTypeMap[x, y] = new TileType.Wall();
                    }
                }
            }
            return initial;
        }

        private WorldState GenerateSpawnPositions(Random random, WorldState initial)
        {
            var spawnCount = Config.GetValueOrDefault<int>("spawn.count", 10);
            for (int i = 0; i < spawnCount; i++)
            {
                var goodPosition = false;
                while (!goodPosition)
                {
                    var (xx, yx) = random.RandomPoint(Size.Item1, Size.Item2);
                    if (initial.TileTypeMap[xx, yx] is TileType.Space)
                    {
                        goodPosition = true;
                    }
                }
                var (x, y) = random.RandomPoint(Size.Item1, Size.Item2);
                
                initial.InitialSpawnPositions.Add((x, y));
            }
            return initial;
        }

        public ProceduralMapGenerator WithConfig(ProceduralMapGeneratorConfig config)
        {
            Config = config;
            return this;
        }

        protected class MapGenerationMemory: ILoggable
        {
            public (int, int) CrawlerRoot { get; set; } = (0, 0);
            public List<PointCluster> LargeSpaces { get; set; } = new List<PointCluster>();
        }
        
        protected class MapGenerationStep: ILoggable
        {
            public Func<Random, WorldState, WorldState> Step { get; }

            public readonly string Name;

            public bool Executed { get; private set; } = false;

            public TimeSpan Duration { get; private set; } = TimeSpan.Zero;

            public MapGenerationStep(string name, Func<Random, WorldState, WorldState> step)
            {
                Step = step;
                Name = name;
            }

            public WorldState Execute(Random random, WorldState initial)
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                var done = Step(random, initial);
                stopwatch.Stop();
                Executed = true;
                Duration = stopwatch.Elapsed;
                var loggable = (ILoggable)this;
                loggable.Log($"Step {Name} executed in {Duration.TotalMilliseconds}ms");
                return done;
            }
        }
    }
}
