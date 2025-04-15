using BotSimZero.World.UI;
using SimuliEngine.Simulation;
using SimuliEngine.World;
using Stride.Core;
using Stride.Engine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotSimZero.Core
{
    [DataContract(nameof(GlobalController))]
    public class GlobalController : EntityComponent
    {
        public CellHighlighter Highlighter;
        public Entity PointingAtEntity = null;
        public WorldState WorldState = null;
        public SimulationSubsystem Sim = null;

        public string DebugMessage = "";

        public static GlobalController FindGlobalWorldController(Entity entity)
        {
            var globalWorldController = entity.Scene.Entities
                .FirstOrDefault(e => e.Get<GlobalController>() != null)
                ?.Get<GlobalController>();
            if (globalWorldController == null)
            {
                throw new InvalidOperationException("GlobalController not found in the scene.");
            }
            return globalWorldController;
        }

        public static GlobalController FindGlobalWorldController(EntityComponent component)
        {
            return FindGlobalWorldController(component.Entity);
        }

        /// <summary>
        /// Run all systems per frame
        /// Time should be already shifted by the MainWorldController
        /// </summary>
        /// <param name="dt"></param>
        public void Update(float dt)
        {
            var stopwatch = Stopwatch.StartNew(); // Start measuring time
            
            Sim.Tick(dt, WorldState);

            stopwatch.Stop(); // Stop measuring time

            DebugMessage = $"Sim.Tick executed in {stopwatch.ElapsedMilliseconds} ms ({stopwatch.Elapsed.TotalSeconds}), while frame time is {dt}";
        }
    }
}
