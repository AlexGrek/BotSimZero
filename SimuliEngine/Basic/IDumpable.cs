using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SimuliEngine.Basic
{
    /// <summary>
    /// Interface for objects that can be dumped as a formatted string representation
    /// </summary>
    public interface IDumpable
    {
        /// <summary>
        /// Dumps the object properties as a formatted string with the specified indentation depth
        /// </summary>
        /// <param name="depth">The indentation depth</param>
        /// <returns>A formatted string representation of the object</returns>
        public virtual string Dump(HashSet<object>? visited, int depth = 0)
        {
            visited ??= new HashSet<object>();
            if (visited.Contains(this))
                return $"{GetIndentation(depth)}[Circular reference]\n";

            visited.Add(this);
            if (this == null)
                return $"{GetIndentation(depth)}null\n";

            Type type = this.GetType();
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

            if (properties.Length == 0 && fields.Length == 0)
                return $"{GetIndentation(depth)}{type.Name}: [No public properties or fields]\n";

            var result = new StringBuilder();

            // Only add type name at top level (depth 0)
            if (depth == 0)
                result.AppendLine($"{type.Name}:");

            // Process properties
            foreach (var prop in properties)
            {
                result.Append(DumpProperty(prop.Name, () => prop.GetValue(this), visited, depth + 1));
            }

            // Process fields
            foreach (var field in fields)
            {
                result.Append(DumpProperty(field.Name, () => field.GetValue(this), visited, depth + 1));
            }

            return result.ToString().TrimEnd();
        }

        public static string DumpProperty(string name, Func<object?> valueProvider, HashSet<object> visited, int depth)
        {
            var result = new StringBuilder();
            string tabs = GetIndentation(depth);
            object? value;

            try
            {
                value = valueProvider();
            }
            catch (Exception ex)
            {
                value = $"[Error getting value: {ex.Message}]";
            }

            result.Append($"{tabs}{name}: ");

            if (value == null)
            {
                result.AppendLine("null");
            }
            else if (value is IDumpable dumpable)
            {
                result.AppendLine();
                result.Append(dumpable.Dump(visited, depth));
            }
            else if (value is IDictionary dictionary)
            {
                result.AppendLine(dictionary.Dump(visited, depth));
            }
            else if (value is IEnumerable collection && !(value is string))
            {
                result.AppendLine(collection.Dump(visited, depth));
            }
            else
            {
                result.AppendLine(value.ToString());
            }

            return result.ToString();
        }

        public static string GetIndentation(int depth)
        {
            return new string('\t', depth);
        }

        public static string GenerateCode(object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            // Get the hash code and take the absolute value to avoid negative numbers
            int hashCode = Math.Abs(value.GetHashCode());

            // Take first 3 digits (or fewer if hash code is small)
            string hashStr = hashCode.ToString();
            string firstDigits = hashStr.Length >= 3
                ? hashStr.Substring(0, 3)
                : hashStr.PadLeft(3, '0');

            // Get the type name and take first 3 characters
            string typeName = value.GetType().Name;
            if (typeName.Length < 3)
                throw new ArgumentException("Type name must be at least 3 characters long", nameof(value));

            string typePrefix = typeName.Substring(0, 3);

            // Return combined code
            return $"{firstDigits}-{typePrefix}";
        }
    }

    public static class DumpExtensions
    {
        public static string Dump(this IEnumerable collection, HashSet<object> visited, int depth)
        {
            var result = new StringBuilder();
            string tabs = IDumpable.GetIndentation(depth);
            result.AppendLine(collection.GetType().Name);

            int index = 1;
            foreach (var item in collection)
            {
                result.Append($"{tabs}{index}) ");

                if (item == null)
                {
                    result.AppendLine("null");
                }
                else if (item is IDumpable dumpableItem)
                {
                    result.AppendLine(dumpableItem.GetType().ToString());
                    result.Append(dumpableItem.Dump(visited, depth + 1));
                }
                else
                {
                    result.AppendLine(item.ToString());
                }

                index++;
            }

            return result.ToString();
        }

        public static string Dump(this IDictionary dictionary, HashSet<object> visited, int depth)
        {
            var result = new StringBuilder();
            string tabs = IDumpable.GetIndentation(depth);
            result.AppendLine(dictionary.GetType().Name);

            foreach (DictionaryEntry entry in dictionary)
            {
                result.Append($"{tabs}{entry.Key}: ");

                if (entry.Value == null)
                {
                    result.AppendLine("null");
                }
                else if (entry.Value is IDumpable dumpableValue)
                {
                    result.AppendLine();
                    result.Append(dumpableValue.Dump(visited, depth + 1));
                }
                else
                {
                    result.AppendLine(entry.Value.ToString());
                }
            }

            return result.ToString();
        }
    }
}
