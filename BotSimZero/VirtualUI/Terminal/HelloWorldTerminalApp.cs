using SimuliEngine.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotSimZero.VirtualUI.Terminal
{
    public class HelloWorldTerminalApp: ITerminalApp
    {
        public string[] GetLines(WorldState worldState)
        {
            return new string[]
            {
                "Hello, World!",
                "This is a simple terminal app.",
                "It can display multiple lines of text.",
                "123456789012345678901234567890"
            };
        }
    }
}
