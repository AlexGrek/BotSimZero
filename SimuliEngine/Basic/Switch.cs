using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimuliEngine.Basic
{
    public class Switch
    {
        bool _state = false;

        public void Set(bool state)
        {
            _state = state;
        }

        public bool ReadAndReset()
        {
            bool state = _state;
            _state = false;
            return state;
        }
    }
}
