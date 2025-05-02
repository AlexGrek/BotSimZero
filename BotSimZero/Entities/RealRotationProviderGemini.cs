using SimuliEngine.Simulation;
using Stride.Core.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotSimZero.Entities
{
    public class RealRotationProviderGemini: IRealRotationProvider
    {
        private Stride.Engine.Entity trackedEntity;
        private Quaternion currentRotation;
        private readonly float snapThresholdAngleCos;

        public RealRotationProviderGemini(Stride.Engine.Entity entity, float snapAngleDegrees = 5f)
        {
            trackedEntity = entity;
            currentRotation = entity.Transform.Rotation;
            snapThresholdAngleCos = (float)Math.Cos(MathUtil.DegreesToRadians(snapAngleDegrees));
        }

        // Implementation for Stride.Core.Mathematics.Vector2
        public void LookAtSmooth(float turnStep, Vector2 target)
        {
            Vector2 currentDirection = GetRotationDirection();
            Vector2 objectPosition2D = new Vector2(trackedEntity.Transform.Position.X, trackedEntity.Transform.Position.Z);
            Vector2 targetDirection = Vector2.Normalize(target - objectPosition2D);

            float dotProduct = Vector2.Dot(currentDirection, targetDirection);
            if (Math.Abs(dotProduct) > 1f)
                dotProduct = Math.Sign(dotProduct);

            float angle = (float)Math.Acos(dotProduct);
            float crossProductZ = currentDirection.X * targetDirection.Y - currentDirection.Y * targetDirection.X;
            float sign = Math.Sign(crossProductZ);

            float actualTurn = Math.Min(turnStep, Math.Abs(angle));
            Quaternion rotationDelta = Quaternion.RotationZ(sign * actualTurn);
            currentRotation = rotationDelta * currentRotation;
            currentRotation.Normalize();
            trackedEntity.Transform.Rotation = currentRotation;
        }

        public bool IsFacing(Vector2 target)
        {
            Vector2 currentDirection = GetRotationDirection();
            Vector2 objectPosition2D = new Vector2(trackedEntity.Transform.Position.X, trackedEntity.Transform.Position.Z);
            Vector2 targetDirection = (target - objectPosition2D);
            targetDirection.Normalize();

            float dotProduct = Vector2.Dot(currentDirection, targetDirection);
            return dotProduct >= snapThresholdAngleCos;
        }

        public Vector2 GetRotationDirection()
        {
            Vector3 forwardVector3 = Vector3.UnitX;
            Vector3 rotatedVector3 = Vector3.Transform(forwardVector3, currentRotation);
            // With this corrected line:
            return Vector2.Normalize(new Vector2(rotatedVector3.X, rotatedVector3.Y));
        }

        public void LookAtImmediately(Vector2 target)
        {
            Vector2 objectPosition2D = new Vector2(trackedEntity.Transform.Position.X, trackedEntity.Transform.Position.Z);
            Vector2 targetDirection = Vector2.Normalize(target - objectPosition2D);
            float angleRadians = (float)Math.Atan2(targetDirection.Y, targetDirection.X);
            currentRotation = Quaternion.RotationZ(angleRadians);
            trackedEntity.Transform.Rotation = currentRotation;
        }

        // Implementation for System.Numerics.Vector2
        public void LookAtSmooth(float turnStep, System.Numerics.Vector2 target)
        {
            LookAtSmooth(turnStep, target.ToStrideVector());
        }

        public bool IsFacing(System.Numerics.Vector2 target)
        {
            return IsFacing(target.ToStrideVector());
        }

        public void LookAtImmediately(System.Numerics.Vector2 target)
        {
            LookAtImmediately(target.ToStrideVector());
        }

        System.Numerics.Vector2 IRealRotationProvider.GetRotationDirection()
        {
            return GetRotationDirection();
        }
    }

    public static class Vector2Extensions
    {
        public static Vector2 ToStrideVector(this System.Numerics.Vector2 vector)
        {
            return new Vector2(vector.X, vector.Y);
        }

        public static System.Numerics.Vector2 ToSystemVector(this Vector2 vector)
        {
            return new System.Numerics.Vector2(vector.X, vector.Y);
        }
    }
}
