using SimuliEngine.Basic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SimuliEngine.MapGen
{
    public class PointCluster: IReadOnlySet<(int x, int y)>, IDumpable
    {
        private HashSet<(int x, int y)> _withoutWalls;
        private HashSet<(int x, int y)> _walls;

        #region picks

        public (int x, int y) PickRandom(Random random)
        {
            var withoutWalls = _withoutWalls.ToArray();
            return withoutWalls[random.Next(withoutWalls.Length)];
        }

        public (int x, int y) PickRandomWall(Random random)
        {
            var walls = _walls.ToArray();
            return walls[random.Next(walls.Length)];
        }

        public ((int x, int y) wall, (int x, int y) facing)? PickRandomWallFacingInside(Random random)
        {
            var displacements = RandomizeDisplacements(random);
            var walls = _walls.ToArray().Shuffle(random);
            for (int i = 0; i < walls.Length; i++)
            {
                var wall = walls[i];
                foreach (var displacement in displacements)
                {
                    var facing = (wall.x + displacement.X, wall.y + displacement.Y);
                    if (_withoutWalls.Contains(facing) && !_walls.Contains(facing))
                    {
                        return (wall, facing);
                    }
                }
            }
            return null;
        }

        public ((int x, int y) wall, (int x, int y) facing)? PickRandomWallFacingOutside(Random random)
        {
            var displacements = RandomizeDisplacements(random);
            var walls = _walls.ToArray().Shuffle(random);
            for (int i = 0; i < walls.Length; i++)
            {
                var wall = walls[i];
                foreach (var displacement in displacements)
                {
                    var facing = (wall.x + displacement.X, wall.y + displacement.Y);
                    if (!_withoutWalls.Contains(facing) && !_walls.Contains(facing))
                    {
                        return (wall, facing);
                    }
                }
            }
            return null;
        }

        public ((int x, int y) corner, (int x, int y) wall1, (int x, int y) wall2)? PickRandomCorner(Random random)
        {
            var displacements = RandomizeDisplacements(random);
            var walls = _walls.ToArray().Shuffle(random);
            for (int i = 0; i < walls.Length; i++)
            {
                var wall = walls[i];
                foreach (var displacement in displacements)
                {
                    var cornerCandidate = (wall.x + displacement.X, wall.y + displacement.Y);
                    if (_withoutWalls.Contains(cornerCandidate) && !_walls.Contains(cornerCandidate))
                    {
                        // check candidate, as we already know it is near a wall
                        var nextWalls = displacements.Where(d => d != displacement).ToArray();
                        foreach (var nextWall in nextWalls)
                        {
                            var nextWallCandidate = (cornerCandidate.Item1 + nextWall.X, cornerCandidate.Item2 + nextWall.Y);
                            if (_walls.Contains(nextWallCandidate))
                            {
                                return (cornerCandidate, wall, nextWallCandidate);
                            }
                        }
                    }
                }
            }
            return null;
        }

        public IEnumerable<(int x, int y)> AllPointsNearWalls()
        {
            foreach (var wall in _walls)
            {
                var displacements = Displacements();
                foreach (var displacement in displacements)
                {
                    var candidate = (wall.x + displacement.X, wall.y + displacement.Y);
                    if (_withoutWalls.Contains(candidate) && !_walls.Contains(candidate))
                    {
                        yield return candidate;
                    }
                }
            }
        }

        public IEnumerable<(int x, int y)> AllPointsNotNearWalls()
        {
            var near = AllPointsNearWalls().ToImmutableHashSet();
            return _withoutWalls.Except(near);
        }

        public static Point[] RandomizeDisplacements(Random random)
        {
            var displacements = new[]
                        {
                new Point(1, 0), new Point(-1, 0), new Point(0, 1), new Point(0, -1)
            }.Shuffle(random);
            return displacements;
        }

        public static Point[] Displacements()
        {
            var displacements = new[]
                        {
                new Point(1, 0), new Point(-1, 0), new Point(0, 1), new Point(0, -1)
            };
            return displacements;
        }

        #endregion

        #region constructors
        
        public static PointCluster CreateEmpty()
        {
            return new PointCluster(Enumerable.Empty<(int x, int y)>(), Enumerable.Empty<(int x, int y)>());
        }

        public static PointCluster CreateFromRectWithWalls((int x, int y) start, (int x, int y) end)
        {
            if (start.x > end.x || start.y > end.y)
                throw new ArgumentException("Start point must be less than or equal to end point in both dimensions.");
            var withoutWalls = new HashSet<(int x, int y)>();
            var walls = new HashSet<(int x, int y)>();
            for (int x = start.x + 1; x <= end.x - 1; x++)
            {
                for (int y = start.y + 1; y <= end.y - 1; y++)
                {
                    withoutWalls.Add((x, y));
                }
            }
            // Add walls around the rectangle
            for (int x = start.x; x <= end.x; x++)
            {
                walls.Add((x, start.y)); // Top wall
                walls.Add((x, end.y));   // Bottom wall
            }
            for (int y = start.y; y <= end.y; y++)
            {
                walls.Add((start.x, y)); // Left wall
                walls.Add((end.x, y));   // Right wall
            }
            return new PointCluster(withoutWalls, walls);
        }

        public static PointCluster CreateFromRectWithoutWalls((int x, int y) start, (int x, int y) end)
        {
            if (start.x > end.x || start.y > end.y)
                throw new ArgumentException("Start point must be less than or equal to end point in both dimensions.");
            var withoutWalls = new HashSet<(int x, int y)>();
            for (int x = start.x; x <= end.x; x++)
            {
                for (int y = start.y; y <= end.y; y++)
                {
                    withoutWalls.Add((x, y));
                }
            }
            return new PointCluster(withoutWalls, Enumerable.Empty<(int x, int y)>());
        }

        #endregion

        public PointCluster Offset((int x, int y) offset)
        {
            ArgumentNullException.ThrowIfNull(offset, nameof(offset));
            var newWithoutWalls = _withoutWalls.Select(p => (p.x + offset.x, p.y + offset.y)).ToHashSet();
            var newWalls = _walls.Select(p => (p.x + offset.x, p.y + offset.y)).ToHashSet();
            return new PointCluster(newWithoutWalls, newWalls);
        }

        public (int, int) BoundingBoxSize()
        {
            if (_withoutWalls.Count == 0 && _walls.Count == 0)
                return (0, 0);
            int minX = int.MaxValue, maxX = int.MinValue;
            int minY = int.MaxValue, maxY = int.MinValue;
            foreach (var point in _withoutWalls.Concat(_walls))
            {
                if (point.x < minX) minX = point.x;
                if (point.x > maxX) maxX = point.x;
                if (point.y < minY) minY = point.y;
                if (point.y > maxY) maxY = point.y;
            }
            foreach (var wall in _walls)
            {
                if (wall.x < minX) minX = wall.x;
                if (wall.x > maxX) maxX = wall.x;
                if (wall.y < minY) minY = wall.y;
                if (wall.y > maxY) maxY = wall.y;
            }
            return (maxX - minX + 1, maxY - minY + 1);
        }

        public (int, int) BoundingBoxSizeWithoutWalls()
        {
            if (_withoutWalls.Count == 0 && _walls.Count == 0)
                return (0, 0);
            int minX = int.MaxValue, maxX = int.MinValue;
            int minY = int.MaxValue, maxY = int.MinValue;
            foreach (var point in _withoutWalls.Concat(_walls))
            {
                if (point.x < minX) minX = point.x;
                if (point.x > maxX) maxX = point.x;
                if (point.y < minY) minY = point.y;
                if (point.y > maxY) maxY = point.y;
            }
            return (maxX - minX + 1, maxY - minY + 1);
        }

        public ((int, int), (int, int)) BoundingBox()
        {
            if (_withoutWalls.Count == 0 && _walls.Count == 0)
                return ((0, 0), (0, 0));
            int minX = int.MaxValue, maxX = int.MinValue;
            int minY = int.MaxValue, maxY = int.MinValue;
            foreach (var point in _withoutWalls.Concat(_walls))
            {
                if (point.x < minX) minX = point.x;
                if (point.x > maxX) maxX = point.x;
                if (point.y < minY) minY = point.y;
                if (point.y > maxY) maxY = point.y;
            }
            foreach (var wall in _walls)
            {
                if (wall.x < minX) minX = wall.x;
                if (wall.x > maxX) maxX = wall.x;
                if (wall.y < minY) minY = wall.y;
                if (wall.y > maxY) maxY = wall.y;
            }
            return ((minX, minY), (maxX, maxY));
        }

        public ((int, int), (int, int)) BoundingBoxWithoutWalls()
        {
            if (_withoutWalls.Count == 0 && _walls.Count == 0)
                return ((0, 0), (0, 0));
            int minX = int.MaxValue, maxX = int.MinValue;
            int minY = int.MaxValue, maxY = int.MinValue;
            foreach (var point in _withoutWalls.Concat(_walls))
            {
                if (point.x < minX) minX = point.x;
                if (point.x > maxX) maxX = point.x;
                if (point.y < minY) minY = point.y;
                if (point.y > maxY) maxY = point.y;
            }
            return ((minX, minY), (maxX, maxY));
        }

        #region set operations

        public PointCluster MergeRemovingInnerWallsWith(PointCluster other)
        {
            ArgumentNullException.ThrowIfNull(other, nameof(other));

            // Merge all non-wall points
            var combinedWithoutWalls = new HashSet<(int x, int y)>(_withoutWalls);
            combinedWithoutWalls.UnionWith(other._withoutWalls);

            // Merge all wall points
            var allWalls = new HashSet<(int x, int y)>(_walls);
            allWalls.UnionWith(other._walls);

            // Find walls that are present in both clusters
            var intersectionWalls = new HashSet<(int x, int y)>(_walls);
            intersectionWalls.IntersectWith(other._walls);

            // Keep walls that are not inside, or that were present in both clusters
            allWalls.ExceptWith(combinedWithoutWalls);

            return new PointCluster(combinedWithoutWalls, allWalls);
        }

        public PointCluster SubtractRemovingOuterWallsWith(PointCluster cluster)
        {
            ArgumentNullException.ThrowIfNull(cluster, nameof(cluster));

            // Remove all non-wall points that are present in cluster
            var newWithoutWalls = new HashSet<(int x, int y)>(_withoutWalls);
            newWithoutWalls.ExceptWith(cluster._withoutWalls);

            // Remove all walls that are present in cluster's non-walls or walls
            var newWalls = new HashSet<(int x, int y)>(_walls);
            newWalls.RemoveWhere(wall => cluster._withoutWalls.Contains(wall) || cluster._walls.Contains(wall));

            return new PointCluster(newWithoutWalls, newWalls);
        }


        public bool ContainsPoint((int x, int y) point)
        {
            return _withoutWalls.Contains(point) || _walls.Contains(point);
        }

        #endregion

        public bool IsFarFromPoint((int x, int y) point, int manhattanDistance)
        {
            // every point (including walls) must be at least `manhattanDistance` away from the given point
            // Use Manhattan distance (|x1-x2| + |y1-y2|) as is common for grid-based maps.
            foreach (var p in _withoutWalls)
            {
                if (Utils.ManhattanDistance(p, point) < manhattanDistance)
                    return false;
            }
            foreach (var p in _walls)
            {
                if (Utils.ManhattanDistance(p, point) < manhattanDistance)
                    return false;
            }
            return true;
        }

        public bool IsInBounds(int maxX, int maxY)
        {
            // Check if all points are within the bounds of the map
            foreach (var point in _withoutWalls)
            {
                if (point.x < 0 || point.x >= maxX || point.y < 0 || point.y >= maxY)
                    return false;
            }
            foreach (var wall in _walls)
            {
                if (wall.x < 0 || wall.x >= maxX || wall.y < 0 || wall.y >= maxY)
                    return false;
            }
            return true;
        }

        public PointCluster(IEnumerable<(int x, int y)> withoutWalls, IEnumerable<(int x, int y)> walls)
        {
            _withoutWalls = new HashSet<(int x, int y)>(withoutWalls);
            _walls = new HashSet<(int x, int y)>(walls);
        }

        public PointCluster Insert((int x, int y) point)
        {
            _withoutWalls.Add(point);
            return this;
        }

        public PointCluster InsertWall((int x, int y) point)
        {
            _walls.Add(point);
            return this;
        }

        public PointCluster Remove((int x, int y) point)
        {
            _withoutWalls.Remove(point);
            _walls.Remove(point);
            return this;
        }

        public PointCluster Remove(IEnumerable<(int x, int y)> points)
        {
            foreach (var point in points)
            {
                _withoutWalls.Remove(point);
                _walls.Remove(point);
            }
            return this;
        }

        public IEnumerable<(int x, int y)> WithWalls => _withoutWalls.Concat(_walls);

        public int Count => ((IReadOnlyCollection<(int x, int y)>)_withoutWalls).Count;

        public IEnumerable<(int x, int y)> Walls { get => _walls; }

        public bool Contains((int x, int y) item)
        {
            return ((IReadOnlySet<(int x, int y)>)_withoutWalls).Contains(item);
        }

        public IEnumerator<(int x, int y)> GetEnumerator()
        {
            return ((IEnumerable<(int x, int y)>)_withoutWalls).GetEnumerator();
        }

        public bool IsProperSubsetOf(IEnumerable<(int x, int y)> other)
        {
            return ((IReadOnlySet<(int x, int y)>)_withoutWalls).IsProperSubsetOf(other);
        }

        public bool IsProperSupersetOf(IEnumerable<(int x, int y)> other)
        {
            return ((IReadOnlySet<(int x, int y)>)_withoutWalls).IsProperSupersetOf(other);
        }

        public bool IsSubsetOf(IEnumerable<(int x, int y)> other)
        {
            return ((IReadOnlySet<(int x, int y)>)_withoutWalls).IsSubsetOf(other);
        }

        public bool IsSupersetOf(IEnumerable<(int x, int y)> other)
        {
            return ((IReadOnlySet<(int x, int y)>)_withoutWalls).IsSupersetOf(other);
        }

        public bool Overlaps(IEnumerable<(int x, int y)> other)
        {
            return ((IReadOnlySet<(int x, int y)>)_withoutWalls).Overlaps(other);
        }

        public bool SetEquals(IEnumerable<(int x, int y)> other)
        {
            return ((IReadOnlySet<(int x, int y)>)_withoutWalls).SetEquals(other);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_withoutWalls).GetEnumerator();
        }

        public bool Intersects(PointCluster other)
        {
            ArgumentNullException.ThrowIfNull(other, nameof(other));

            // Check if any point (including walls) overlaps between the two clusters
            // Check _withoutWalls
            if (_withoutWalls.Overlaps(other._withoutWalls) ||
                _withoutWalls.Overlaps(other._walls) ||
                _walls.Overlaps(other._withoutWalls) ||
                _walls.Overlaps(other._walls))
            {
                return true;
            }
            return false;
        }

        public bool IntersectsIgnoringWalls(PointCluster other)
        {
            ArgumentNullException.ThrowIfNull(other, nameof(other));

            // Only consider non-wall points for intersection
            return _withoutWalls.Overlaps(other._withoutWalls);
        }
    }
    public static class Extensions
    {
        public static T[] Shuffle<T>(this T[] array, Random random)
        {
            for (int i = array.Length - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (array[i], array[j]) = (array[j], array[i]);
            }
            return array;
        }
    }
}
