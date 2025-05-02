using SimuliEngine.Tiles;
using SimuliEngine.World;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

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

        public WorldState Generate()
        {
            var initial = new WorldState(Size.Item1, Size.Item2);
            var seed = Config.GetValueOrDefault<int?>("seed", null);
            var random = seed.HasValue ? new Random(seed.Value) : new Random();
            AddGenerateTerrainSteps(random, initial);
            AddGenerateFluctuationsSteps(random, initial);
            _steps.Add(new MapGenerationStep("GenerateSpawnPositions", GenerateSpawnPositions));
            initial = RunGeneration(_steps, random, initial);
            return initial;
        }

        private WorldState RunGeneration(List<MapGenerationStep> steps, Random rand, WorldState initial)
        {
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
