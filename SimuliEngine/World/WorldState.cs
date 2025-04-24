using SimuliEngine.Basic;
using SimuliEngine.Simulation.ActorSystem;
using SimuliEngine.Simulation.Obstacles;
using SimuliEngine.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimuliEngine.World
{
    public class WorldState
    {
        public (int, int) Size;
        public readonly int HypercellChunkSize = 16; // Size of the hypercell chunk in each dimension
        public readonly int SubdivisionSize = 4; // Size of the hypercell chunk in each dimension

        public DoubleBufferedHyperMap<float> Temperature;
        public HyperMap<TileType> TileTypeMap; // Map of tile types
        public HyperMap<HashSet<CellActorReference>> ActorMap; // Map of actors in the world
        public ICollection<Actor> Actors = new List<Actor>(); // List of instantiated actors in the world
        public List<(int x, int y)> InitialSpawnPositions = new List<(int x, int y)>();
        public ObstacleTracker ObstacleTracker { get; private set; }

        public int SizeX => Size.Item1;
        public int SizeY => Size.Item2;

        public WorldState(int sizeX, int sizeY)
        {
            Size = (sizeX, sizeY);
            Temperature = new DoubleBufferedHyperMap<float>(HypercellChunkSize);
            Temperature.PreInit(Size);
            TileTypeMap = new HyperMap<TileType>(HypercellChunkSize);
            TileTypeMap.PreInit(Size);
            ActorMap = new HyperMap<HashSet<CellActorReference>>(HypercellChunkSize);
            ActorMap.PreInit(Size);
            ActorMap.Fill(() => []); // Initialize each cell with an empty set of actors

            ObstacleTracker = new ObstacleTracker(Size, SubdivisionSize, HypercellChunkSize);
        }

        public void Reset(int sizeX, int sizeY)
        {
            Size = (sizeX, sizeY);
            Temperature = new DoubleBufferedHyperMap<float>(HypercellChunkSize);
            Temperature.PreInit(Size);
            TileTypeMap = new HyperMap<TileType>(HypercellChunkSize);
            TileTypeMap.PreInit(Size);
            ActorMap.PreInit(Size);
            ActorMap.Fill(() => []);
            ObstacleTracker = new ObstacleTracker(Size, SubdivisionSize, HypercellChunkSize);
        }

        public CellDigest GetCellState(int x, int y)
        {
            return new CellDigest
            {
                Temperature = Temperature[x, y],
                TileType = TileTypeMap[x, y]

            };
        }

        /// <summary>
        /// Instantiates an actor at the specified coordinates in the world.
        /// Updates ActorMap to include the new actor.
        /// Updates Acror list.
        /// Updates actor's main position.
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void InstantiateActor(Actor actor, int x, int y)
        {
            this.Actors.Add(actor);
            this.ActorMap[x, y].Add(new CellActorReference(actor, (0f, 0f)));
            actor.MainPosition = (x, y);
        }

        /// <summary>
        /// Removes an actor from the world.
        /// Updates Acror list.
        /// Updates ActorMap.
        /// </summary>
        /// <param name="actor"></param>
        internal void RemoveActor(Actor actor)
        {
            this.Actors.Remove(actor);

            // Find the CellActorReference corresponding to the actor
            var cellActorReference = ActorMap[actor.MainPosition.x, actor.MainPosition.y]
                .FirstOrDefault(reference => reference.Actor == actor);

            // Remove the CellActorReference if it exists
            if (cellActorReference != null)
            {
                ActorMap[actor.MainPosition.x, actor.MainPosition.y].Remove(cellActorReference);
            }
        }
    }
}
