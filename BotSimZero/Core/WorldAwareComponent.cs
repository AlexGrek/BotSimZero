using SharpDX.D3DCompiler;
using SimuliEngine.World;
using Stride.Core;
using Stride.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotSimZero.Core
{
    public abstract class WorldAwareComponent: EntityComponent
    {
        protected GlobalController globalWorldController;

        public GlobalController GlobalWorldController { get
            {
                if (globalWorldController == null)
                {
                    globalWorldController = FindGlobalWorldController();
                }
                return globalWorldController;
            }
            protected set => globalWorldController = value;
        }

        public WorldState WorldState => GlobalWorldController?.WorldState;
        public int SizeX = GlobalGameContext.GetSizeX;
        public int SizeY = GlobalGameContext.GetSizeY;

        public GlobalController FindGlobalWorldController()
        {
            var globalWorldController = this.Entity.Scene.Entities
                .FirstOrDefault(e => e.Get<GlobalController>() != null)
                ?.Get<GlobalController>();
            if (globalWorldController == null)
            {
                throw new InvalidOperationException("GlobalController not found in the scene.");
            }
            return globalWorldController;
        }
    }
}
