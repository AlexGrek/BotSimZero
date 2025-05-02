using SimuliEngine.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimuliEngine.Simulation.ActorSystem
{
    public class StaticActor : Actor
    {
        public StaticActor(WorldState stateReference, IRealPositionProvider positionProvider, IRealRotationProvider rotationProvider) : base(stateReference, positionProvider, rotationProvider)
        {

        }

        //TODO: define size dynamically
        public override float Size => 0.5f;

        //TODO: calculate half size in constructior
        public override float HalfSize => 0.25f;

        public override void MovePosition(float deltaTime)
        {
            //throw new NotImplementedException();
        }

        public override void Think(float deltaTime)
        {
            //throw new NotImplementedException();
        }

        public override bool TryMove(float deltaTime)
        {
            return true;
        }
    }
}
