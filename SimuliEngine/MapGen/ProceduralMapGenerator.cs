using SimuliEngine.Tiles;
using SimuliEngine.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimuliEngine.MapGen
{
    public class ProceduralMapGenerator
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

        public WorldState Generate()
        {
            var initial = new WorldState(Size.Item1, Size.Item2);
            var seed = Config.GetValueOrDefault<int?>("seed", null);
            var random = seed.HasValue ? new Random(seed.Value) : new Random();
            GenerateTerrain(random, initial);
            GenerateFluctuations(random, initial);
            GenerateSpawnPositions(random, initial);
            return initial;
        }

        private void GenerateFluctuations(Random random, WorldState initial)
        {
            var points = Config.GetValueOrDefault<int>("fluctuations.temp.points", 30);
            var baseTemp = Config.GetValueOrDefault<float>("fluctuations.temp.base", 21f);
            var spikesTemp = Config.GetValueOrDefault<float>("fluctuations.temp.spikes", -2f);

            // fill arrays with base temperature
            initial.Temperature.FillIgnoringBuffer(baseTemp);
            initial.Temperature.BeginProcessing();
            for (int i = 0; i < points; i++)
            {
                var (x, y) = random.RandomPoint(Size.Item1, Size.Item2);
                initial.Temperature[x, y] = spikesTemp;
            }
            initial.Temperature.FinalizeProcessing();
        }

        private void GenerateTerrain(Random random, WorldState initial)
        {
            initial.TileTypeMap.Fill(new TileType.Space());
            var wallChance = Config.GetValueOrDefault<float>("terrain.wall.chance", 0.1f);
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
        }

        private void GenerateSpawnPositions(Random random, WorldState initial)
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
        }

        public ProceduralMapGenerator WithConfig(ProceduralMapGeneratorConfig config)
        {
            Config = config;
            return this;
        }
    }
}
