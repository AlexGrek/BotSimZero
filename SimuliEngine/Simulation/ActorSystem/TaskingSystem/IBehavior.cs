using SimuliEngine.Basic;
using SimuliEngine.Simulation.Obstacles;
using SimuliEngine.World;

namespace SimuliEngine.Simulation.ActorSystem.TaskingSystem
{
    public interface IBehavior<T>: IDumpable
    {
        public void OnBehaviorAdd(T actor, WorldState world)
        {
            // Called after the behavior is just added to the actor
        }

        public void OnBehaviorRemove(T actor, WorldState world)
        {
            // Called after the behavior is removed from the actor
        }

        bool RecoverFromInterruption(T actor, WorldState state, IObstacle failedToMove) { return false; }

        public void Update(float deltaTime, T actor, WorldState world)
        {
            // Called every frame
        }
    }
}
