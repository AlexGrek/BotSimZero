using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SimuliEngine.Simulation
{
    public interface IRealPositionProvider
    {
        public void Move(float s, Vector2 direction);
        public Vector2 GetRelativePosition(Vector2 center);
        public Vector2 GetNormalizedPosition();
        public void SetNormalizedPosition(Vector2 pos);
    }
}
