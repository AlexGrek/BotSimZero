using SimuliEngine.Simulation.ActorSystem.Pathfinding;
using SimuliEngine.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimuliEngine.Simulation.ActorSystem.TaskingSystem
{
    internal class FindAndGoToStaticPointLogicalTask : GoToPointLogicalTask
    {
        protected Predicate<(int x, int y)> _predicate;
        protected float _max;

        public FindAndGoToStaticPointLogicalTask(Predicate<(int x, int y)> predicate, float max, object createdBy) : base((0, 0), createdBy)
        {
            _predicate = predicate;
            _max = max;
        }

        public override void OnTaskStart<T>(T actor, WorldState world)
        {
            var point = MovingActor.PathfindHelper.FindClosestPointOf(actor, world, _predicate, _max);
            base.OnTaskStart(actor, world);
        }
    }
}
