using SimuliEngine.Simulation.ActorSystem.Pathfinding;
using SimuliEngine.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimuliEngine.Simulation.ActorSystem.TaskingSystem
{
    public class GoToPointLogicalTask : LogicalTask
    {
        public (int x, int y) Point { get; protected set; }

        private LowLevelTask? _activeLowLevelTask;

        public override LowLevelTask? ActiveLowLevelTask { get => _activeLowLevelTask; protected set => _activeLowLevelTask = value; }

        public GoToPointLogicalTask((int x, int y) point, object createdBy) : base(createdBy)
        {
            Point = point;
        }

        public override void OnTaskStart<T>(T actor, WorldState world)
        {
            var path = MovingActor.PathfindHelper.AStar(actor, Point, world);
            if (path == null && actor.MainPosition != Point)
            {
                Cancel();
                return;
            }
            if (path == null)
            {
                Finish();
                return;
            }
            ActiveLowLevelTask = new FollowPathLowLevelTask(path, actor, world, this);
        }

        public override void ExecuteTask<T>(float deltaTime, T actor, WorldState state)
        {
            if (ActiveLowLevelTask == null)
            {
                throw new InvalidOperationException("ActiveLowLevelTask is null. This can only happen if OnTaskStart was never called.");
            }
        }
    }
}
