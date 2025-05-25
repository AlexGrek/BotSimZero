using SimuliEngine.Simulation.ActorSystem.Pathfinding;
using SimuliEngine.Simulation.ActorSystem.TaskingSystem;
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
        public virtual Intellect<MovingActor> Intellect { get; protected set; }

        public float ActualSpeed { get; set; } = 1.0f;
        public float RotationSpeed { get; set; } = 4f; // Rotation speed in units per second  
        public Vector2 Direction { get; set; } = new Vector2(0, 0); // Default direction is nowhere  

        private Random _rnd = new Random();

        public IObstacle? FailedToMove { get; set; } = null; // Default failed to move is nowhere  

        public MovementStep? MovementStep { get; set; } = null; // Default target position is nowhere  

        public MovingActor(WorldState stateReference, IRealPositionProvider positionProvider, IRealRotationProvider rotationProvider)
            : base(stateReference, positionProvider, rotationProvider)
        {
            Intellect = new Intellect<MovingActor>(this);
        }

        //TODO: define size dynamically  
        public override float Size => 0.5f;

        //TODO: calculate half size in constructor  
        public override float HalfSize => 0.25f;

        public virtual void HitObstacle(IObstacle obstacle)
        {
            FailedToMove = obstacle;

            Direction = Vector2.Zero; // Stop moving when hitting an obstacle  
            MovementStep = null; // Clear the target when hitting an obstacle
            Intellect.ConsiderMovementStepFailed(FailedToMove, State);
        }

        public virtual void CenterChanged()
        {
            Intellect.ConsiderCenterChanged();
            foreach (var component in this.ActorComponents)
            {
                component.OnMainCellChanged(this, _stateReference, PrevMainPosition, MainPosition);
            }
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

        public Vector2 GetNormalizedRotation()
        {
            return _rotationProvider.GetRotationDirection();
        }

        public IEnumerable<((int cellX, int cellY), (int subX, int subY))> TouchingSubcells()
        {
            return this.State.ObstacleTracker.GetTouchedSubcellsByObject(this);
        }

        public virtual bool ReachedTargetCell((int x, int y) targetCell)
        {
            return ReachedTargetCell(new Vector2(targetCell.x, targetCell.y));
        }

        public virtual bool ReachedTargetCell(Vector2 targetPosition)
        {
            return Vector2.DistanceSquared(_positionProvider.GetWorldCoordinates(), targetPosition) < 0.01f; // Adjust the threshold as needed  
        }

        public void EndMovementStep()
        {
            Direction = Vector2.Zero;
            MovementStep = null;
            Intellect.ConsiderMovementStepCompleted();
        }

        public virtual void MoveAndRotateTowardsTarget(float deltaTime)
        {
            if (CenterPositionChanged.ReadAndReset())
            {
                CenterChanged();
            }
            if (MovementStep.HasValue)
            {
                var (x, y) = MovementStep.Value.TargetCell;
                var targetPosition = new Vector2(x, y);
                var currentPosition = _positionProvider.GetWorldCoordinates();

                if (!MovementStep.Value.RotateFacingTargetOnly)
                {
                    if (ReachedTargetCell(targetPosition))
                    {
                        EndMovementStep();
                        return;
                    }

                    // Calculate direction towards the target
                    var directionToTarget = targetPosition - currentPosition;

                    if (directionToTarget != Vector2.Zero)
                    {
                        Direction = Vector2.Normalize(directionToTarget);
                    }
                    else
                    {
                        Direction = Vector2.Zero; // Stop moving
                    }
                }
                else
                {
                    // No target, so stop moving  
                    Direction = Vector2.Zero;
                }
                // Rotate towards the target direction  
                _rotationProvider.LookAtSmooth(RotationSpeed * deltaTime, targetPosition);
                if (MovementStep.Value.RotateFacingTargetOnly)
                {
                    var done = ReachedTargetCellRotation(targetPosition);
                    if (done)
                    {
                        EndMovementStep();
                    }
                }
            }
        }

        private bool ReachedTargetCellRotation(Vector2 targetPosition)
        {
            return _rotationProvider.IsFacing(targetPosition);
        }

        public override void Think(float deltaTime)
        {
            Intellect.ProcessBehaviors(deltaTime, State);
            Intellect.ProcessLogicalTask(deltaTime, State);
            MoveAndRotateTowardsTarget(deltaTime);
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

        public float GetPassCost(WorldState state, (int x, int y) coordinates)
        {
            return !TileType.IsWall(state.TileTypeMap[coordinates.x, coordinates.y]) ? 1 : 100;
        }

        public void SwapPositionProvider(IRealPositionProvider newPositionProvider)
        {
            newPositionProvider.SetWorldCoordinates(_positionProvider.GetWorldCoordinates());
            _positionProvider = newPositionProvider;
        }

        public void SwapRotationProvider(IRealRotationProvider newRotationProvider)
        {
            //TODO: correct swapping
            newRotationProvider.LookAtImmediately(_rotationProvider.GetRotationDirection());
            _rotationProvider = newRotationProvider;
        }

        public static class PathfindHelper
        {
            public static Pathfinding.Path? AStar(MovingActor actor, (int x, int y) target, WorldState world)
            {
                var unoptimized = Pathfinder.AStar4(actor.MainPosition.x, actor.MainPosition.y, target.x, target.y,
                    world.SizeX, world.SizeY, (x, y) => actor.IsPassable(world, (x, y)),
                    (x, y) => actor.GetPassCost(world, (x, y)));
                if (unoptimized == null)
                    return null;
                var optimized = Pathfinder.OptimizePath(unoptimized, (x, y) => actor.IsPassable(world, (x, y)));
                return optimized;
            }

            public static SortedSet<(int x, int y)> FindAllReachablePointsOf(MovingActor actor, WorldState world, Predicate<(int x, int y)> predicate, float max=100f)
            {
                var points = Pathfinder.FindReachablePointsWithCost4(actor.MainPosition.x, actor.MainPosition.y, world.SizeX, world.SizeY, max,
                    (x, y) => actor.IsPassable(world, (x, y)),
                    (x, y) => actor.GetPassCost(world, (x, y)),
                    predicate);
                return points;
            }

            public static (int x, int y)? FindClosestPointOf(MovingActor actor, WorldState world, Predicate<(int x, int y)> predicate, float max = 100f)
            {
                var points = FindAllReachablePointsOf(actor, world, predicate);
                return points.FirstOrDefault();
            }
        }
    }
}
