using SimuliEngine.Simulation.ActorSystem.TaskingSystem;
using SimuliEngine.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimuliEngine.Simulation.ActorSystem.Bots
{
    public class BotActor : MovingActor
    {
        public BotActor(WorldState stateReference, IRealPositionProvider positionProvider, IRealRotationProvider rotationProvider) : base(stateReference, positionProvider, rotationProvider)
        {
            
        }
    }
}
