using System.Runtime.CompilerServices;

namespace SimuliEngine.Tiles
{
    // Base record for TileType
    public abstract record TileType
    {
        private TileType() { } // Prevent external inheritance

        public sealed record Space : TileType; // Empty space, passable
        public sealed record Wall : TileType; // Solid wall, not passable
        public sealed record Hole : TileType; // Hole, not passable
        public sealed record TransparentWall(TransparentWallType Type) : TileType; // Transparent wall, not passable, but passable for light
        public sealed record DoorTile(Door DoorData) : TileType; // Door, passable if open
        public sealed record SpecialObject(SpecialObject ObjectData, bool IsPassable) : TileType; // Special object, passability depends on 2nd parameter

        public static bool IsWall(TileType tileType)
        {
            return tileType is Wall || tileType is TransparentWall;
        }
    }

    public enum TransparentWallType
    {
        Glass,
        Mesh,
        Grate,
    }

    public enum SpecialObject
    {
        None,
        Door,
        Window,
        Trapdoor,
        StairsUp,
        StairsDown,
        Elevator,
    }
}
