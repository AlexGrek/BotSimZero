using SimuliEngine.Basic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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
