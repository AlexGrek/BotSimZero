using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotSimZero.VirtualUI
{
    using Stride.Engine;
    using Stride.Core.Mathematics;
    using global::BotSimZero.Core;
    using global::BotSimZero.Entities;

    namespace BotSimZero.Camera
    {
        public class FollowerWithInertiaLimited : SyncScript
        {
            // The entity to follow (the camera)
            public Entity FollowsEntity { get; set; }

            // Maximum allowed distance from the camera
            public float MaxDistance { get; set; } = 10.0f;

            // How quickly to move when within allowed distance
            public float CatchupSpeed { get; set; } = 40.0f;

            // How quickly rotation follows the camera
            public float RotationSpeed { get; set; } = 10.0f;

            // Smoothing factor (lower = smoother but slower)
            public float SmoothingFactor { get; set; } = 0.85f;

            public float DeadZone { get; set; } = 0.5f;

            public float VelocityDamping { get; set; } = 0.9f;

            private Vector3 currentVelocity = Vector3.Zero;
            private bool isInitialized = false;
            private bool isWithinDeadZone = false;

            public override void Start()
            {
                if (FollowsEntity == null)
                    FindSomethingToFollow();
            }

            private void FindSomethingToFollow()
            {
                // Find an entity with a BotComponent to follow
                FollowsEntity = this.Entity.Scene.Entities
                    .Where(e => e.Get<BotComponent>() != null)
                    .FirstOrDefault();
            }

            public override void Update()
            {
                if (FollowsEntity == null)
                {
                    FindSomethingToFollow();
                    if (FollowsEntity == null)
                        return;
                }

                // Initialize position if not done yet
                if (!isInitialized)
                {
                    Entity.Transform.Position = FollowsEntity.Transform.Position;
                    Entity.Transform.Rotation = Utils.ExtractRotation(FollowsEntity.Transform.WorldMatrix);
                    isInitialized = true;
                    return;
                }

                // Get the target position and rotation
                var targetPosition = FollowsEntity.Transform.WorldMatrix.TranslationVector;
                var targetRotation = Utils.ExtractRotation(FollowsEntity.Transform.WorldMatrix);

                // Calculate current position and distance to target
                var currentPosition = Entity.Transform.Position;
                float deltaTime = (float)Game.UpdateTime.Elapsed.TotalSeconds;

                // Calculate vector to target and distance
                Vector3 toTarget = targetPosition - currentPosition;
                float distance = toTarget.Length();

                // Check if we're within the dead zone
                bool wasInDeadZone = isWithinDeadZone;
                isWithinDeadZone = distance <= DeadZone;

                // If we just entered the dead zone, match the position exactly
                if (isWithinDeadZone && !wasInDeadZone)
                {
                    Entity.Transform.Position = targetPosition;
                    currentVelocity = Vector3.Zero;
                }
                // If we're outside the dead zone but within max distance
                else if (!isWithinDeadZone && distance <= MaxDistance)
                {
                    // Calculate target velocity based on distance and direction
                    Vector3 direction = distance > 0.001f ? toTarget / distance : Vector3.Zero;

                    // Scale speed based on distance - slower when closer to target
                    float speedFactor = Math.Min(1.0f, distance / (MaxDistance * 0.5f));
                    Vector3 targetVelocity = direction * CatchupSpeed * speedFactor;

                    // Apply stronger smoothing when getting close to the target
                    float dynamicSmoothingFactor = SmoothingFactor;
                    if (distance < MaxDistance * 0.2f)
                    {
                        // Increase smoothing when close to reduce oscillation
                        dynamicSmoothingFactor = Utils.Lerp(SmoothingFactor, 0.95f, 1.0f - distance / (MaxDistance * 0.2f));
                    }

                    // Smoothly transition current velocity
                    currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, 1.0f - dynamicSmoothingFactor);

                    // Apply velocity to position
                    Entity.Transform.Position += currentVelocity * deltaTime;

                    // Dampen velocity when close to target to prevent oscillation
                    if (distance < MaxDistance * 0.3f)
                    {
                        currentVelocity *= VelocityDamping;
                    }
                }
                // If we're beyond max distance, teleport
                else if (distance > MaxDistance)
                {
                    // Calculate new position at maximum allowed distance
                    Vector3 direction = toTarget / distance;
                    Vector3 newPosition = targetPosition - direction * MaxDistance * 0.9f;

                    // Set position and reset velocity
                    Entity.Transform.Position = newPosition;
                    currentVelocity = Vector3.Zero;
                }

                // Smoothly interpolate rotation with dead zone consideration
                var currentRotation = Entity.Transform.Rotation;
                float rotationLerpFactor;

                // If we're in the dead zone, snap rotation
                if (isWithinDeadZone)
                {
                    rotationLerpFactor = 1.0f;
                }
                else
                {
                    rotationLerpFactor = Math.Min(1.0f, deltaTime * RotationSpeed);
                }

                Entity.Transform.Rotation = Quaternion.Slerp(currentRotation, targetRotation, rotationLerpFactor);

                // Debug output
                string zoneStatus = isWithinDeadZone ? "IN DEAD ZONE" : "FOLLOWING";
                //DebugText.Print($"Distance: {distance:F2}, Speed: {currentVelocity.Length():F2}, Status: {zoneStatus}", new Int2(300, 125));
            }
        }
    }

}
