using SimuliEngine.Basic;
using SimuliEngine.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimuliEngine.Simulation.ActorSystem.TaskingSystem
{
    public abstract class LowLevelTask: IDumpable
    {
        public virtual LowLevelTaskClass TaskClass => LowLevelTaskClass.None;

        public abstract bool IsCompleted { get; }

        public abstract bool ExecuteConcurrently(float deltaTime, MovingActor actor, WorldState world);

        public bool RecoverFromInterruption(object interruption)
        {
            return false;
        }
    }
}
