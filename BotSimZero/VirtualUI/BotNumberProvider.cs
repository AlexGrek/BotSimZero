using SimuliEngine.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BotSimZero.VirtualUI
{
    public class BotNumberProvider : IDisplayDataStringProvider, ISerializable
    {
        public int Number { get; set; } = 0;

        public BotNumberProvider()
        {
            Random rnd = new Random();
            Number = rnd.Next(1000, 9999);
        }

        public string GetDisplayDataHeader(dynamic options)
        {
            return "Bot ID";
        }

        public string GetDisplayDataString(dynamic options)
        {
            return Number.ToString();
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            return;
        }
    }
}
