using SimuliEngine.Simulation;
using Stride.Core.Mathematics;
using Stride.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotSimZero.Entities
{
    public class RealRotationProviderClaude: IRealRotationProvider
    {
        private readonly Entity entity;
        private readonly float snapThreshold; // Angle in radians below which we snap to target

        public RealRotationProviderClaude(Entity entity, float snapThresholdDegrees = 8.0f)
        {
            this.entity = entity;
            snapThreshold = MathUtil.DegreesToRadians(snapThresholdDegrees);
        }

        // Convert from System.Numerics.Vector2 to Stride.Core.Mathematics.Vector2
        private Stride.Core.Mathematics.Vector2 ToStrideVector2(System.Numerics.Vector2 vector)
        {
            return new Stride.Core.Mathematics.Vector2(vector.X, vector.Y);
        }

        // Convert from Stride.Core.Mathematics.Vector2 to System.Numerics.Vector2
        private System.Numerics.Vector2 ToSystemVector2(Stride.Core.Mathematics.Vector2 vector)
        {
            return new System.Numerics.Vector2(vector.X, vector.Y);
        }

        public void LookAtSmooth(float turnStep, System.Numerics.Vector2 target)
        {
            // Convert target to Stride vector
            Stride.Core.Mathematics.Vector2 strideTarget = ToStrideVector2(target);

            // Get the current forward direction in the XZ plane
            Stride.Core.Mathematics.Vector2 currentDirection = GetStrideRotationDirection();

            // Calculate direction to target (in XZ plane)
            Stride.Core.Mathematics.Vector2 currentPosition2D = new Stride.Core.Mathematics.Vector2(
                entity.Transform.Position.X,
                entity.Transform.Position.Z
            );
            Stride.Core.Mathematics.Vector2 toTarget = strideTarget - currentPosition2D;

            if (toTarget.LengthSquared() < 0.001f)
                return; // We're at the target position, no need to rotate

            toTarget.Normalize();

            // Calculate angle between current direction and target direction
            float dot = Stride.Core.Mathematics.Vector2.Dot(currentDirection, toTarget);
            float angle = (float)Math.Acos(MathUtil.Clamp(dot, -1.0f, 1.0f));

            // Determine rotation direction (clockwise or counter-clockwise)
            float cross = currentDirection.X * toTarget.Y - currentDirection.Y * toTarget.X;
            float rotationSign = Math.Sign(cross);

            // If angle is smaller than threshold, snap directly to target rotation
            if (angle < snapThreshold)
            {
                LookAtImmediately(target);
                return;
            }

            // Otherwise rotate by turnStep towards target
            float rotationAmount = Math.Min(turnStep, angle);

            // Create rotation quaternion for this frame's rotation
            Quaternion rotationDelta = Quaternion.RotationAxis(
                Vector3.UnitY,
                rotationSign * rotationAmount
            );

            // Apply rotation
            entity.Transform.Rotation = Quaternion.Multiply(rotationDelta, entity.Transform.Rotation);
            entity.Transform.Rotation.Normalize(); // Ensure quaternion stays normalized
        }

        public bool IsFacing(System.Numerics.Vector2 target)
        {
            // Convert target to Stride vector
            Stride.Core.Mathematics.Vector2 strideTarget = ToStrideVector2(target);

            // Get current direction
            Stride.Core.Mathematics.Vector2 currentDirection = GetStrideRotationDirection();

            // Calculate direction to target
            Stride.Core.Mathematics.Vector2 currentPosition2D = new Stride.Core.Mathematics.Vector2(
                entity.Transform.Position.X,
                entity.Transform.Position.Z
            );
            Stride.Core.Mathematics.Vector2 toTarget = strideTarget - currentPosition2D;

            if (toTarget.LengthSquared() < 0.001f)
                return true; // We're at the target position

            toTarget.Normalize();

            // Calculate angle between current direction and target direction
            float dot = Stride.Core.Mathematics.Vector2.Dot(currentDirection, toTarget);
            float angle = (float)Math.Acos(MathUtil.Clamp(dot, -1.0f, 1.0f));

            // Return true if angle is less than threshold
            return angle < snapThreshold;
        }

        // Internal method that returns a Stride Vector2
        private Stride.Core.Mathematics.Vector2 GetStrideRotationDirection()
        {
            // Forward vector in Stride is typically +Z, transform it using the current rotation
            Vector3 forward = Vector3.UnitZ;
            forward = Vector3.Transform(forward, entity.Transform.Rotation);

            // Extract X and Z components for the 2D direction
            Stride.Core.Mathematics.Vector2 direction = new Stride.Core.Mathematics.Vector2(forward.X, forward.Z);

            // Make sure it's normalized
            if (direction.LengthSquared() > 0)
                direction.Normalize();

            return direction;
        }

        public System.Numerics.Vector2 GetRotationDirection()
        {
            // Get the Stride Vector2 direction and convert to System.Numerics.Vector2
            return ToSystemVector2(GetStrideRotationDirection());
        }

        public void SetRotationByNormalVector(System.Numerics.Vector2 normal)
        {
            // Convert normal to Stride vector  
            Stride.Core.Mathematics.Vector3 strideNormal = new Stride.Core.Mathematics.Vector3(normal.X, 0, normal.Y);

            // Calculate the forward direction (normalized)  
            strideNormal.Normalize();

            // Calculate the quaternion rotation using BetweenDirections  
            Quaternion rotation = Quaternion.BetweenDirections(Vector3.UnitZ, strideNormal);

            // Set the entity's rotation  
            entity.Transform.Rotation = rotation;
        }

        public void LookAtImmediately(System.Numerics.Vector2 target)
        {
            // Convert target to Stride vector
            Stride.Core.Mathematics.Vector2 strideTarget = ToStrideVector2(target);

            // Calculate direction to target in XZ plane
            Stride.Core.Mathematics.Vector2 currentPosition2D = new Stride.Core.Mathematics.Vector2(
                entity.Transform.Position.X,
                entity.Transform.Position.Z
            );
            Stride.Core.Mathematics.Vector2 toTarget = strideTarget - currentPosition2D;

            if (toTarget.LengthSquared() < 0.001f)
                return; // We're at the target position, no need to rotate

            toTarget.Normalize();

            // Calculate the target rotation (converting 2D direction to angle around Y axis)
            float angle = (float)Math.Atan2(toTarget.X, toTarget.Y);

            // Create quaternion for the new rotation
            entity.Transform.Rotation = Quaternion.RotationY(angle);
        }
    }
}
