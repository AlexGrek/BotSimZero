using SimuliEngine.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimuliEngine.Simulation
{
    public abstract class SimulationSubsystem
    {
        public bool Enabled { get; set; } = true; // enable each subsystem by default

        public abstract void Tick(float dt, WorldState world);
    }
}
