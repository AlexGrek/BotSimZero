using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SimuliEngine.Simulation
{
    public interface IRealRotationProvider
    {
        public void LookAtSmooth(float turnStep, Vector2 target);

        public bool IsFacing(Vector2 target);

        public Vector2 GetRotationDirection();

        public void SetRotationByNormalVector(Vector2 vector);

        public void LookAtImmediately(Vector2 target);
    }
}
