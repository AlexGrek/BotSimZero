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
        public class FollowerWithInertia : SyncScript
        {
            public Entity FollowsEntity { get; set; } // The entity to follow
            public float MovementInertia { get; set; } = 10f; // Inertia for movement
            public float RotationInertia { get; set; } = 0.1f; // Inertia for rotation

            public float BaseSpeed { get; set; } = 20.0f; // Base speed in units per second
            public float AccelerationRate { get; set; } = 0.5f; // How quickly speed increases with distance
            public float DampingFactor { get; set; } = 0.8f; // How quickly to dampen velocity

            public bool SlerpMovement { get; set; } = false;

            private Vector3 velocity = Vector3.Zero; // For smooth movement
            private Vector3 currentVelocity = Vector3.Zero;

            public override void Start()
            {
                if (FollowsEntity == null)
                    FindSomethingToFollow();
                if (FollowsEntity == null)
                    return;

                // Initialize position to match the target entity
                Entity.Transform.Position = FollowsEntity.Transform.Position;
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
                    FindSomethingToFollow();
                if (FollowsEntity == null)
                    return;

                // Get the target position and rotation
                var targetPosition = FollowsEntity.Transform.WorldMatrix.TranslationVector;
                var targetRotation = Utils.ExtractRotation(FollowsEntity.Transform.WorldMatrix);

                // Smoothly interpolate position using MovementInertia
                var currentPosition = Entity.Transform.Position;
                float deltaTime = (float)Game.UpdateTime.Elapsed.TotalSeconds;
                if (SlerpMovement)
                    Entity.Transform.Position = Utils.SimpleLerp(deltaTime, currentPosition, targetPosition, MovementInertia);
                else
                {
                    Entity.Transform.Position = Utils.ResponsiveFollow(
                    deltaTime,
                    currentPosition,
                    targetPosition,
                    ref currentVelocity,
                    BaseSpeed,
                    AccelerationRate,
                    DampingFactor);
                }



                // Smoothly interpolate rotation using RotationInertia
                var currentRotation = Entity.Transform.Rotation;
                Entity.Transform.Rotation = Quaternion.Slerp(currentRotation, targetRotation, deltaTime / RotationInertia);

                // Debug output for tracking movement
                DebugText.Print($"Moving towards {FollowsEntity.Transform.Position}", new Int2(100, 100));
                DebugText.Print($"from {Entity.Transform.Position} ", new Int2(100, 125));
                DebugText.Print($"with speed {velocity}", new Int2(100, 150));
            }
        }
    }

}
