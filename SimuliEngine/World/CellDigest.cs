using SimuliEngine.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimuliEngine.World
{
    public readonly struct CellDigest
    {
        public float Temperature { get; init; }
        public TileType TileType { get; init; }
    }
}
