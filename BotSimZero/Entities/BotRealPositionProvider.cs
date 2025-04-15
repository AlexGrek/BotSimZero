using SimuliEngine.Simulation;
using Stride.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

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
            var realPosition = new Vector2(Entity.Transform.Position.X, Entity.Transform.Position.Y);
            var relativePosition = realPosition - center;
            return relativePosition;
        }

        public void Move(float s, Vector2 direction)
        {
            var RealPosition = new Vector2(Entity.Transform.Position.X, Entity.Transform.Position.Y);
            var newPosition = RealPosition + direction * s;
            Entity.Transform.Position = new Vector3(newPosition.X, Entity.Transform.Position.Y, newPosition.Y);
        }

        public Vector2 GetNormalizedPosition()
        {
            return new Vector2(Entity.Transform.Position.X, Entity.Transform.Position.Y);
        }
    }
}
