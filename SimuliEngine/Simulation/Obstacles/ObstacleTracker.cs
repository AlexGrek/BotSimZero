using SimuliEngine.Basic;
using SimuliEngine.Simulation.ActorSystem;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SimuliEngine.Simulation.Obstacles
{
    public class ObstacleTracker
    {
        private (int, int) size;

        private float _cellSize;
        public int SubdivisionCount { get; private set; }
        public HyperMap<SubdivisionCell<IObstacle>> Map { get; private set; }

        private Dictionary<Actor, HashSet<((int cellX, int cellY), (int subX, int subY))>> _actorTouchedSubcells
            = new();

        public ObstacleTracker((int, int) size, int subdivisions, int hypercellChunkSize, float cellSize)
        {
            this.size = size;
            this.SubdivisionCount = subdivisions;

            Map = new HyperMap<SubdivisionCell<IObstacle>>(hypercellChunkSize);
            Map.PreInit(size);
            Map.Fill(() => new SubdivisionCell<IObstacle>(subdivisions));
            _cellSize = cellSize;
        }

        public ObstacleTracker((int, int) size, int subdivisions, int hypercellChunkSize)
            : this(size, subdivisions, hypercellChunkSize, 1f)
        {
        }

        public bool CheckMoveHitObstacle(Actor obj, Vector2 newPosition)
        {
            var pos = newPosition;
            HashSet<(int x, int y)> touchedCells = GetTouchedCells(obj, pos, out var bounds);
            foreach (var cellPos in touchedCells)
            {
                SubdivisionCell<IObstacle> cell = Map[cellPos.x, cellPos.y];
                if (cell.IsSubdivided)
                {
                    // Check if the object is touching any subcells in this cell
                    var touchedSubcells = GetTouchedSubcells(obj, cellPos, bounds);
                    foreach (var subcell in touchedSubcells)
                    {
                        var obstacle = cell[subcell.x, subcell.y];
                        if (obstacle != null && obstacle != obj)
                        {
                            return true; // Hit an obstacle
                        }
                    }
                }
            }
            return false; // No hit detected
        }

        public void ConfirmMove(Actor obj)
        {
            // Actor is moved already, but center position is not updated yet
            // WARNING: Actor state is inconsistent!

            // Update the actor's main position
            var pos = obj.GetNormalizedPosition();

            var centerCell = GetCenterCell(pos);
            obj.MainPosition = centerCell;
            ClearActorSubcells(obj);
            UpdateActorSubcells(obj, pos);
        }

        // Calculate and return the center cell for an actor at a given position
        public (int x, int y) GetCenterCell(Vector2 position)
        {
            return ((int)Math.Floor(position.X), (int)Math.Floor(position.Y));
        }

        // Remove actor from all subcells it currently occupies using cached subcell information
        public void ClearActorSubcells(Actor actor)
        {
            if (_actorTouchedSubcells.TryGetValue(actor, out var touchedSubcells))
            {
                foreach (var subcell in touchedSubcells)
                {
                    var (cellPos, subPos) = subcell;
                    SubdivisionCell<IObstacle> cell = Map[cellPos.cellX, cellPos.cellY];
                    if (cell.IsSubdivided)
                    {
                        if (cell[subPos.subX, subPos.subY] == actor)
                        {
                            cell[subPos.subX, subPos.subY] = null;
                        }
                    }
                }

                // Clear the cache but keep the HashSet for reuse
                touchedSubcells.Clear();
            }
            else
            {
                // Create new set if none exists
                _actorTouchedSubcells[actor] = new HashSet<((int cellX, int cellY), (int subX, int subY))>();
            }
        }




        // Mark subcells as taken by the object
        public void UpdateActorSubcells(Actor obj, Vector2 pos)
        {
            // Get or create the touched subcells cache
            if (!_actorTouchedSubcells.TryGetValue(obj, out var touchedSubcellsCache))
            {
                touchedSubcellsCache = new HashSet<((int cellX, int cellY), (int subX, int subY))>();
                _actorTouchedSubcells[obj] = touchedSubcellsCache;
            }

            // Calculate all cells this object touches
            HashSet<(int x, int y)> touchedCells = GetTouchedCells(obj, pos, out var bounds);

            foreach (var cellPos in touchedCells)
            {
                SubdivisionCell<IObstacle> cell = Map[cellPos.x, cellPos.y];

                // Calculate which subcells are touched in this cell
                var touchedSubcells = GetTouchedSubcells(obj, cellPos, bounds);

                // Mark subcells as taken
                foreach (var subcell in touchedSubcells)
                {
                    cell[subcell.x, subcell.y] = obj;
                    touchedSubcellsCache.Add(((cellPos.x, cellPos.y), (subcell.x, subcell.y)));
                }
            }
        }

        private HashSet<(int x, int y)> GetTouchedCells(Actor obj, Vector2 normalizedPosition, out (float minX, float minY, float maxX, float maxY) bounds)
        {
            HashSet<(int x, int y)> touchedCells = new HashSet<(int x, int y)>();

            // Calculate object bounds in world coordinates
            float halfSize = obj.HalfSize;
            var position = normalizedPosition;
            float minX = position.X - halfSize;
            float maxX = position.X + halfSize;
            float minY = position.Y - halfSize;
            float maxY = position.Y + halfSize;

            bounds = (minX, minY, maxX, maxY);

            // Convert to cell coordinates and add all cells
            int startCellX = (int)Math.Floor(minX);
            int endCellX = (int)Math.Floor(maxX);
            int startCellY = (int)Math.Floor(minY);
            int endCellY = (int)Math.Floor(maxY);

            for (int x = startCellX; x <= endCellX; x++)
            {
                for (int y = startCellY; y <= endCellY; y++)
                {
                    touchedCells.Add((x, y));
                }
            }

            return touchedCells;
        }

        // Get all subcells touched by the object within a specific cell
        private List<(int x, int y)> GetTouchedSubcells(Actor obj, (int x, int y) cellPos, (float minX, float minY, float maxX, float maxY) bounds)
        {
            List<(int x, int y)> touchedSubcells = new List<(int x, int y)>();

            // Convert to subcell coordinates within this cell
            float subCellSize = _cellSize / SubdivisionCount;

            float cellMinX = cellPos.x;
            float cellMinY = cellPos.y;

            // Calculate subcell range that the object touches
            int startSubX = Math.Max(0, (int)Math.Floor((bounds.minX - cellMinX) / subCellSize));
            int endSubX = Math.Min(SubdivisionCount - 1, (int)Math.Floor((bounds.maxX - cellMinX) / subCellSize));
            int startSubY = Math.Max(0, (int)Math.Floor((bounds.minY - cellMinY) / subCellSize));
            int endSubY = Math.Min(SubdivisionCount - 1, (int)Math.Floor((bounds.maxY - cellMinY) / subCellSize));

            for (int x = startSubX; x <= endSubX; x++)
            {
                for (int y = startSubY; y <= endSubY; y++)
                {
                    touchedSubcells.Add((y, x)); // Note: row = y, column = x
                }
            }

            return touchedSubcells;
        }
    }
}
