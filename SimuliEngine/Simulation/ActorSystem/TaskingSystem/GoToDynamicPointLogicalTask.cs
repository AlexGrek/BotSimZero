using SimuliEngine.Simulation.ActorSystem.Pathfinding;
using SimuliEngine.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimuliEngine.Simulation.ActorSystem.TaskingSystem
{
    public class GoToDynamicPointLogicalTask : LogicalTask
    {
        public (int x, int y) Point { get; protected set; }
        public IPointProvider PointProvider { get; protected set; }

        private LowLevelTask? _activeLowLevelTask;

        public override LowLevelTask? ActiveLowLevelTask { get => _activeLowLevelTask; protected set => _activeLowLevelTask = value; }

        public GoToDynamicPointLogicalTask(IPointProvider provider, object createdBy) : base(createdBy)
        {
            PointProvider = provider;
        }

        public override void OnTaskStart<T>(T actor, WorldState world)
        {
            Point = PointProvider.GetPoint();
            var path = MovingActor.PathfindHelper.AStar(actor, Point, world);
            if (path == null && actor.MainPosition != Point)
            {
                Cancel();
                return;
            }
            if (path == null)
            {
                this.Finish();
                return;
            }
            ActiveLowLevelTask = new FollowPathLowLevelTask(path, actor, world, this);
        }

        public override void ExecuteTask<T>(float deltaTime, T actor, WorldState state)
        {
            var newPoint = PointProvider.GetPoint();
            if (Point != newPoint)
            {
                Point = newPoint;
                var path = MovingActor.PathfindHelper.AStar(actor, Point, state);
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
                ActiveLowLevelTask = new FollowPathLowLevelTask(path, actor, state, this);
            }
            if (ActiveLowLevelTask == null)
            {
                throw new InvalidOperationException("ActiveLowLevelTask is null. This can only happen if OnTaskStart was never called.");
            }
        }
    }
}
