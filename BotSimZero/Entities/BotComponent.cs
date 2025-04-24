using BotSimZero.Core;
using SimuliEngine.Simulation.ActorSystem;
using Stride.Engine;

namespace BotSimZero.Entities
{
    public class BotComponent: StartupScript, IMovingEntity
    {
        public Actor Actor { get; private set; }
        public (int x, int y) SpawnPosition { get; set; }
        public void Tick()
        {
            
        }
        public class Processor : IMovingEntity.Processor
        {
            
        }

        public void Initialize()
        {
            var controller = GlobalController.FindGlobalWorldController(this);
            Actor = new MovingActor(controller.WorldState, new BotRealPositionProvider(Entity));
        }
    }
}
