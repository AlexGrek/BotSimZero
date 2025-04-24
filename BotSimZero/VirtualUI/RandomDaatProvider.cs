using SimuliEngine.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BotSimZero.VirtualUI
{
    public class RandomDaatProvider : IDisplayDataStringProvider, ISerializable
    {
        public string GetDisplayDataHeader(dynamic options)
        {
            return "Random data";
        }

        public string GetDisplayDataString(dynamic options)
        {
            Random rnd = new Random();
            return rnd.Next().ToString();
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            return;
        }
    }
}
