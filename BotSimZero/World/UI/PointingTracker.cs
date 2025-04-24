using BotSimZero.Core;
using BotSimZero.World.Terrain;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Graphics;
using Stride.Input;
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
        private CameraComponent _camera;
        public Entity PointingAt = null;
        private Entity _rayVisualizer = null;

        protected CameraComponent GetCamera()
        {
            if (_camera == null || simulation == null)
            {
                _camera = Entity.Scene.Entities.FirstOrDefault(e => e.Get<CameraComponent>() != null)?.Get<CameraComponent>();
                simulation = this.GetSimulation();
            }
            return _camera;
;        }

        public override void Start()
        {
            base.Start();
            _rayVisualizer = new Entity(Entity.Name + "_RayVisualizer");
            var modelComponent = new ModelComponent
            {
                Model = ModelFactory.CreateCyl(GraphicsDevice, 1000, 0.1f)
            };
        }

        private HitResult? Raycast(Vector2 screenPos)
        {
            var camera = GetCamera();
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
                new Vector3(screenPos, 1f),
                camera.ProjectionMatrix,
                camera.ViewMatrix,
                Matrix.Identity);

            // Perform the raycast from near to far point
            var result = simulation.Raycast(
                unprojectedNear,
                unprojectedFar,
                filterGroup: CollisionFilterGroups.AllFilter,
                filterFlags: CollisionFilterGroupFlags.AllFilter);

            if (this.IsClicked())
            {
                // if we clicked on empty space, call the event
                //VisualizeRay(unprojectedNear, unprojectedFar);
            }

            return result;
        }

        public void DebugShot()
        {
            // Check if the space key is pressed
            if (Input.IsKeyPressed(Keys.Space))
            {
                // Iterate through all entities in the scene
                foreach (var entity in Entity.Scene.Entities)
                {
                    // Check if the entity has a StaticColliderComponent
                    var collider = entity.Get<StaticColliderComponent>();
                    if (collider != null)
                    {
                        // Iterate through all collider shapes in the component
                        foreach (var shape in collider.ColliderShapes)
                        {
                            if (shape is BoxColliderShapeDesc boxShape)
                            {
                                // Get the size and position of the collider
                                var size = boxShape.Size;
                                var position = entity.Transform.WorldMatrix.TranslationVector;

                                // Create a cube to visualize the collider
                                var cubeEntity = new Entity($"Collider_{entity.Name}", position: position, scale: size);
                                var modelComponent = new ModelComponent
                                {
                                    Model = ModelFactory.CreateCube(GraphicsDevice)
                                };

                                // Assign a material to the cube
                                var material = MaterialFactory.CreateSimpleMaterial(GraphicsDevice, Color.Blue);
                                modelComponent.Materials.Add(new(0, material));
                                cubeEntity.Components.Add(modelComponent);

                                // Add the cube to the scene
                                Entity.Scene.Entities.Add(cubeEntity);
                            }
                        }
                    }
                }
            }
        }


        public override void Update()
        {
            DebugShot();
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
