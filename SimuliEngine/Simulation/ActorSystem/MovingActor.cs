using SimuliEngine.Simulation.Obstacles;
using SimuliEngine.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SimuliEngine.Simulation.ActorSystem
{
    public class MovingActor : Actor
    {
        public float ActualSpeed { get; set; }
        public Vector2 Direction { get; set; } = new Vector2(0, 0); // Default direction is nowhere

        public MovingActor(WorldState stateReference, IRealPositionProvider positionProvider) : base(stateReference, positionProvider)
        {

        }

        //TODO: define size dynamically
        public override float Size => 0.5f;

        //TODO: calculate half size in constructior
        public override float HalfSize => 0.25f;

        public virtual void HitObstacle(IObstacle obstacle)
        {

        }

        public virtual void CenterChanged()
        {

        }

        public override void MovePosition(float deltaTime)
        {
            if (Direction == Vector2.Zero)
                return;
            var prevPosition = _positionProvider.GetNormalizedPosition();
            _positionProvider.Move(ActualSpeed * deltaTime, Direction);
            if (State.ObstacleTracker.CheckMoveHitObstacle(this, _positionProvider.GetNormalizedPosition()))
            {
                // Move back to the previous position
                _positionProvider.SetNormalizedPosition(prevPosition);
            }
            else
            {
                State.ObstacleTracker.ConfirmMove(this);
            }
        }

        public override void Think(float deltaTime)
        {
            throw new NotImplementedException();
        }

        public override bool TryMove(float deltaTime)
        {
            throw new NotImplementedException();
        }
    }
}
