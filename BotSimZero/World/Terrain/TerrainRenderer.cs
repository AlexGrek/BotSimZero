using Stride.Engine;
using Stride.Core.Mathematics;
using Stride.Graphics;
using Stride.Rendering;
using Stride.Rendering.Materials;
using System.Threading.Tasks;
using Stride.Graphics.GeometricPrimitives;
using System;
using Stride.Extensions;
using Stride.Physics;
using SharpFont.PostScript;
using SimuliEngine.World;
using SimuliEngine.Tiles;
using BotSimZero.Core;
using SimuliEngine.Interop;
using Stride.Input;
using SimuliEngine.Basic;

namespace BotSimZero.World.Terrain
{
    

    public class TerrainRenderer : WorldAwareSyncScript
    {
        public float WallChance = 0.2f; // 20% chance for wall
        // Matrix you generated elsewhere
        private int[,] WorldMatrix;

        private void GenerateWorldMatrix()
        {
            WorldMatrix = new int[SizeX, SizeY];
            //var rand = new Random();

            var dataSource = new CellDataStringProvider(WorldState);
            GlobalGameContext.Instance.RegisterDataSource("Cells", dataSource);

            for (int x = 0; x < SizeX; x++)
            {
                for (int y = 0; y < SizeY; y++)
                {
                    //WorldState.TileTypeMap[x, y] = rand.NextDouble() < WallChance ? new TileType.Wall() : new TileType.Space(); // Initialize tile type
                    WorldMatrix[x, y] = WorldState.TileTypeMap[x, y] is TileType.Wall ? 1 : 0;
                    
                }
            }
        }

        public override void Start()
        {
            base.Start();
            GenerateWorldMatrix();
            GenerateTerrain(WorldMatrix);
        }

        public void GenerateTerrain(int[,] matrix)
        {
            int width = matrix.GetLength(0);
            int height = matrix.GetLength(1);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var isWall = matrix[x, y] == 1;

                    float cubeHeight = isWall ? 2f : 0.2f;

                    if (isWall)
                    {
                        CreateWall(x, y);
                    }
                    else
                    {
                        CreateFloor(x, y);
                    }
                    
                }
            }
        }

        private void CreateWall(int x, int y)
        {
            var modelAssetName = @"EdemAssets/Walls/Walls";
            var asset = Content.Load<Model>(modelAssetName);
            var wallEntity = new Entity($"Wall_{x}_{y}", position: new Vector3(x, 0, y), scale: new Vector3(1, 1, 1));
            var modelComponent = new ModelComponent
            {
                Model = asset
            };
            wallEntity.Add(modelComponent);
            Entity.AddChild(wallEntity);
        }

        private void CreateFloor(int x, int y)
        {
            var modelAssetName = @"EdemAssets/Walls/Floor";
            var asset = Content.Load<Model>(modelAssetName);
            var cubeEntity = new Entity($"Cell_{x}_{y}", position: new Vector3(x, 0, y), scale: new Vector3(1, 1, 1));
            var modelComponent = new ModelComponent
            {
                Model = asset
            };
            cubeEntity.Add(modelComponent);
            var cell = new CellComponent { Xlocation = x, Ylocation = y };
            cubeEntity.Add(cell);
            var colliderShape = new BoxColliderShapeDesc
            {
                Size = new Vector3(1, 0, 1)
            };
            var collider = new StaticColliderComponent
            {
                CollisionGroup = CollisionFilterGroups.CustomFilter1, // Grid-specific group
                ColliderShapes = { colliderShape },
                IsTrigger = false
            };
            cubeEntity.Add(collider);
            Entity.AddChild(cubeEntity);
        }

        private Entity CreateCube(Vector3 position, float height, bool isWall)
        {
            var cubeEntity = new Entity($"Cell_{position.X}_{position.Z}", position: position, scale: new Vector3(1, height, 1));


            var modelComponent = new ModelComponent
            {
                Model = ModelFactory.CreateCube(GraphicsDevice)
            };

            var material = Content.Load<Material>("Materials/Gold");
            modelComponent.Materials.Add(new(0, material));

            cubeEntity.Components.Add(modelComponent);
            // Add physics for collision detection
            
            if (!isWall)
            {
                var cell = new CellComponent { Xlocation = (int)(position.X), Ylocation = (int)(position.Z) };
                cubeEntity.Add(cell);
                var colliderShape = new BoxColliderShapeDesc
                {
                    Size = new Vector3(1, height, 1)
                };
                var collider = new StaticColliderComponent
                {
                    CollisionGroup = CollisionFilterGroups.CustomFilter1, // Grid-specific group
                    ColliderShapes = { colliderShape },
                    IsTrigger = false
                };
                //cubeEntity.Add(collider);
            }
            return cubeEntity;
        }

        public override void Update()
        {
            if (Input.IsKeyPressed(Keys.F))
            {
                DebugUtils.DumpToFile("Data.txt", WorldState);
            }
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
                Diffuse = new MaterialDiffuseMapFeature()
            }
        };
        return Material.New(device, descriptor);
    }
}


}
