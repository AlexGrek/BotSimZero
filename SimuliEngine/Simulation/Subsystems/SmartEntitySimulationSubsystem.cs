using SimuliEngine.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimuliEngine.Simulation.Subsystems
{
    public class SmartEntitySimulationSubsystem : SimulationSubsystem
    {
        public override void Tick(float dt, WorldState world)
        {
            SimulateParallelThinkProcess(dt, world);
            SimulateActions(dt, world);
            SimulateMovement(dt, world);
        }

        private void SimulateActions(float dt, WorldState world)
        {
            foreach (var actor in world.Actors)
            {
                
            }
        }

        private void SimulateMovement(float dt, WorldState world)
        {
            foreach (var actor in world.Actors)
            {
                actor.MovePosition(dt);
            }
        }

        private void SimulateParallelThinkProcess(float dt, WorldState world)
        {
            world.Actors.AsParallel()
                .ForAll(actor =>
                {
                    actor.Think(dt);
                });
        }
    }
}
