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
using BotSimZero.Entities;
using SimuliEngine.Simulation.ActorSystem;
using SimuliEngine;
using BotSimZero.VirtualUI;

namespace BotSimZero.World.Terrain
{
    

    public class InitialSpawnRenderer : WorldAwareSyncScript
    {
        public Prefab BotPrefab;

        public override void Start()
        {
            base.Start();

            int botIndex = 0;
            foreach (var spawnPosition in WorldState.InitialSpawnPositions)
            {
                SpawnBot(spawnPosition, botIndex);
                botIndex++;
            }
        }

        public void SpawnBot((int x, int y) pos, int i)
        {
            var position = new Vector3(pos.x, 0f, pos.y);
            var bot = BotPrefab.Instantiate()[0];
            bot.Transform.Position = position;
            var botComponent = bot.Get<BotComponent>();
            botComponent.SpawnPosition = pos;
            var screen = bot.GetChild(0);
            var screenComponent = screen.Get<UiDisplayAsyncScript>();
            screenComponent.DataProvider = new BotNumberProvider();
            screenComponent.FontSize = 400f;
            screenComponent.UpdateEveryNFrames = 1024;
            bot.Name = $"Bot_{i}_{pos.x}_{pos.y}";
            Entity.Scene.Entities.Add(bot);
        }

        public override void Update()
        {
            
        }
    }
}
