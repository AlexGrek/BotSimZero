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
        private float _lowTimeAcc = 0f;
        private float _highTimeAcc = 0f;

        public float LowTimeMax = 1f;
        public float HighTimeMax = 0.1f;

        public override void Tick(float dt, WorldState world)
        {
            SimulateUpdateActorComponents(dt, world);
            SimulateParallelThinkProcess(dt, world);
            SimulateActions(dt, world);
            SimulateMovement(dt, world);
        }

        private void SimulateUpdateActorComponents(float dt, WorldState world)
        {
            foreach (var actor in world.Actors)
            {
                actor.UpdateComponentsFrame(dt);
            }
            if (_lowTimeAcc > LowTimeMax)
            {
                foreach (var actor in world.Actors)
                {
                    actor.UpdateComponentsLow(_lowTimeAcc);
                }
                _lowTimeAcc = 0f;
            }
            else
            {
                _lowTimeAcc += dt;
            }

            if (_highTimeAcc > HighTimeMax)
            {
                foreach (var actor in world.Actors)
                {
                    actor.UpdateComponentsHigh(_highTimeAcc);
                }
                _highTimeAcc = 0f;
            }
            else
            {
                _highTimeAcc += dt;
            }
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
            foreach (var actor in world.Actors)
            {
                actor.Think(dt);
            }
            //world.Actors.AsParallel()
            //    .ForAll(actor =>
            //    {
            //        actor.Think(dt);
            //    });
        }
    }
}
