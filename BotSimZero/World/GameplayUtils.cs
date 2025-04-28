using BotSimZero.Core;
using SimuliEngine.World;
using Stride.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotSimZero.World
{
    public static class GameplayUtils
    {

        public static WorldState FindWorldState(Entity entity)
        {
            var globalWorldController = entity.Scene.Entities
                .FirstOrDefault(e => e.Get<GlobalController>() != null)
                ?.Get<GlobalController>();
            return globalWorldController?.WorldState;
        }

        internal static GlobalController GetGlobalController(Entity entity)
        {
            var globalWorldController = entity.Scene.Entities
                .FirstOrDefault(e => e.Get<GlobalController>() != null)
                ?.Get<GlobalController>();
            return globalWorldController;
        }
    }
}
