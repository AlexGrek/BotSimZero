using SimuliEngine.Basic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimuliEngine.Simulation.ActorSystem.TaskingSystem
{
    public struct MovementStep: IDumpable
    {
        public (int x, int y) TargetCell;
        public bool RotateFacingTargetOnly = false;

        public MovementStep()
        {
        }

        public MovementStep((int x, int y) targetCell)
        {
            TargetCell = targetCell;
        }
    }
}
