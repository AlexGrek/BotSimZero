using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimuliEngine.Simulation.ActorSystem.Pathfinding
{
    public class Path: IPathfindingPath, IComparable<Path>
    {
        public IList<(int x, int y)> Points { get; private set; }

        public int InitialLength { get; private set; } = 0;

        public int Length => Points.Count;

        public (int x, int y)? NextPoint => Points.FirstOrDefault();

        public Path(IList<(int x, int y)> points)
        {
            Points = points ?? throw new ArgumentNullException(nameof(points));
            InitialLength = points.Count;
        }

        public (int x, int y)? ConsumeNextPoint()
        {
            return Points.Count > 0 ? Points[0] : null;
        }

        public bool TryGetNextPoint(out (int x, int y) point)
        {
            if (Points.Count > 0)
            {
                point = Points[0];
                return true;
            }
            else
            {
                point = (0, 0); // Default value if no points are available
                return false;
            }
        }

        public bool TryConsumeNextPoint(out (int x, int y) point)
        {
            if (Points.Count > 0)
            {
                point = Points[0];
                Points.RemoveAt(0);
                return true;
            }
            else
            {
                point = (0, 0); // Default value if no points are available
                return false;
            }
        }

        public int CompareTo(Path? other)
        {
            if (other == null)
                return 1;
            if (this.Length == other.Length) { return 0; }
            if (this.Length < other.Length)
                return -1;
            else
                return 1;
        }
    }
}
