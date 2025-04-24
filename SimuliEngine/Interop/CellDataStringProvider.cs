using SimuliEngine.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimuliEngine.Interop
{
    public class CellDataStringProvider : IDisplayDataStringProvider
    {
        private WorldState _world;

        public CellDataStringProvider(WorldState world)
        {
            _world = world;
        }

        public string GetDisplayDataHeader(dynamic options)
        {
            // Assuming 'options' is a tuple of type (int x, int y)
            if (options is ValueTuple<int, int, string> tuple)
            {
                int x = tuple.Item1;
                int y = tuple.Item2;
                string value = tuple.Item3;
                return $"{value} at: {x}{y}";
            }
            throw new ArgumentException("Invalid options provided. Expected a tuple of (int, int, string).");
        }

        public string GetDisplayDataString(dynamic options)
        {
            // Assuming 'options' is a tuple of type (int x, int y)
            if (options is ValueTuple<int, int, string> tuple)
            {
                int x = tuple.Item1;
                int y = tuple.Item2;
                string value = tuple.Item3;
                CellDigest cell = _world.GetCellState(x, y);
                return cell.GetValue(value);
            }
            throw new ArgumentException("Invalid options provided. Expected a tuple of (int, int, string).");
        }
    }
}
