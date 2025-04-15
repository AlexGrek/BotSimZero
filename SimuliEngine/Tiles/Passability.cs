using SimuliEngine.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimuliEngine.Tiles
{
    public static class Passability
    {
        public static bool IsAirPassable((int x, int y) cell, WorldState world)
        {
            var tileType = world.TileTypeMap[cell.x, cell.y];
            return TileType.IsWall(tileType);
        }

        public static bool IsAirPassable(CellDigest cellDigest)
        {
            var tileType = cellDigest.TileType;
            return TileType.IsWall(tileType);
        }
    }
}
