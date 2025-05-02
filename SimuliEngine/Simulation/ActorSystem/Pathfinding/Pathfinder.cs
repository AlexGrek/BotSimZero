using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimuliEngine.Simulation.ActorSystem.Pathfinding
{
    public static class Pathfinder
    {
        private readonly struct Node
        {
            public readonly int X, Y;
            public readonly float CostFromStart;

            public Node(int x, int y, float g)
            {
                X = x;
                Y = y;
                CostFromStart = g;
            }
        }

        public static Path? AStarFree(
            int startX, int startY,
            int goalX, int goalY,
            int width, int height,
            Func<int, int, bool> canPass,
            Func<int, int, float> getCost)
        {
            var cameFrom = new (int x, int y)?[width, height];
            var costSoFar = new float[width, height];
            var visited = new bool[width, height];

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    costSoFar[x, y] = float.MaxValue;

            var open = new PriorityQueue<Node, float>();
            open.Enqueue(new Node(startX, startY, 0), Heuristic(startX, startY, goalX, goalY));
            costSoFar[startX, startY] = 0;

            // Directions: N, NE, E, SE, S, SW, W, NW
            int[] dx = { 0, 1, 1, 1, 0, -1, -1, -1 };
            int[] dy = { -1, -1, 0, 1, 1, 1, 0, -1 };
            float[] moveCost = { 1f, 1.4142f, 1f, 1.4142f, 1f, 1.4142f, 1f, 1.4142f };

            while (open.Count > 0)
            {
                var current = open.Dequeue();
                int x = current.X;
                int y = current.Y;

                if (visited[x, y])
                    continue;
                visited[x, y] = true;

                if (x == goalX && y == goalY)
                    return new Path(ReconstructPath(cameFrom, x, y));

                for (int i = 0; i < 8; i++)
                {
                    int nx = x + dx[i];
                    int ny = y + dy[i];

                    if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                        continue;
                    if (!canPass(nx, ny))
                        continue;

                    float stepCost = getCost(nx, ny) * moveCost[i];
                    float newCost = costSoFar[x, y] + stepCost;

                    if (newCost < costSoFar[nx, ny])
                    {
                        costSoFar[nx, ny] = newCost;
                        float priority = newCost + Heuristic(nx, ny, goalX, goalY);
                        open.Enqueue(new Node(nx, ny, newCost), priority);
                        cameFrom[nx, ny] = (x, y);
                    }
                }
            }

            return null; // No path found
        }

        private static float Heuristic(int x1, int y1, int x2, int y2)
        {
            // Octile distance heuristic
            int dx = Math.Abs(x1 - x2);
            int dy = Math.Abs(y1 - y2);
            return (float)(Math.Max(dx, dy) + (Math.Sqrt(2) - 1) * Math.Min(dx, dy));
        }

        private readonly struct CostPoint : IComparable<CostPoint>
        {
            public readonly int X, Y;
            public readonly float Cost;

            public CostPoint(int x, int y, float cost)
            {
                X = x;
                Y = y;
                Cost = cost;
            }

            public int CompareTo(CostPoint other)
            {
                int cmp = Cost.CompareTo(other.Cost);
                if (cmp != 0) return cmp;
                cmp = X.CompareTo(other.X); // Tie-breaker
                if (cmp != 0) return cmp;
                return Y.CompareTo(other.Y);
            }

            public (int x, int y) ToTuple() => (X, Y);
        }

        public static Path OptimizePath(
        Path original,
        Func<int, int, bool> canPass)
        {
            var input = original.Points;
            if (input == null || input.Count < 3)
                return original;

            var optimized = new List<(int x, int y)>();
            int i = 0;

            while (i < input.Count)
            {
                var start = input[i];
                optimized.Add(start);

                int j = i + 1;
                if (j >= input.Count)
                    break;

                var dir = (dx: input[j].x - start.x, dy: input[j].y - start.y);

                // Normalize direction to unit step
                dir = Normalize(dir);

                // Try to follow straight direction as far as possible
                while (j + 1 < input.Count &&
                       Normalize((input[j + 1].x - input[j].x, input[j + 1].y - input[j].y)) == dir)
                {
                    j++;
                }

                // Now check for a diagonal ladder shortcut: A->B->C can be A->C
                if (i + 2 < input.Count)
                {
                    var a = input[i];
                    var b = input[i + 1];
                    var c = input[i + 2];

                    int dxAC = c.x - a.x;
                    int dyAC = c.y - a.y;

                    if (Math.Abs(dxAC) == 1 && Math.Abs(dyAC) == 1)
                    {
                        // Diagonal shortcut possible if middle (b) is split between two orthogonals
                        // and the diagonal move (a -> c) is legal
                        if (canPass(c.x, c.y) && canPass(a.x, c.y) && canPass(c.x, a.y))
                        {
                            optimized.Add(c);
                            i += 3;
                            continue;
                        }
                    }
                }

                // Move to the last step of the straight segment
                i = j;
            }

            // Ensure the final point is included
            if (optimized[^1] != input[^1])
                optimized.Add(input[^1]);

            return new Path(optimized);
        }

        private static (int dx, int dy) Normalize((int dx, int dy) dir)
        {
            if (dir.dx != 0) dir.dx = dir.dx / Math.Abs(dir.dx);
            if (dir.dy != 0) dir.dy = dir.dy / Math.Abs(dir.dy);
            return dir;
        }

        public static SortedSet<(int x, int y)> FindReachablePointsWithCost4(
        int startX, int startY,
        int width, int height,
        float maxCost,
        Func<int, int, bool> canPass,
        Func<int, int, float> getCost,
        Predicate<(int x, int y)> predicate)
        {
            var result = new SortedSet<CostPoint>();
            var costSoFar = new float[width, height];
            var visited = new bool[width, height];

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    costSoFar[x, y] = float.MaxValue;

            var queue = new PriorityQueue<CostPoint, float>();
            var start = new CostPoint(startX, startY, 0);
            queue.Enqueue(start, 0);
            costSoFar[startX, startY] = 0;

            // Orthogonal directions only: N, E, S, W
            int[] dx = { 0, 1, 0, -1 };
            int[] dy = { -1, 0, 1, 0 };

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                int x = current.X;
                int y = current.Y;
                float currentCost = current.Cost;

                if (visited[x, y])
                    continue;
                visited[x, y] = true;

                if (predicate((x, y)))
                    result.Add(current);

                for (int i = 0; i < 4; i++)
                {
                    int nx = x + dx[i];
                    int ny = y + dy[i];

                    if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                        continue;
                    if (!canPass(nx, ny))
                        continue;

                    float stepCost = getCost(nx, ny);
                    float newCost = currentCost + stepCost;

                    if (newCost > maxCost || newCost >= costSoFar[nx, ny])
                        continue;

                    costSoFar[nx, ny] = newCost;
                    queue.Enqueue(new CostPoint(nx, ny, newCost), newCost);
                }
            }

            // Convert to (int x, int y) set
            var output = new SortedSet<(int x, int y)>();
            foreach (var cp in result)
                output.Add(cp.ToTuple());

            return output;
        }

        public static SortedSet<(int x, int y)> FindReachablePointsWithCost(
            int startX, int startY,
            int width, int height,
            float maxCost,
            Func<int, int, bool> canPass,
            Func<int, int, float> getCost,
            Predicate<(int x, int y)> predicate)
        {
            var result = new SortedSet<CostPoint>();
            var costSoFar = new float[width, height];
            var visited = new bool[width, height];

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    costSoFar[x, y] = float.MaxValue;

            var queue = new PriorityQueue<CostPoint, float>();
            var start = new CostPoint(startX, startY, 0);
            queue.Enqueue(start, 0);
            costSoFar[startX, startY] = 0;

            // Orthogonal + Diagonal directions
            int[] dx = { 0, 1, 1, 1, 0, -1, -1, -1 };
            int[] dy = { -1, -1, 0, 1, 1, 1, 0, -1 };
            float[] moveCost = { 1f, 1.4142f, 1f, 1.4142f, 1f, 1.4142f, 1f, 1.4142f };

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                int x = current.X;
                int y = current.Y;
                float currentCost = current.Cost;

                if (visited[x, y])
                    continue;
                visited[x, y] = true;

                if (predicate((x, y)))
                    result.Add(current);

                for (int i = 0; i < 8; i++)
                {
                    int nx = x + dx[i];
                    int ny = y + dy[i];

                    if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                        continue;
                    if (!canPass(nx, ny))
                        continue;

                    float step = getCost(nx, ny) * moveCost[i];
                    float newCost = currentCost + step;

                    if (newCost > maxCost || newCost >= costSoFar[nx, ny])
                        continue;

                    costSoFar[nx, ny] = newCost;
                    queue.Enqueue(new CostPoint(nx, ny, newCost), newCost);
                }
            }

            // Convert to (int x, int y) set
            var output = new SortedSet<(int x, int y)>();
            foreach (var cp in result)
                output.Add(cp.ToTuple());

            return output;
        }

        public static Path? AStar4(
        int startX, int startY,
        int goalX, int goalY,
        int width, int height,
        Func<int, int, bool> canPass,
        Func<int, int, float> getCost)
        {
            var cameFrom = new (int x, int y)?[width, height];
            var costSoFar = new float[width, height];
            var visited = new bool[width, height];

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    costSoFar[x, y] = float.MaxValue;

            var open = new PriorityQueue<Node, float>();
            open.Enqueue(new Node(startX, startY, 0), ManhattanHeuristic(startX, startY, goalX, goalY));
            costSoFar[startX, startY] = 0;

            // Directions: N, E, S, W
            int[] dx = { 0, 1, 0, -1 };
            int[] dy = { -1, 0, 1, 0 };

            while (open.Count > 0)
            {
                var current = open.Dequeue();
                int x = current.X;
                int y = current.Y;

                if (visited[x, y])
                    continue;
                visited[x, y] = true;

                if (x == goalX && y == goalY)
                    return new Path(ReconstructPath(cameFrom, x, y));

                for (int i = 0; i < 4; i++)
                {
                    int nx = x + dx[i];
                    int ny = y + dy[i];

                    if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                        continue;
                    if (!canPass(nx, ny))
                        continue;

                    float stepCost = getCost(nx, ny);
                    float newCost = costSoFar[x, y] + stepCost;

                    if (newCost < costSoFar[nx, ny])
                    {
                        costSoFar[nx, ny] = newCost;
                        float priority = newCost + ManhattanHeuristic(nx, ny, goalX, goalY);
                        open.Enqueue(new Node(nx, ny, newCost), priority);
                        cameFrom[nx, ny] = (x, y);
                    }
                }
            }

            return null; // No path found
        }

        public static Path? FullyOptimizePath(
        Path original,
        Func<int, int, bool> canPass)
        {
            if (original == null || original.Points.Count < 3)
                return original;

            // 1. Straight-line and diagonal ladder optimization
            var step1 = OptimizePath(original, canPass);

            // 2. Curve smoothing with full passability check
            var step2 = SmoothPathWithCurvesByChatgptDoesNotWork(step1, canPass);

            return step2;
        }

        public static Path SmoothPathWithCurvesByChatgptDoesNotWork(
    Path original,
    Func<int, int, bool> canPass)
        {
            var input = original.Points;
            if (input == null || input.Count < 3)
                return original;

            var smoothed = new List<(int x, int y)>();
            int i = 0;

            while (i < input.Count)
            {
                var start = input[i];
                smoothed.Add(start);

                int farthest = i + 1;
                for (int j = input.Count - 1; j > i + 1; j--)
                {
                    if (LinePassable(start, input[j], canPass))
                    {
                        farthest = j;
                        break;
                    }
                }

                i = farthest;
            }

            if (smoothed[^1] != input[^1])
                smoothed.Add(input[^1]);

            return new Path(smoothed);
        }

        // Bresenham-style line passability check
        private static bool LinePassable((int x, int y) a, (int x, int y) b, Func<int, int, bool> canPass)
        {
            int x0 = a.x, y0 = a.y;
            int x1 = b.x, y1 = b.y;

            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                if (!canPass(x0, y0))
                    return false;

                if (x0 == x1 && y0 == y1)
                    break;

                int e2 = 2 * err;
                if (e2 > -dy) { err -= dy; x0 += sx; }
                if (e2 < dx) { err += dx; y0 += sy; }
            }

            return true;
        }

        private static float ManhattanHeuristic(int x1, int y1, int x2, int y2)
        {
            return Math.Abs(x1 - x2) + Math.Abs(y1 - y2);
        }

        private static IList<(int x, int y)> ReconstructPath((int x, int y)?[,] cameFrom, int endX, int endY)
        {
            var path = new List<(int x, int y)>();
            int x = endX, y = endY;
            while (cameFrom[x, y].HasValue)
            {
                path.Add((x, y));
                var prev = cameFrom[x, y]!.Value;
                x = prev.x;
                y = prev.y;
            }
            path.Add((x, y)); // start
            path.Reverse();
            return path;
        }
    }
}
