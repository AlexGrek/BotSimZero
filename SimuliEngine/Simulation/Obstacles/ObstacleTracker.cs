using SimuliEngine.Basic;
using SimuliEngine.Simulation.ActorSystem;
using SimuliEngine.World;
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
    public class ObstacleTracker: IDumpable
    {
        private (int, int) size;

        private float _cellSize;
        private float _halfCellSize;
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
            _halfCellSize = _cellSize / 2;
        }

        public ObstacleTracker((int, int) size, int subdivisions, int hypercellChunkSize)
            : this(size, subdivisions, hypercellChunkSize, 1f)
        {
        }

        public IObstacle? CheckMoveHitObstacle(Actor obj, Vector2 newPosition)
        {
            if (HitPlayzoneBounds(newPosition))
            {
                return new PlayzoneBoundaryObstacle(); // Hit the playzone bounds
            }
            var pos = newPosition;
            HashSet<(int x, int y)> touchedCells = GetTouchedCells(obj, pos, out var bounds);
            foreach (var cellPos in touchedCells)
            {
                if (HitPlayzoneBounds(cellPos))
                {
                    continue; // Skip if out of bounds
                }
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
                            return obstacle; // Hit an obstacle
                        }
                    }
                }
            }
            return null; // No hit detected
        }

        public bool CheckMoveTouchesImpassableCell(Actor obj, WorldState state, Vector2 newPosition)
        {
            HashSet<(int x, int y)> touchedCells = GetTouchedCells(obj, newPosition, out var bounds);
            return CheckMoveTouchesImpassableCell(obj, state, newPosition, touchedCells);
        }

        public bool CheckMoveTouchesImpassableCell(Actor obj, WorldState state, Vector2 newPosition, HashSet<(int x, int y)> touchedCells)
        {
            foreach (var cell in touchedCells)
            {
                if (!HitPlayzoneBounds(cell))
                {
                    var passable = obj.IsPassable(state, cell);
                    if (!passable)
                    {
                        return true; // Hit an impassable cell
                    }
                } 
                else
                {
                    return true; // Hit the playzone bounds
                }
            }
            return false; // No hit detected
        }

        public IObstacle? CheckMove(Actor obj, WorldState state, Vector2 newPosition)
        {
            HashSet<(int x, int y)> touchedCells = GetTouchedCells(obj, newPosition, out var bounds);
            var obst = CheckMoveHitObstacleOrBounds(obj, newPosition, touchedCells, bounds);
            if (obst != null)
            {
                return obst; // Hit an obstacle
            }
            var wallHit = CheckMoveTouchesImpassableCell(obj, state, newPosition, touchedCells);
            if (wallHit)
            {
                return new WallObstacle((0, 0)); // Hit the wall, I don't care where
            }
            return null; // No hit detected
        }


        public IObstacle? CheckMoveHitObstacleOrBounds(Actor obj, Vector2 newPosition)
        {
            HashSet<(int x, int y)> touchedCells = GetTouchedCells(obj, newPosition, out var bounds);
            return this.CheckMoveHitObstacleOrBounds(obj, newPosition, touchedCells, bounds);
        }

        public IObstacle? CheckMoveHitObstacleOrBounds(Actor obj, Vector2 newPosition, HashSet<(int x, int y)> touchedCells, (float minX, float minY, float maxX, float maxY) bounds)
        {
            if (HitPlayzoneBounds(newPosition))
            {
                return new PlayzoneBoundaryObstacle(); // Hit the playzone bounds
            }
            var pos = newPosition;
            
            foreach (var cellPos in touchedCells)
            {
                if (HitPlayzoneBounds(cellPos))
                {
                    return new PlayzoneBoundaryObstacle(); // Hit the playzone bounds
                }
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
                            return obstacle; // Hit an obstacle
                        }
                    }
                }
            }
            return null; // No hit detected
        }

        private bool HitPlayzoneBounds(Vector2 newPosition)
        {
            // Extract the playzone dimensions
            int playzoneWidth = size.Item1;
            int playzoneHeight = size.Item2;

            // Check if the position is outside the playzone bounds
            if (newPosition.X < 0 || newPosition.X >= playzoneWidth ||
                newPosition.Y < 0 || newPosition.Y >= playzoneHeight)
            {
                return true; // Position is out of bounds
            }

            return false; // Position is within bounds
        }

        private bool HitPlayzoneBounds((int X, int Y) newPosition)
        {
            // Extract the playzone dimensions
            int playzoneWidth = size.Item1;
            int playzoneHeight = size.Item2;

            // Check if the position is outside the playzone bounds
            if (newPosition.X < 0 || newPosition.X >= playzoneWidth ||
                newPosition.Y < 0 || newPosition.Y >= playzoneHeight)
            {
                return true; // Position is out of bounds
            }

            return false; // Position is within bounds
        }


        public void ConfirmMove(Actor obj)
        {
            // Actor is moved already, but center position is not updated yet
            // WARNING: Actor state is inconsistent!

            // Update the actor's main position
            var pos = obj.GetNormalizedPosition();
            var prevCenterCell = obj.MainPosition;
            var centerCell = GetCenterCell(pos);
            if (centerCell != prevCenterCell)
            {
                // If the center cell has changed, update it
                obj.MainPosition = centerCell;
                obj.SetCenterPositionChanged(prevCenterCell);
            }
            ClearActorSubcells(obj);
            UpdateActorSubcells(obj, pos);
        }

        // Calculate and return the center cell for an actor at a given position
        public (int x, int y) GetCenterCell(Vector2 position)
        {
            return ((int)Math.Round(position.X / _cellSize), (int)Math.Round(position.Y / _cellSize));
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
                if (HitPlayzoneBounds(cellPos))
                {
                    continue; // Skip if out of bounds
                }
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
            int startCellX = (int)Math.Round(minX / _cellSize);
            int endCellX = (int)Math.Round(maxX / _cellSize);
            int startCellY = (int)Math.Round(minY / _cellSize);
            int endCellY = (int)Math.Round(maxY / _cellSize);

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

            // Adjust cell position to align with the new center-based coordinate system
            float cellCenterX = cellPos.x * _cellSize;
            float cellCenterY = cellPos.y * _cellSize;

            // Calculate subcell range that the object touches
            int startSubX = Math.Max(0, (int)Math.Round((bounds.minX - (cellCenterX - _halfCellSize)) / subCellSize));
            int endSubX = Math.Min(SubdivisionCount - 1, (int)Math.Round((bounds.maxX - (cellCenterX - _halfCellSize)) / subCellSize));
            int startSubY = Math.Max(0, (int)Math.Round((bounds.minY - (cellCenterY - _halfCellSize)) / subCellSize));
            int endSubY = Math.Min(SubdivisionCount - 1, (int)Math.Round((bounds.maxY - (cellCenterY - _halfCellSize)) / subCellSize));

            for (int x = startSubX; x <= endSubX; x++)
            {
                for (int y = startSubY; y <= endSubY; y++)
                {
                    touchedSubcells.Add((x, y));
                }
            }

            return touchedSubcells;
        }
    }
}
