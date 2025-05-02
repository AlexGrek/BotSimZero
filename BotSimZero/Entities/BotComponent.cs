using BotSimZero.Core;
using SimuliEngine.Simulation;
using SimuliEngine.Simulation.ActorSystem;
using SimuliEngine.Simulation.ActorSystem.Bots;
using Stride.Engine;

namespace BotSimZero.Entities
{
    public class BotComponent : StartupScript, IMovingEntity
    {
        public MovingActor Actor { get; private set; }
        public (int x, int y) SpawnPosition { get; set; }

        Actor IMovingEntity.Actor => Actor;

        public void Tick()
        {
        }

        public class Processor : IMovingEntity.Processor
        {
        }

        public void Initialize()
        {
            var controller = GlobalController.FindGlobalWorldController(this);

            // Fix for CS0019: Convert Guid to a hash code (int) before applying the modulus operator
            IRealRotationProvider rot = Entity.Id.GetHashCode() % 2 == 0
                ? new RealRotationProviderClaude(Entity)
                : new RealRotationProviderGemini(Entity);

            Actor = new BotActor(controller.WorldState, new BotRealPositionProvider(Entity), new RealRotationProviderClaude(Entity));
            Actor.AddActorComponent(new RunsOnBatteries());
            Actor.Intellect.AddBehavior(new BotBehavior());
            Actor.Name = Entity.Name;
        }
    }
}
