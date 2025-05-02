using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimuliEngine.Simulation.ActorSystem
{
    public interface IPointProvider
    {
        public (int x, int y) GetPoint();
    }
}
