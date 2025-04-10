using BotSimZero.World.Terrain;
using Stride.Core;
using Stride.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotSimZero.World
{
    [DataContract(nameof(DoomedComponent))]
    public class DoomedComponent : EntityComponent
    {
        public DateTime ExpirationTime; // Store expiration time directly
        protected bool isDead = false;

        private DoomedComponent(long lifetimeMillis) // Changed parameter type to long
        {
            ExpirationTime = DateTime.UtcNow.AddMilliseconds(lifetimeMillis); // Calculate expiration time at creation
        }
        public DoomedComponent() { }

        public bool IsExpired()
        {
            return DateTime.UtcNow >= ExpirationTime; // Compare current time with expiration time
        }

        public void Die()
        {
            if (isDead)
            {
                string name = this.Entity.Name;
                throw new InvalidOperationException($"Doomed entity {name} killed after being already dead");
            }
            isDead = true;
            Entity.Transform.Parent = null; // the death of the entity
        }

        public static DoomedComponent CreateAndRegister(long lifetimeMillis, DoomedObjectTracker doomedTracker) // Changed parameter type to long
        {
            var doomedComponent = new DoomedComponent(lifetimeMillis);
            doomedTracker.Register(doomedComponent);
            return doomedComponent;
        }
    }

    public static class EntityExtensions
    {
        public static void Doom(this Entity entity, int lifetimeMillis, DoomedObjectTracker doomedTracker)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            if (doomedTracker == null) throw new ArgumentNullException(nameof(doomedTracker));

            // Create and register a DoomedComponent
            var doomedComponent = DoomedComponent.CreateAndRegister(lifetimeMillis, doomedTracker);

            // Attach the DoomedComponent to the entity
            entity.Components.Add(doomedComponent);
        }
    }
}
