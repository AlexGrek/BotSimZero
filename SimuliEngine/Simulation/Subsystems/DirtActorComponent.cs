using SimuliEngine.Basic;
using SimuliEngine.Simulation.ActorSystem;
using SimuliEngine.Simulation.ActorSystem.ActorComponentSystem;
using SimuliEngine.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace SimuliEngine.Simulation.Subsystems
{
    public class DirtActorComponent : IActorComponent
    {
        public float DirtLevel = 0.1f;

        public float DirtMinLevel = 0.1f;

        public float DirtApplicationRate = 0.1f;

        public float DirtAquisitionRate = 0.2f;

        public string ComponentName => "DirtActor";

        public bool IsIndependent => true;

        public UpdateFrequency RequiredUpdateFrequency => UpdateFrequency.OnDemand;

        public void Initialize(Actor actor, WorldState world)
        {
            
        }

        public void Update(float deltaTime, Actor actor, WorldState world)
        {
            
        }

        public void OnMainCellChanged(Actor actor, WorldState world, (int x, int y) prev, (int x, int y) next)
        {
            var prevDirt = world.Dirt[prev.x, prev.y];
            world.Dirt[prev.x, prev.y] = Math.Min(1f, prevDirt + DirtLevel * DirtApplicationRate);
            DirtLevel = Math.Min(1f, DirtLevel + (DirtLevel - prevDirt) * DirtAquisitionRate);
            if (DirtLevel < DirtMinLevel)
            {
                DirtLevel = DirtMinLevel;
            }
        }
    }
}
