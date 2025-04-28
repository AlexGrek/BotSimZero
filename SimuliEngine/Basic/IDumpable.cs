using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
        public string Dump(int depth = 0)
        {
            if (this == null)
                return $"{GetIndentation(depth)}null";

            Type type = this.GetType();
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

            if (properties.Length == 0 && fields.Length == 0)
                return $"{GetIndentation(depth)}{type.Name}: [No public properties or fields]";

            var result = new StringBuilder();

            // Only add type name at top level (depth 0)
            if (depth == 0)
                result.AppendLine($"{type.Name}:");

            // Process properties
            foreach (var prop in properties)
            {
                string tabs = GetIndentation(depth + 1);
                string name = prop.Name;
                object value;

                try
                {
                    value = prop.GetValue(this);
                }
                catch (Exception ex)
                {
                    value = $"[Error getting value: {ex.Message}]";
                }

                result.Append($"{tabs}{name}: ");

                if (value == null)
                {
                    result.AppendLine("null");
                    continue;
                }

                if (value is IDumpable dumpable)
                {
                    result.AppendLine();
                    result.Append(dumpable.Dump(depth + 1));
                }
                else if (IsTuple(value.GetType()))
                {
                    result.AppendLine(FormatTuple(value));
                }
                else if (IsIDictionary(value.GetType()))
                {
                    var dict = value as IDictionary;
                    result.AppendLine(value.GetType().Name);

                    foreach (DictionaryEntry entry in dict)
                    {
                        string dictTabs = GetIndentation(depth + 2);
                        result.AppendLine($"{dictTabs}{entry.Key}: ");

                        if (entry.Value == null)
                        {
                            result.AppendLine("null");
                        }
                        else if (entry.Value is IDumpable dumpableValue)
                        {
                            result.AppendLine();
                            result.AppendLine(dumpableValue.Dump(depth + 2));
                        }
                        else
                        {
                            result.AppendLine(entry.Value.ToString());
                        }
                    }
                }
                else if (value is IEnumerable collection && !(value is string))
                {
                    result.AppendLine(value.GetType().Name);
                    int index = 1;
                    foreach (var item in collection)
                    {
                        string itemTabs = GetIndentation(depth + 2);

                        result.AppendLine($"{itemTabs}{index}) ");

                        if (item == null)
                        {
                            result.AppendLine("null");
                        }
                        else if (item is IDumpable dumpableItem)
                        {
                            result.AppendLine();
                            result.Append(dumpableItem.Dump(depth + 2));
                        }
                        else
                        {
                            result.AppendLine(item.ToString());
                        }

                        index++;
                    }
                }
                else
                {
                    result.AppendLine(value.ToString());
                }
            }

            // Process fields
            foreach (var field in fields)
            {
                string tabs = GetIndentation(depth + 1);
                string name = field.Name;
                object value;

                try
                {
                    value = field.GetValue(this);
                }
                catch (Exception ex)
                {
                    value = $"[Error getting value: {ex.Message}]";
                }

                result.Append($"{tabs}{name}: ");

                if (value == null)
                {
                    result.AppendLine("null");
                    continue;
                }

                if (value is IDumpable dumpable)
                {
                    result.AppendLine();
                    result.Append(dumpable.Dump(depth + 1));
                }
                else if (IsTuple(value.GetType()))
                {
                    result.AppendLine(FormatTuple(value));
                }
                else if (IsIDictionary(value.GetType()))
                {
                    var dict = value as IDictionary;
                    result.AppendLine(value.GetType().Name);

                    foreach (DictionaryEntry entry in dict)
                    {
                        string dictTabs = GetIndentation(depth + 2);
                        result.Append($"{dictTabs}{entry.Key}: ");

                        if (entry.Value == null)
                        {
                            result.AppendLine("null");
                        }
                        else if (entry.Value is IDumpable dumpableValue)
                        {
                            result.AppendLine();
                            result.Append(dumpableValue.Dump(depth + 2));
                        }
                        else
                        {
                            result.AppendLine(entry.Value.ToString());
                        }
                    }
                }
                else if (value is IEnumerable collection && !(value is string))
                {
                    result.AppendLine(value.GetType().Name);
                    int index = 1;
                    foreach (var item in collection)
                    {
                        string itemTabs = GetIndentation(depth + 2);

                        result.Append($"{itemTabs}{index}) ");

                        if (item == null)
                        {
                            result.AppendLine("null");
                        }
                        else if (item is IDumpable dumpableItem)
                        {
                            result.AppendLine();
                            result.Append(dumpableItem.Dump(depth + 2));
                        }
                        else
                        {
                            result.AppendLine(item.ToString());
                        }

                        index++;
                    }
                }
                else
                {
                    result.AppendLine(value.ToString());
                }
            }

            return result.ToString().TrimEnd();
        }

        private static string GetIndentation(int depth)
        {
            return new string('\t', depth);
        }

        private static bool IsTuple(Type type)
        {
            if (!type.IsGenericType)
                return false;

            type = type.IsGenericTypeDefinition ? type : type.GetGenericTypeDefinition();
            return type == typeof(Tuple<>) ||
                   type == typeof(Tuple<,>) ||
                   type == typeof(Tuple<,,>) ||
                   type == typeof(Tuple<,,,>) ||
                   type == typeof(Tuple<,,,,>) ||
                   type == typeof(Tuple<,,,,,>) ||
                   type == typeof(Tuple<,,,,,,>) ||
                   type == typeof(ValueTuple<>) ||
                   type == typeof(ValueTuple<,>) ||
                   type == typeof(ValueTuple<,,>) ||
                   type == typeof(ValueTuple<,,,>) ||
                   type == typeof(ValueTuple<,,,,>) ||
                   type == typeof(ValueTuple<,,,,,>) ||
                   type == typeof(ValueTuple<,,,,,,>);
        }

        private static string FormatTuple(object tuple)
        {
            Type type = tuple.GetType();
            var items = type.GetProperties()
                           .Where(p => p.Name.StartsWith("Item"))
                           .OrderBy(p => p.Name)
                           .Select(p => p.GetValue(tuple)?.ToString() ?? "null")
                           .ToList();

            return $"({string.Join(", ", items)})";
        }

        private static bool IsIDictionary(Type type)
        {
            return typeof(IDictionary).IsAssignableFrom(type);
        }

        public string DumpLisp()
        {
            if (this == null)
                return "null";

            Type type = this.GetType();
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

            var result = new StringBuilder();

            // Add type name
            result.Append($"({type.Name} ");

            // Process properties
            foreach (var prop in properties)
            {
                string name = prop.Name;
                object value;

                try
                {
                    value = prop.GetValue(this);
                }
                catch (Exception ex)
                {
                    value = $"[Error getting value: {ex.Message}]";
                }

                result.Append($"({name} ");

                if (value == null)
                {
                    result.Append("null)");
                }
                else if (value is IDumpable dumpable)
                {
                    result.Append($"{dumpable.DumpLisp()})");
                }
                else if (IsTuple(value.GetType()))
                {
                    result.Append($"{FormatTuple(value)})");
                }
                else if (IsIDictionary(value.GetType()))
                {
                    var dict = value as IDictionary;
                    result.Append("(");

                    foreach (DictionaryEntry entry in dict)
                    {
                        result.Append($"({entry.Key} ");

                        if (entry.Value == null)
                        {
                            result.Append("null)");
                        }
                        else if (entry.Value is IDumpable dumpableValue)
                        {
                            result.Append($"{dumpableValue.DumpLisp()})");
                        }
                        else
                        {
                            result.Append($"{entry.Value})");
                        }
                    }

                    result.Append("))");
                }
                else if (value is IEnumerable collection && !(value is string))
                {
                    result.Append("(");

                    foreach (var item in collection)
                    {
                        if (item == null)
                        {
                            result.Append("null ");
                        }
                        else if (item is IDumpable dumpableItem)
                        {
                            result.Append($"{dumpableItem.DumpLisp()} ");
                        }
                        else
                        {
                            result.Append($"{item} ");
                        }
                    }

                    result.Append("))");
                }
                else
                {
                    result.Append($"{value})");
                }
            }

            // Process fields
            foreach (var field in fields)
            {
                string name = field.Name;
                object value;

                try
                {
                    value = field.GetValue(this);
                }
                catch (Exception ex)
                {
                    value = $"[Error getting value: {ex.Message}]";
                }

                result.Append($"({name} ");

                if (value == null)
                {
                    result.Append("null)");
                }
                else if (value is IDumpable dumpable)
                {
                    result.Append($"{dumpable.DumpLisp()})");
                }
                else if (IsTuple(value.GetType()))
                {
                    result.Append($"{FormatTuple(value)})");
                }
                else if (IsIDictionary(value.GetType()))
                {
                    var dict = value as IDictionary;
                    result.Append("(");

                    foreach (DictionaryEntry entry in dict)
                    {
                        result.Append($"({entry.Key} ");

                        if (entry.Value == null)
                        {
                            result.Append("null)");
                        }
                        else if (entry.Value is IDumpable dumpableValue)
                        {
                            result.Append($"{dumpableValue.DumpLisp()})");
                        }
                        else
                        {
                            result.Append($"{entry.Value})");
                        }
                    }

                    result.Append("))");
                }
                else if (value is IEnumerable collection && !(value is string))
                {
                    result.Append("(");

                    foreach (var item in collection)
                    {
                        if (item == null)
                        {
                            result.Append("null ");
                        }
                        else if (item is IDumpable dumpableItem)
                        {
                            result.Append($"{dumpableItem.DumpLisp()} ");
                        }
                        else
                        {
                            result.Append($"{item} ");
                        }
                    }

                    result.Append("))");
                }
                else
                {
                    result.Append($"{value})");
                }
            }

            result.Append(")");
            return result.ToString();
        }
    }
}
