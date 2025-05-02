using Stride.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotSimZero.World.UI
{
    internal class MultiCellHighlightComponent : SyncScript
    {
        public float ScalingFactor = 10f;
        public float Delay = 0f;

        public bool Scaling = false;

        public override void Start()
        {
            if (!Scaling)
            {
                var scale = Entity.Transform.Scale;
                scale.X = 1f;
                scale.Z = 1f;
                Entity.Transform.Scale = scale;
            }
        }

        public override void Update()
        {
            if (Delay > 0f)
            {
                Delay -= (float)Game.UpdateTime.Elapsed.TotalSeconds;
                return;
            }
            if (Scaling)
            {
                var scale = Entity.Transform.Scale;
                scale.X += ScalingFactor * (float)Game.UpdateTime.Elapsed.TotalSeconds;
                scale.Z += ScalingFactor * (float)Game.UpdateTime.Elapsed.TotalSeconds;
                Entity.Transform.Scale = scale;
                if (scale.X >= 1f)
                {
                    scale.X = 1f;
                    scale.Z = 1f;
                    Entity.Transform.Scale = scale;
                    Scaling = false;
                    this.Entity.Remove(this);
                }
            }
        }
    }
}
