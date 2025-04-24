using SimuliEngine.Simulation;
using Stride.Engine;
using System.Numerics;

namespace BotSimZero.Entities
{
    public class BotRealPositionProvider : IRealPositionProvider
    {
        public Entity Entity {get; set;}

        public BotRealPositionProvider(Entity entity)
        {
            Entity = entity;
        }

        public Vector2 GetRelativePosition(Vector2 center)
        {
            var realPosition = new Vector2(Entity.Transform.Position.X, Entity.Transform.Position.Z);
            var relativePosition = realPosition - center;
            return relativePosition;
        }

        public void Move(float s, Vector2 direction)
        {
            var RealPosition = new Vector2(Entity.Transform.Position.X, Entity.Transform.Position.Z);
            var newPosition = RealPosition + direction * s;
            Entity.Transform.Position = new Vector3(newPosition.X, Entity.Transform.Position.Y, newPosition.Y);
        }

        public Vector2 GetWorldCoordinates()
        {
            return new Vector2(Entity.Transform.Position.X, Entity.Transform.Position.Z);
        }

        public void SetWorldCoordinates(Vector2 newPosition)
        {
            Entity.Transform.Position = new Vector3(newPosition.X, Entity.Transform.Position.Y, newPosition.Y);
        }
    }
}
