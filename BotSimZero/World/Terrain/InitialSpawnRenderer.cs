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

namespace BotSimZero.World.Terrain
{
    

    public class InitialSpawnRenderer : WorldAwareSyncScript
    {
        public Prefab BotPrefab;

        public override void Start()
        {
            base.Start();
            WorldState.InitialSpawnPositions.ForEach(SpawnBot);
        }

        public void SpawnBot((int x, int y) pos)
        {
            var position = new Vector3(pos.x, 1f, pos.y);
            var bot = BotPrefab.Instantiate()[0];
            bot.Transform.Position = position;
            bot.Get<BotComponent>().SpawnPosition = pos;
            Entity.Scene.Entities.Add(bot);
        }

        public override void Update()
        {
            
        }
    }
}
