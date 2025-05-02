using BotSimZero.Core;
using SimuliEngine.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotSimZero.VirtualUI.Terminal
{
    public class LogTerminalApp: ITerminalApp
    {
        public LogTerminalApp() {
        
        }

        public string[] GetLines(WorldState worldState)
        {
            var provider = GlobalGameContext.Instance.GetDataSourceByAddress("LastLog");

            var longLine = provider.GetDisplayDataString(null);
            var lines = longLine.SplitEveryNChar(37);
            return lines;
        }
    }
    public static class StringExtensions
    {
        public static string[] SplitEveryNChar(this string input, int n)
        {
            if (input == null)
                throw new ArgumentException("Input string cannot be null or empty, and n must be greater than 0.");

            var result = new List<string>();
            for (int i = 0; i < input.Length; i += n)
            {
                result.Add(input.Substring(i, Math.Min(n, input.Length - i)));
            }
            return [.. result];
        }
    }
}
