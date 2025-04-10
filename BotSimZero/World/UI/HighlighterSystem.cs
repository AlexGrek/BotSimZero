using System.Linq;
using BotSimZero.World.Terrain;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Graphics;
using Stride.Graphics.GeometricPrimitives;
using Stride.Input;
using Stride.Physics;
using Stride.Rendering.Materials;
using Stride.Rendering;
using Stride.Extensions;
using Stride.Rendering.Materials.ComputeColors;

namespace BotSimZero.World.UI
{
    public class HighlighterSystem : SyncScript
    {
        private CellHighlighter highlighter;
        private Simulation simulation;
        private CameraComponent camera;
        public float CellSize = 1;
        public float HighlightGroundLevel = 0.2f;

        public override void Start()
        {
            // Initialize the highlighter
            highlighter = new CellHighlighter
            {
                Size = new Vector3(1, 1, 1),
                GlowColor = Color.Yellow,
                GlowIntensity = 5.0f,
                FadeSpeed = 2.0f
            };
            Entity.Add(highlighter);

            camera = Entity.Scene.Entities.FirstOrDefault(e => e.Get<CameraComponent>() != null)?.Get<CameraComponent>();

            // Get the physics simulation
            simulation = this.GetSimulation();
        }

        public override void Update()
        {
            // Example of handling input to highlight cells
            if (Input.IsMouseButtonPressed(MouseButton.Left))
            {
                Raycast(Input.MousePosition);
            }
        }

        private void Raycast(Vector2 screenPos)
        {
            // Make sure we have camera and simulation
            if (camera == null || simulation == null)
                return;

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

//            Log.Warning($"Near: {unprojectedNear}, far: {unprojectedFar}");

//#if DEBUG
//            VisualizeRay(unprojectedNear, unprojectedFar);
//#endif

            // Perform the raycast from near to far point
            var result = simulation.Raycast(
                unprojectedNear,
                unprojectedFar,
                filterFlags: CollisionFilterGroupFlags.CustomFilter1);


            // If we hit something, process it
            if (result.Succeeded && result.Collider != null)
            {
                var hitEntity = result.Collider.Entity;
                if (hitEntity == null)
                    return;

                var cellComponent = hitEntity.Get<CellComponent>();
                if (cellComponent != null)
                {
                    var gridPosX = cellComponent.Xlocation;
                    var gridPosY = cellComponent.Ylocation;

                    // Calculate world position of cell center
                    var worldPos = new Vector3(
                        gridPosX * CellSize,
                        HighlightGroundLevel,  // Keep at ground level
                        gridPosY * CellSize);

                    // Highlight the cell
                    highlighter.HighlightCell(worldPos);

                    // Handle cell selection for your game logic
                    OnCellSelected(new Vector2(gridPosX, gridPosY));
                }
            }
        }

        private void OnCellSelected(Vector2 gridPosition)
        {
            // Your game logic when a cell is selected
            // For example, moving a unit, placing a building, etc.
            System.Diagnostics.Debug.WriteLine($"Cell selected: {gridPosition}");
        }

        private void VisualizeRay(Vector3 unprojectedNear, Vector3 unprojectedFar)
        {
            var cubeEntityNear = new Entity($"Ray_{unprojectedNear}", position: unprojectedNear, scale: new Vector3(10, 10, 10));
            var modelComponent = new ModelComponent
            {
                Model = ModelFactory.CreateCube(GraphicsDevice)
            };

            var material = MaterialFactory.CreateSimpleMaterial(GraphicsDevice, Color.IndianRed);
            modelComponent.Materials.Add(new(0, material));
            cubeEntityNear.Components.Add(modelComponent);


            var cubeEntityFar = new Entity($"Ray_{unprojectedFar}", position: unprojectedFar, scale: new Vector3(10, 10, 10));
            var modelComponentFar = new ModelComponent
            {
                Model = ModelFactory.CreateCube(GraphicsDevice)
            };

            modelComponentFar.Materials.Add(new(0, material));
            cubeEntityFar.Components.Add(modelComponentFar);

            Entity.Scene.Entities.Add(cubeEntityNear);
            Entity.Scene.Entities.Add(cubeEntityFar);

            const int FRACTIONS = 30;

            var differenceFraction = (unprojectedFar - unprojectedNear) / FRACTIONS;

            for (int i = 0; i < FRACTIONS; i++)
            {
                var intermediate = unprojectedNear + differenceFraction * i;
                var intermediateEntity = new Entity($"Ray_interm_{unprojectedNear}x{i}", position: intermediate, scale: new Vector3(1, 1, 10));
                var imodel = new ModelComponent
                {
                    Model = ModelFactory.CreateCube(GraphicsDevice)
                };
                imodel.Materials.Add(new(0, material));
                intermediateEntity.Components.Add(imodel);
                Entity.Scene.Entities.Add(intermediateEntity);
            }
        }

        public static class ModelFactory
        {
            public static Model CreateCube(GraphicsDevice device)
            {
                var meshDraw = GeometricPrimitive.Cube.New(device).ToMeshDraw();
                var mesh = new Mesh { Draw = meshDraw };
                var model = new Model();
                model.Meshes.Add(mesh);
                return model;
            }
        }

        public static class MaterialFactory
        {
            public static Material CreateSimpleMaterial(GraphicsDevice device, Color color)
            {
                // Materials/DarkStone
                var descriptor = new MaterialDescriptor
                {
                    Attributes =
            {
                Diffuse = new MaterialDiffuseMapFeature(),
                Emissive = new MaterialEmissiveMapFeature(new ComputeColor(color)),
                Transparency = new MaterialTransparencyBlendFeature()
                {
                    Alpha = new ComputeFloat(0.2f)
                }
            }
                };
                return Material.New(device, descriptor);
            }
        }
    }
}
