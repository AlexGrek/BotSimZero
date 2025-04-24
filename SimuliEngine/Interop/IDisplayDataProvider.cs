using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimuliEngine.Interop
{
    public interface IDisplayDataProvider<T>
    {
        public T GetDisplayData();
        public string GetHeader(dynamic options);

        public string GetDisplayDataString(dynamic options);
    }
}
