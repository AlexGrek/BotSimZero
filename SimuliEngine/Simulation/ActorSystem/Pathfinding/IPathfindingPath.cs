using SimuliEngine.Basic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimuliEngine.Simulation.ActorSystem.Pathfinding
{
    public interface IPathfindingPath: IDumpable
    {
        public (int x, int y)? NextPoint { get; }

        public (int x, int y)? ConsumeNextPoint();

        public bool TryGetNextPoint(out (int x, int y) point);

        public bool TryConsumeNextPoint(out (int x, int y) point);
    }
}
