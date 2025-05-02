using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SimuliEngine.MapGen
{
    public class ProceduralMapGeneratorConfig
    {
        public static ProceduralMapGeneratorConfig DefaultConfig()
        {
            return new ProceduralMapGeneratorConfig(@"
seed: 42
terrain:
    wall:
        chance: 0.2
fluctuations:
    temp:
        points: 30
        base: 21
        spikes: -2
spawn:
    count: 10
special:
    charging:
        enabled: true
        count: 5
");
        }

        private readonly dynamic _config;

        public ProceduralMapGeneratorConfig()
        {
            _config = new ExpandoObject();
        }

        public ProceduralMapGeneratorConfig(string yaml)
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

                var yamlObject = deserializer.Deserialize<Dictionary<object, object>>(yaml);
                // Convert to dynamic
                _config = ProceduralMapGeneratorConfig.ConvertToDynamic(yamlObject);
        }

        protected static dynamic ConvertToDynamic(object obj)
        {
            if (obj is Dictionary<object, object> dictionary)
            {
                var expando = new ExpandoObject() as IDictionary<string, object>;

                foreach (var kvp in dictionary)
                {
                    var key = kvp.Key.ToString();
                    var value = ProceduralMapGeneratorConfig.ConvertToDynamic(kvp.Value);
                    if (key == null)
                    {
                        throw new InvalidOperationException("Key cannot be null");
                    }
                    expando[key] = value;
                }

                return expando;
            }
            else if (obj is List<object> list)
            {
                return list.ConvertAll(item => ProceduralMapGeneratorConfig.ConvertToDynamic(item));
            }
            else
            {
                return obj;
            }
        }

        public dynamic Data => _config;

        // Helper method to navigate nested paths using "dot.notation"
        public dynamic GetValue(string path)
        {
            var parts = path.Split('.');
            dynamic current = _config;

            foreach (var part in parts)
            {
                if (current is IDictionary<string, object> dict)
                {
                    if (dict.TryGetValue(part, out var value))
                    {
                        current = value;
                    }
                    else
                    {
#pragma warning disable CS8603 // Possible null reference return.
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
#pragma warning restore CS8603
            return current;
        }

#pragma warning disable CS8601 // Possible null reference assignment.
        public T GetValueOrDefault<T>(string path, T defaultValue = default)
#pragma warning restore CS8601 // Possible null reference assignment.
        {
            try
            {
                var value = GetValue(path);
                if (value == null)
                    return defaultValue;

                // Handle type conversion
                if (value is T typedValue)
                    return typedValue;

                // Try converting
                try
                {
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                catch
                {
                    return defaultValue;
                }
            }
            catch
            {
                return defaultValue;
            }
        }

        // Check if a path exists
        public bool HasPath(string path)
        {
            try
            {
                var parts = path.Split('.');
                dynamic current = _config;

                foreach (var part in parts)
                {
                    if (current is IDictionary<string, object> dict)
                    {
                        if (!dict.ContainsKey(part))
                            return false;

                        current = dict[part];
                    }
                    else
                    {
                        return false;
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
        // GetValueOrDie: Throws an exception if the path is not found
        public T GetValueOrDie<T>(string path)
        {
            var value = GetValue(path);
            if (value == null)
            {
                throw new KeyNotFoundException($"The specified path '{path}' was not found in the configuration.");
            }

            // Handle type conversion
            if (value is T typedValue)
                return typedValue;

            try
            {
                // Handle numeric conversions
                if (typeof(T).IsPrimitive || typeof(T) == typeof(decimal))
                {
                    return (T)Convert.ChangeType(value, typeof(T));
                }

                throw new InvalidCastException($"The value at path '{path}' cannot be cast to type '{typeof(T).Name}'.");
            }
            catch (Exception ex)
            {
                return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
                throw new InvalidCastException($"Failed to convert the value at path '{path}' to type '{typeof(T).Name}'.", ex);
            }
        }
    }
}
