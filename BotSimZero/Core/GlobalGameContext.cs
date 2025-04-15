using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotSimZero.Core
{
    internal class GlobalGameContext
    {
        private static GlobalGameContext _instance;
        public static GlobalGameContext Instance => _instance ??= new GlobalGameContext(64, 64);

        public int SizeX { get; private set; }
        public int SizeY { get; private set; }

        private GlobalGameContext(int sizeX, int sizeY)
        {
            SizeX = sizeX;
            SizeY = sizeY;
        }

        public static (int, int) GetSize => (Instance.SizeX, Instance.SizeY);
        public static int GetSizeX => Instance.SizeX;
        public static int GetSizeY => Instance.SizeY;
    }
}
