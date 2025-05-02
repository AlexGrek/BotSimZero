using SimuliEngine;
using SimuliEngine.Simulation;
using Stride.Engine;
using System;
using System.Numerics;

namespace BotSimZero.Entities
{
    public class BotRealRotationProvider : IRealRotationProvider
    {
        public Entity Entity { get; set; }

        public BotRealRotationProvider(Entity entity)
        {
            Entity = entity;
        }

        public void LookAtSmooth(float turnStep, Vector2 target)
        {
            if (target == Vector2.Zero)
                return;

            target = Vector2.Normalize(target);
            var currentRotation = GetRotationDirection();

            // Check if already facing the target direction
            if (IsFacing(target))
                return;

            // Calculate the current and target angles in the 2D plane
            var currentAngle = MathF.Atan2(currentRotation.Y, currentRotation.X);
            var targetAngle = MathF.Atan2(target.Y, target.X);

            // Interpolate between the angles
            var interpolatedAngle = Core.Utils.Lerp(currentAngle, targetAngle, turnStep);

            // Convert the interpolated angle back to a direction vector
            var interpolatedDirection = new Vector2(MathF.Cos(interpolatedAngle), MathF.Sin(interpolatedAngle));

            SetRotationDirection(interpolatedDirection);
        }

        public bool IsFacing(Vector2 target)
        {
            if (target == Vector2.Zero)
                return false;

            target = Vector2.Normalize(target);
            var currentRotation = GetRotationDirection();

            // Check if the angle between the current and target directions is small enough
            return Vector2.Dot(currentRotation, target) > 0.999f; // Close enough
        }

        public Vector2 GetRotationDirection()
        {
            // Extract the forward direction from the entity's rotation quaternion
            var forward = Entity.Transform.WorldMatrix.Forward;
            return Vector2.Normalize(new Vector2(forward.X, forward.Z));
        }

        public void LookAtImmediately(Vector2 target)
        {
            if (target == Vector2.Zero)
                return;

            target = Vector2.Normalize(target);
            SetRotationDirection(target);
        }

        private void SetRotationDirection(Vector2 newRotation)
        {
            if (newRotation == Vector2.Zero)
                return;

            newRotation = Vector2.Normalize(newRotation);
            var angle = MathF.Atan2(newRotation.Y, newRotation.X);
            Entity.Transform.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, angle);
        }
    }
}
