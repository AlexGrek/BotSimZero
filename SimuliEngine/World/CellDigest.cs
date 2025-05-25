using SimuliEngine.Basic;
using SimuliEngine.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SimuliEngine.World
{
    public readonly struct CellDigest: IDumpable
    {
        public float Temperature { get; init; }

        public float Dirt { get; init; }
        public TileType TileType { get; init; }

        public string GetValue(string propertyName)
        {
            // Use reflection to get the property by name
            var property = typeof(CellDigest).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance) ?? throw new ArgumentException($"Property '{propertyName}' does not exist in {nameof(CellDigest)}.");

            // Get the value of the property
            var value = property.GetValue(this);

            // Return the value as a string
            return value?.ToString() ?? string.Empty;
        }

        public override string ToString()
        {
            return (this as IDumpable).Dump(new HashSet<object>());
        }
    }
}
