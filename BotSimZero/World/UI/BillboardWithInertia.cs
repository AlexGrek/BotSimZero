using BotSimZero.Core;
using Stride.Core.Mathematics;
using Stride.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotSimZero.World.UI
{
    public class BillboardWithInertia: SyncScript
    {
        public Entity FollowsEntity { get; set; } // The entity to follow
        public float RotationInertia { get; set; } = 0.1f; // Inertia for rotation

        public float ScaleByDistanceFactor { get; set; } = 1f; // Scale factor based on distance

        public override void Start()
        {
            if (FollowsEntity == null)
                FindSomethingToFollow();
            if (FollowsEntity == null)
                return;
            // Initialize position to match the target entity
        }
        private void FindSomethingToFollow()
        {
            // Find an entity with a BotComponent to follow
            FollowsEntity = this.Entity.Scene.Entities
                .Where(e => e.Get<CameraComponent>() != null)
                .FirstOrDefault();
        }
        public override void Update()
        {
            if (FollowsEntity == null)
                FindSomethingToFollow();
            if (FollowsEntity == null)
                return;

            var targetRotation = Utils.ExtractRotation(FollowsEntity.Transform.WorldMatrix);
            if (ScaleByDistanceFactor > 0)
            {
                // Calculate the distance to the target entity
                var distance = Vector3.Distance(Entity.Transform.Position, FollowsEntity.Transform.Position);
                // Scale the billboard based on the distance
                Entity.Transform.Scale = new Vector3(ScaleByDistanceFactor * distance);
            }
            float deltaTime = (float)Game.UpdateTime.Elapsed.TotalSeconds;
            // Smoothly interpolate rotation using RotationInertia
            var currentRotation = Entity.Transform.Rotation;
            Entity.Transform.Rotation = Quaternion.Slerp(
                currentRotation,
                targetRotation,
                deltaTime / RotationInertia);
        }
    }
}
