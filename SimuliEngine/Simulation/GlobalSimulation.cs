using SimuliEngine.Simulation.Subsystems;
using SimuliEngine.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimuliEngine.Simulation
{
    public class GlobalSimulation: SimulationSubsystem
    {
        public List<SimulationSubsystem> Subsystems { get; private set; }

        public GlobalSimulation()
        {
            Subsystems = new List<SimulationSubsystem>();
            Subsystems.AddRange(
            [
                new Subsystems.TemperatureSimulationSubsystem(),
                new SmartEntitySimulationSubsystem(),
            ]);
        }

        public IEnumerable<SimulationSubsystem> GetEnabledSubsystems()
        {
            return Subsystems.Where(s => s.Enabled);
        }

        public override void Tick(float dt, WorldState world)
        {
            foreach (var subsystem in GetEnabledSubsystems())
            {
                subsystem.Tick(dt, world);
            }
        }
    }
}
