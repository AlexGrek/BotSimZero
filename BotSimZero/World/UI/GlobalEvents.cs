using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Engine.Events;

namespace BotSimZero.World.UI
{
    public static class GlobalEvents
    {
        public static EventKey<Vector3> HighlightCellEventKey = new EventKey<Vector3>("Global", "Game Over With Data");

        public static void SendHighlightCellEvent(Vector3 position)
        {
            HighlightCellEventKey.Broadcast(position);
        }
    }
}
