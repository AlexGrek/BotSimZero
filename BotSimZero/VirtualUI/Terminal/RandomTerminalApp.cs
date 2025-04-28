using SimuliEngine.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotSimZero.VirtualUI.Terminal
{
    public class RandomTerminalApp: ITerminalApp
    {
        public string[] GetLines(WorldState worldState)
        {
            Random random = new Random();
            return new string[]
            {
                "Random Terminal App",
                "This app generates random text.",
                "It can be used for testing purposes.",
                $"{random.Next()}"
            };
        }
    }
}
