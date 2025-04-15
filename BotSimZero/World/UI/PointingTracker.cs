using BotSimZero.Core;
using BotSimZero.World.Terrain;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Graphics;
using Stride.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace BotSimZero.World.UI
{
    internal class PointingTracker: WorldAwareSyncScript
    {
        private Simulation simulation;
        private CameraComponent camera;
        public Entity PointingAt = null;

        public override void Start()
        {
            base.Start();
            camera = Entity.Scene.Entities.FirstOrDefault(e => e.Get<CameraComponent>() != null)?.Get<CameraComponent>();
            simulation = this.GetSimulation();
        }

        private HitResult? Raycast(Vector2 screenPos)
        {
            // Make sure we have camera and simulation
            if (camera == null || simulation == null)
                return null;

            // Get the back buffer dimensions
            var backBuffer = GraphicsDevice.Presenter.BackBuffer;
            screenPos.X *= backBuffer.Width;
            screenPos.Y *= backBuffer.Height;

            // Create viewport matching the back buffer
            var viewport = new Viewport(0, 0, backBuffer.Width, backBuffer.Height);

            // Unproject the screen position to 3D world coordinates at near plane
            var unprojectedNear = viewport.Unproject(
            new Vector3(screenPos, 0.0f),
            camera.ProjectionMatrix,
                camera.ViewMatrix,
                Matrix.Identity);

            // Unproject the screen position to 3D world coordinates at far plane
            var unprojectedFar = viewport.Unproject(
                new Vector3(screenPos, 1.0f),
                camera.ProjectionMatrix,
                camera.ViewMatrix,
                Matrix.Identity);

            // Perform the raycast from near to far point
            var result = simulation.Raycast(
                unprojectedNear,
                unprojectedFar,
                filterFlags: CollisionFilterGroupFlags.AllFilter);

            return result;
        }

        public override void Update()
        {
            if (Raycast(Input.MousePosition) is HitResult result)
            {
                // If we hit something, process it
                if (result.Succeeded && result.Collider != null)
                {
                    var hitEntity = result.Collider.Entity;
                    RegisterEntity(hitEntity); // call events even if pointer is pointing to nowhere
                    if (hitEntity == null)
                        return;
                    // we hit some entity, save it and call corresponding events
                    if (this.IsClicked())
                    {
                        GlobalEvents.PointerClickEventKey.Broadcast(hitEntity);
                        DebugText.Print($"PointerClickEvent fired for {hitEntity.Name}", new Int2(300, 125));
                    }
                }
                
            }
        }

        private void RegisterEntity(Entity hitEntity)
        {
            var prev = GlobalWorldController.PointingAtEntity;
            GlobalWorldController.PointingAtEntity = hitEntity;
            // swap them before calling events
            if (prev != null)
            {
                // Unhighlight the previous entity
                GlobalEvents.PointerExitEventKey.Broadcast(prev);
            }
            if (hitEntity != null)
            {
                // Highlight the new entity
                GlobalEvents.PointerEnterEventKey.Broadcast(hitEntity);
            }
        }
    }
}
