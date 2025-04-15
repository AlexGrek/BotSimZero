using Stride.Core;
using Stride.Engine.FlexibleProcessing;
using Stride.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stride.Engine.Design;
using Stride.Games;
using SimuliEngine.Simulation.ActorSystem;
using System.Collections.Concurrent;

namespace BotSimZero.Entities
{
    public interface IMovingEntity : IComponent<IMovingEntity.Processor, IMovingEntity> {
        void Tick();
        Actor Actor { get; }
        public (int x, int y) SpawnPosition { get; set; }

        public class Processor : IProcessor, IUpdateProcessor
        {
            public List<IMovingEntity> Components = new();
            public ConcurrentQueue<IMovingEntity> ComponentsToAdd = new();
            public ConcurrentQueue<IMovingEntity> ComponentsToRemove = new();

            public void SystemAdded(IServiceRegistry registryParam) { }
            public void SystemRemoved() { }

            public void OnComponentAdded(IMovingEntity item) {
                ComponentsToAdd.Enqueue(item);
                    }
            public void OnComponentRemoved(IMovingEntity item)
            {
                ComponentsToRemove.Enqueue(item);
            }

            // The execution order of this Update, smaller values execute first compared to other IComponent Processors
            public int Order => 0;

            public void Update(GameTime gameTime)
            {
                // add all components
                while(ComponentsToAdd.TryDequeue(out var movingEntity))
                {
                    Components.Add(movingEntity);
                    movingEntity.Initialize();
                    movingEntity.Actor.Instantiate(movingEntity.SpawnPosition.x, movingEntity.SpawnPosition.y);
                }
                // remove all components
                while (ComponentsToRemove.TryDequeue(out var movingEntity))
                {
                    Components.Remove(movingEntity);
                    movingEntity.Actor.RemoveInstance();
                }
                foreach (var comp in Components)
                    comp.Tick();
            }
        }

        void Initialize();
    }
}
