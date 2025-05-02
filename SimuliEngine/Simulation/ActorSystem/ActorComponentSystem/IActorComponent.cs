using SimuliEngine.Basic;
using SimuliEngine.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimuliEngine.Simulation.ActorSystem.ActorComponentSystem
{
    public interface IActorComponent: IDumpable, ILoggable
    {
        public string ComponentName { get; }

        public void Initialize(Actor actor, WorldState world);

        public void Update(float deltaTime, Actor actor, WorldState world);

        public bool IsIndependent { get; }

        public UpdateFrequency RequiredUpdateFrequency { get; }
    }
}
