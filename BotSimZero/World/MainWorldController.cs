using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using BotSimZero.World.UI;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Physics;

namespace BotSimZero.World
{
    internal class MainWorldController : SyncScript
    {
        private CellHighlighter highlighter;


        public override void Start()
        {
            // Create the highlighter
            highlighter = new CellHighlighter()
            {
                Size = new Vector3(1, 1, 2),
                GlowColor = Color.Yellow,
                GlowIntensity = 5.0f,
                FadeSpeed = 2.0f
            };
            Entity.Add(highlighter);
        }

        public override void Update()
        {
            
        }
    }
}
