using BotSimZero.Entities;
using BotSimZero.World.Terrain;
using Stride.Engine.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BotSimZero.World.UI.HighlighterSystem;
using static BotSimZero.World.UI.MultiCellHighlighter;

namespace BotSimZero.Core
{
    public class UiContext
    {
        public (int x, int y)? HighlightedCell = null;
        public BotComponent HighlightedBot = null;

        public void SetHighlightedCell((int x, int y)? cell)
        {
            HighlightedCell = cell;
            UiEvents.OnCellHighlightChanged.Broadcast(cell);
        }

        public void SetHighlightedBot(BotComponent bot)
        {
            HighlightedBot = bot;
            UiEvents.OnBotHighlightChanged.Broadcast(bot);
        }

        public void HighlightAny(dynamic cell, HighlightType type)
        {
            UiEvents.OnHighlightAnything.Broadcast((cell, type));
        }

        public void HighlightSubcells(IEnumerable<CellSubcellHighlight> subcells)
        {
            UiEvents.OnHighlightSubcells.Broadcast(subcells);
        }

        public static class UiEvents
        {
            public static EventKey<(int x, int y)?> OnCellHighlightChanged = new EventKey<(int x, int y)?>("Global", "Highlight cell changed");

            public static EventKey<(dynamic, HighlightType)> OnHighlightAnything = new EventKey<(dynamic, HighlightType)>("Global", "Highlight cell changed");

            public static EventKey<IEnumerable<CellSubcellHighlight>> OnHighlightSubcells = new EventKey<IEnumerable<CellSubcellHighlight>>("Global", "Highlight subcells");

            public static EventKey<BotComponent> OnBotHighlightChanged = new EventKey<BotComponent>("Global", "Highlight bot changed");
        }
    }
}
