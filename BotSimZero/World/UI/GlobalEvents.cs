using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BotSimZero.World.Terrain;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Engine.Events;

namespace BotSimZero.World.UI
{
    public static class GlobalEvents
    {
        public static EventKey<CellComponent> HighlightCellEventKey = new EventKey<CellComponent>("Global", "Highlight cell");

        public static void SendHighlightCellEvent(CellComponent position)
        {
            HighlightCellEventKey.Broadcast(position);
        }

        public static EventKey<Entity> PointerEnterEventKey = new EventKey<Entity>("Global", "Mouse enter");
        public static EventKey<Entity> PointerExitEventKey = new EventKey<Entity>("Global", "Mouse exit");
        public static EventKey<Entity> PointerClickEventKey = new EventKey<Entity>("Global", "Mouse click");

        public static bool Initialized { get; private set; } = false;
        public static void Initialize()
        {
            if (Initialized)
                throw new InvalidOperationException("GlobalEvents has already been initialized.");
            Initialized = true;
        }
    }
}
