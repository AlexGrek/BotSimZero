using SimuliEngine.Simulation.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimuliEngine.World
{
    public class CellActorReference
    {
        public Actor Actor { get; set; }
        public (float, float) Offset { get; set; }

        public CellActorReference(Actor actor, (float, float) offset)
        {
            Actor = actor;
            Offset = offset;
        }
    }
}
