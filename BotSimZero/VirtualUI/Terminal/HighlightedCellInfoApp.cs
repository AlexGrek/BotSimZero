using BotSimZero.Core;
using SimuliEngine.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotSimZero.VirtualUI.Terminal
{
    public class HighlightedCellInfoApp : ITerminalApp
    {
        public string[] GetLines(WorldState worldState)
        {
            if (GlobalGameContext.Instance.UiContext.HighlightedCell.HasValue)
            {
                var (x, y) = GlobalGameContext.Instance.UiContext.HighlightedCell.Value;
                return new string[]
                {
                "Highlighted cell:",
                $"{worldState.GetCellState(x, y)}"
                };
            }
            else
                return new string[]
                {
                "No cell highlighted"
                };
        }
    }
}
