using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimuliEngine.Simulation.ActorSystem.TaskingSystem
{
    public enum LogicalTaskStatus
    {
        NotStarted,
        InProgress,
        Interrupted,
        Finished,
        Cancelled
    }
}
