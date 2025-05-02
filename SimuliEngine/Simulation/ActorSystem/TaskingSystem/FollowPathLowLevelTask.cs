using SimuliEngine.Simulation.ActorSystem.Pathfinding;
using SimuliEngine.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimuliEngine.Simulation.ActorSystem.TaskingSystem
{
    internal class FollowPathLowLevelTask : LowLevelTask
    {
        private readonly IPathfindingPath _path;
        private bool _pathCompleted = false;

        public IPathfindingPath Path => _path;

        private bool _rotationConsumed = false;

        public bool IsRotationConsumed => _rotationConsumed;

        public bool HasRotation => _path is PathWithRotationAfter;

        public FollowPathLowLevelTask(IPathfindingPath path, MovingActor actor, WorldState world, LogicalTask task)
        {
            _path = path ?? throw new ArgumentNullException(nameof(path));
        }

        public override bool IsCompleted => _pathCompleted;

        public override bool ExecuteConcurrently(float deltaTime, MovingActor actor, WorldState world)
        {
            if (actor.MovementStep == null)
            {
                if (_path.TryConsumeNextPoint(out var nextPoint))
                {
                    // If we can consume the next point, we set it as the target cell for the actor
                    actor.MovementStep = new MovementStep(nextPoint);
                    return true;
                }
                else
                {
                    if (_path is PathWithRotationAfter pathWithRotation)
                    {
                        // If the path is a PathWithRotationAfter, we need to handle the rotation
                        if (_rotationConsumed)
                        {
                            // If the rotation has already been consumed, we can assume the path is completed
                            _pathCompleted = true;
                            return false; // No more points to consume
                        }
                        actor.MovementStep = new MovementStep { RotateFacingTargetOnly = true, TargetCell = pathWithRotation.RotationAfter };
                        _rotationConsumed = true;
                    }
                    else
                    {
                        // If the path is not a PathWithRotationAfter, we can assume it's a simple path
                        _pathCompleted = true;
                        return false; // No more points to consume
                    }
                    
                }
            }
            return true; // Continue executing the current movement step
        }
    }
}
