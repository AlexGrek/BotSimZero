using BotSimZero.World.UI;
using Stride.Engine;
using Stride.Graphics.GeometricPrimitives;
using Stride.Graphics;
using Stride.Rendering.Materials.ComputeColors;
using Stride.Rendering.Materials;
using Stride.Rendering;
using System;
using System.Linq;
using Stride.Extensions;
using Stride.Core.Mathematics;
using SimuliEngine.World;
using Stride.Physics;
using BotSimZero.World;

namespace BotSimZero.Core
{
    public abstract class WorldAwareSyncScript : SyncScript
    {
        public GlobalController GlobalWorldController { get; protected set; }

        public WorldState WorldState => GlobalWorldController?.WorldState;
        public int SizeX = GlobalGameContext.GetSizeX;
        public int SizeY = GlobalGameContext.GetSizeY;

        public CellHighlighter Highlighter
        {
            get => GlobalWorldController?.Highlighter;
            protected set
            {
                if (GlobalWorldController != null)
                {
                    GlobalWorldController.Highlighter = value;
                }
                else
                {
                    throw new InvalidOperationException("GlobalController is not initialized.");
                }
            }
        }

        public override void Start()
        {
            GlobalWorldController = GameplayUtils.GetGlobalController(Entity);
            if ( GlobalWorldController == null)
            {
                throw new InvalidOperationException("GlobalController not found in the scene.");
            }
        }

        public void VisualizeRay(Vector3 unprojectedNear, Vector3 unprojectedFar)
        {
            var cubeEntityNear = new Entity($"Ray_{unprojectedNear}", position: unprojectedNear, scale: new Vector3(1, 1, 1));
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

            const int FRACTIONS = 300;

            var differenceFraction = (unprojectedFar - unprojectedNear) / (FRACTIONS * 50);

            for (int i = 0; i < FRACTIONS; i++)
            {
                var intermediate = unprojectedNear + differenceFraction * i;
                var intermediateEntity = new Entity($"Ray_interm_{unprojectedNear}x{i}", position: intermediate, scale: new Vector3(0.1f, 0.1f, 0.1f));
                var imodel = new ModelComponent
                {
                    Model = ModelFactory.CreateCube(GraphicsDevice)
                };
                imodel.Materials.Add(new(0, material));
                intermediateEntity.Components.Add(imodel);
                // Add physics for collision detection
                var colliderShape = new BoxColliderShapeDesc
                {
                    Size = new Vector3(0.1f, 0.1f, 0.1f)
                };
                var collider = new RigidbodyComponent
                {
                    Mass = 0.1f,
                    CollisionGroup = CollisionFilterGroups.CustomFilter1, // Grid-specific group
                    ColliderShapes = { colliderShape },
                    IsTrigger = false
                };
                //intermediateEntity.Add(collider);

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

            public static Model CreateCyl(GraphicsDevice device, float height = 1f, float r = 0.1f)
            {
                var meshDraw = GeometricPrimitive.Cylinder.New(device, height, r).ToMeshDraw();
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
