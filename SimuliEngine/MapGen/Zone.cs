using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimuliEngine.MapGen
{
    public class Zone<T> where T: IEquatable<T>
    {
        public readonly T Definition;

        public int ZoneId = 0;

        public PointCluster Points { get; private set; } = new PointCluster([], []);

        public Dictionary<string, object> Properties { get; private set; } = new Dictionary<string, object>();

        public Zone(T definition, PointCluster points)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            Points = points ?? throw new ArgumentNullException(nameof(points));
        }

        public Zone(T definition)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        }

        public Zone(T definition, IEnumerable<(int x, int y)> points)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            Points = new PointCluster(points, []);
        }

        public Zone(T definition, IEnumerable<(int x, int y)> points, IEnumerable<(int x, int y)> walls)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            Points = new PointCluster(points, walls);
        }
    }
}
