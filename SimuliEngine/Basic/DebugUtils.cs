using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimuliEngine.Basic
{
    public static class DebugUtils
    {
        public static void DumpToFile(string fileName, IDumpable obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj), "Object to dump cannot be null.");
            string dump = obj.DumpLisp();
            System.IO.File.WriteAllText(fileName, dump);
        }

        public static void DumpToOutput(string fileName, IDumpable obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj), "Object to dump cannot be null.");
            string dump = obj.Dump();
            Console.WriteLine(dump);
        }
    }
}
