using BotSimZero.Core;
using Stride.Core;
using Stride.Engine;
using Stride.Engine.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotSimZero.Entities
{
    [DataContract(nameof(SmartEntity))]
    [DefaultEntityComponentProcessor(typeof(SmartEntityProcessor))]
    public class SmartEntity: WorldAwareComponent
    {

    }
}
