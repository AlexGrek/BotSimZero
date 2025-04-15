using BotSimZero.Core;
using SimuliEngine.Simulation.Actor;
using Stride.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            Actor = new StaticActor(controller.WorldState, new BotRealPositionProvider(Entity));
        }
    }
}
