using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace SimuliEngine.Simulation.Obstacles
{
    public class PlayzoneBoundaryObstacle : IObstacle
    {
        public float HalfSize => 1f;

        public (int x, int y) MainPosition => (0, 0);

        public bool IsTemporary => false;

        public bool IsMoving => false;
    }

    public class WallObstacle : IObstacle
    {
        private (int x, int y) _position;

        public WallObstacle((int x, int y) position)
        {
            _position = position;
        }

        public float HalfSize => 1f;

        public (int x, int y) MainPosition => (0, 0);

        public bool IsTemporary => false;

        public bool IsMoving => false;
    }
}
