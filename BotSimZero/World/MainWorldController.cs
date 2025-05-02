using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using BotSimZero.Core;
using BotSimZero.World.UI;
using SimuliEngine.MapGen;
using SimuliEngine.Simulation;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Physics;

namespace BotSimZero.World
{
    internal class MainWorldController : WorldAwareSyncScript
    {
        
        public float HighlighterFadeSpeed = 0.1f;
        public Prefab HighlightPrefab;
        public Prefab GreenHighlightPrefab;
        private ProceduralMapGenerator _generator;
        public Prefab VisibleWorldManagerPrefab;
        public float TimeShift = 1f;

        public override void Start()
        {
            base.Start();
            // Save and keep world generator
            _generator = new ProceduralMapGenerator(SizeX, SizeY);
            // Create, uhm, The World
            var world = _generator.WithConfig(ProceduralMapGeneratorConfig.DefaultConfig()).Generate();
            // Set the world state
            if (GlobalWorldController == null)
            {
                throw new InvalidOperationException("GlobalController is null during MainWorldController startup script");
            }
            GlobalWorldController.WorldState = world;
            // Create the highlighter
            Highlighter = new CellHighlighter()
            {
                Size = new Vector3(0.5f, 0.5f, 0.5f),
                GlowColor = Color.Yellow,
                GlowIntensity = 5.0f,
                FadeSpeed = HighlighterFadeSpeed,
                HighlightPrefab = HighlightPrefab,
                GreenHighlightPrefab = GreenHighlightPrefab,
            };
            Entity.Add(Highlighter);
            Entity.Scene.Entities.AddRange(VisibleWorldManagerPrefab.Instantiate());
            // Create simulations
            GlobalWorldController.Sim = new GlobalSimulation();
        }

        public override void Update()
        {
            var dt = (float)Game.UpdateTime.Elapsed.TotalSeconds * TimeShift;
            GlobalWorldController.Update(dt);
            DebugText.Print(GlobalWorldController.DebugMessage, new Stride.Core.Mathematics.Int2(20, 20), Color.DarkRed);
            if (Input.IsKeyPressed(Stride.Input.Keys.Space))
            {
                GlobalWorldController.Sim.Enabled = !GlobalWorldController.Sim.Enabled;
            }
        }
    }
}
