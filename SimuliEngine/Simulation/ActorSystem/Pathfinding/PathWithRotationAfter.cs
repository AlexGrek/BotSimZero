using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimuliEngine.Simulation.ActorSystem.Pathfinding
{
    public class PathWithRotationAfter: Path
    {
        public (int x, int y) RotationAfter { get; private set; }

        public PathWithRotationAfter(IList<(int x, int y)> points, (int x, int y) facePoint): base(points)
        {
            RotationAfter = facePoint;
        }
    }
}
