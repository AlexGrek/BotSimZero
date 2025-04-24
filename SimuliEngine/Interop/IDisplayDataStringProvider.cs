using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimuliEngine.Interop
{
    public interface IDisplayDataStringProvider
    {
        public string GetDisplayDataHeader(dynamic options);
        public string GetDisplayDataString(dynamic options);
    }
}
