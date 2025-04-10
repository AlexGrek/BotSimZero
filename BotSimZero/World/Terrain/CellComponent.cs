using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stride.Engine.Design;
using Stride.Engine;
using Stride.Core;

namespace BotSimZero.World.Terrain
{
    [DataContract(nameof(CellComponent))]
    //[DefaultEntityComponentProcessor(typeof(MyProcessor))]
    public class CellComponent : EntityComponent
    {
        public CellComponent(int x, int y)
        {
            Xlocation = x;
            Ylocation = y;
        }

        public CellComponent() { }

        public int Xlocation { get; set; }
        public int Ylocation { get; set; }
    }
}
