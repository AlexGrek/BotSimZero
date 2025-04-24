using SimuliEngine.Simulation.Obstacles;
using SimuliEngine.Tiles;
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
        public float ActualSpeed { get; set; } = 1.0f;
        public Vector2 Direction { get; set; } = new Vector2(0, 0); // Default direction is nowhere

        private Random _rnd = new Random();

        protected IObstacle? FailedToMove { get; set; } = null; // Default failed to move is nowhere

        public (int targetX, int targetY)? MovementTarget { get; set; } = null; // Default target position is nowhere


        public MovingActor(WorldState stateReference, IRealPositionProvider positionProvider) : base(stateReference, positionProvider)
        {

        }

        //TODO: define size dynamically
        public override float Size => 0.5f;

        //TODO: calculate half size in constructior
        public override float HalfSize => 0.25f;

        public virtual void HitObstacle(IObstacle obstacle)
        {
            FailedToMove = obstacle;
            
            Direction = Vector2.Zero; // Stop moving when hitting an obstacle
            MovementTarget = null; // Clear the target when hitting an obstacle
        }

        public virtual void CenterChanged()
        {

        }

        public override void MovePosition(float deltaTime)
        {
            if (Direction == Vector2.Zero)
                return;

            var prevPosition = _positionProvider.GetWorldCoordinates();
            _positionProvider.Move(ActualSpeed * deltaTime, Direction);

            var obstacle = State.ObstacleTracker.CheckMove(this, State, _positionProvider.GetWorldCoordinates());
            if (obstacle != null)
            {
                // Move back to the previous position
                _positionProvider.SetWorldCoordinates(prevPosition);
                HitObstacle(obstacle);
            }
            else
            {
                State.ObstacleTracker.ConfirmMove(this);
            }
        }

        public virtual void BlindlyMoveTowardsTarget()
        {
            if (MovementTarget != null)
            {
                var target = MovementTarget.Value;
                var targetPosition = new Vector2(target.targetX, target.targetY);
                Direction = Vector2.Normalize(targetPosition - _positionProvider.GetWorldCoordinates());
            }
            else
            {
                // No target, so stop moving
                Direction = Vector2.Zero;

                //DEBUG:
                // move to a random position
                MovementTarget = _rnd.RandomPoint(State.SizeX, State.SizeY);
            }
        }

        public override void Think(float deltaTime)
        {
            BlindlyMoveTowardsTarget();
        }

        public override bool TryMove(float deltaTime)
        {
            MovePosition(deltaTime);
            return FailedToMove == null;
        }

        public override bool IsPassable(WorldState state, (int x, int y) coordinates)
        {
            return !TileType.IsWall(state.TileTypeMap[coordinates.x, coordinates.y]);
        }
    }
}
