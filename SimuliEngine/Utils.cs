﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SimuliEngine
{
    public static class Utils
    {
        public class RandomVariant<T>
        {
            public T Item { get; set; }
            public double Chance { get; set; }
            public RandomVariant(T item, double chance)
            {
                Item = item;
                Chance = chance;
            }
        }

        public static T RandomItem<T>(this Random random, IEnumerable<RandomVariant<T>> variants)
        {
            if (variants == null || !variants.Any())
                throw new ArgumentException("Variants collection cannot be null or empty");
            double totalChance = variants.Sum(v => v.Chance);
            if (totalChance <= 0)
                throw new ArgumentException("Total chance must be greater than zero");
            double randomValue = random.NextDouble() * totalChance;
            double cumulativeChance = 0;
            foreach (var variant in variants)
            {
                cumulativeChance += variant.Chance;
                if (randomValue < cumulativeChance)
                {
                    return variant.Item;
                }
            }
            // Fallback in case of rounding errors
            return variants.Last().Item;
        }

        public static T RandomOf<T>(this Random random, params (T, double)[] variants)
        {
            var asVariants = variants.Select(v => new RandomVariant<T>(v.Item1, v.Item2));
            return random.RandomItem(asVariants);
        }

        /// <summary>
        /// Generates a random point within the given 2D array dimensions
        /// </summary>
        /// <param name="random">The Random instance</param>
        /// <param name="sizeX">Width of the 2D array (number of columns)</param>
        /// <param name="sizeY">Height of the 2D array (number of rows)</param>
        /// <returns>A tuple containing (x, y) coordinates within the array bounds</returns>
        public static (int x, int y) RandomPoint(this Random random, int sizeX, int sizeY)
        {
            if (sizeX <= 0)
                throw new ArgumentOutOfRangeException(nameof(sizeX), "Array width must be positive");

            if (sizeY <= 0)
                throw new ArgumentOutOfRangeException(nameof(sizeY), "Array height must be positive");

            int x = random.Next(sizeX);
            int y = random.Next(sizeY);

            return (x, y);
        }

        /// <summary>
        /// Generates a random point within the given 2D array dimensions, excluding a specific point
        /// </summary>
        /// <param name="random">The Random instance</param>
        /// <param name="sizeX">Width of the 2D array (number of columns)</param>
        /// <param name="sizeY">Height of the 2D array (number of rows)</param>
        /// <param name="excludeX">X coordinate to exclude</param>
        /// <param name="excludeY">Y coordinate to exclude</param>
        /// <returns>A tuple containing (x, y) coordinates within the array bounds, different from the excluded point</returns>
        public static (int x, int y) RandomPoint(this Random random, int sizeX, int sizeY, int excludeX, int excludeY)
        {
            if (sizeX <= 0)
                throw new ArgumentOutOfRangeException(nameof(sizeX), "Array width must be positive");

            if (sizeY <= 0)
                throw new ArgumentOutOfRangeException(nameof(sizeY), "Array height must be positive");

            if (sizeX == 1 && sizeY == 1)
                throw new ArgumentException("Cannot generate a different point in a 1x1 array");

            // Generate a random point
            int x, y;
            do
            {
                x = random.Next(sizeX);
                y = random.Next(sizeY);
            } while (x == excludeX && y == excludeY);

            return (x, y);
        }

        public static int NormalDistributionRandomInt(this Random random, int min, int max)
        {
            if (min >= max)
                throw new ArgumentException("Min must be less than max");

            // Calculate mean and standard deviation for the range
            double mean = (min + max) / 2.0;
            double stddev = (max - min) / 6.0; // 99.7% of values within [min, max] (3σ rule)

            int value;
            do
            {
                // Box-Muller transform to generate a standard normal value
                double u1 = 1.0 - random.NextDouble(); // avoid 0
                double u2 = 1.0 - random.NextDouble();
                double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);

                // Scale and shift to desired mean and stddev
                double randNormal = mean + stddev * randStdNormal;

                value = (int)Math.Round(randNormal);
            }
            while (value < min || value >= max);

            return value;
        }

        /// <summary>
        /// Returns the coordinates of the neighbours of a cell in a 2D grid.
        /// </summary>
        /// <param name="x">initial cell x</param>
        /// <param name="y">initial cell y</param>
        /// <param name="xBounds">x must be less than this</param>
        /// <param name="yBounds">y must be less than this</param>
        /// <param name="minX">x must be more or equal to this</param>
        /// <param name="minY">y must be more or equal to this</param>
        /// <returns></returns>
        public static (int, int)[] NeighboursOfACell(int x, int y, int xBounds, int yBounds, int minX = 0, int minY = 0)
        {
            if (x == xBounds - 1)
            {
                // x is at the right edge
                if (y == yBounds - 1)
                {
                    // x is at the bottom right corner
                    return [(x - 1, y), (x, y - 1)];
                } else
                {
                    // x is at the right edge but not at the bottom
                    if (y == minY)
                    {
                        // x is at the top right corner
                        return [(x - 1, y), (x, y + 1)];
                    }
                    return [(x - 1, y), (x, y - 1), (x, y + 1)];
                }
            } else if (x == minX)
            {
                // x is at the left edge
                if (y == yBounds - 1)
                {
                    // x is at the bottom left corner
                    return [(x + 1, y), (x, y - 1)];
                }
                else
                {
                    // x is at the left edge but not at the bottom
                    if (y == minY)
                    {
                        // x is at the top left corner
                        return [(x + 1, y), (x, y + 1)];
                    }
                    return [(x + 1, y), (x, y - 1), (x, y + 1)];
                }
            }
            if (y == yBounds - 1)
            {
                // x is not at the right or left edge but at the bottom edge
                return [(x - 1, y), (x + 1, y), (x, y - 1)];
            }
            if (y == minY)
            {
                // x is not at the right or left edge but at the top edge
                return [(x - 1, y), (x + 1, y), (x, y + 1)];
            }
            // x is not at any edge
            return
                [
            (x - 1, y),
            (x, y - 1),
            (x, y + 1),
            (x + 1, y),
                ];
        }

        public static float Distance((int x, int y) a, (int x, int y) b)
        {
            return (float)Math.Sqrt(Math.Pow(a.x - b.x, 2) + Math.Pow(a.y - b.y, 2));
        }

        public static int ManhattanDistance((int x, int y) a, (int x, int y) b)
        {
            return Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);
        }

        public static dynamic ParseTuple(string input)
        {
            // Remove parentheses and split by commas
            string content = input.Trim();
            if (content.StartsWith("(") && content.EndsWith(")"))
            {
                content = content.Substring(1, content.Length - 2);
            }

            // Split by commas, but not commas within quotes
            var regex = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
            string[] parts = regex.Split(content);

            // Parse each part and determine its type
            var values = parts.Select<string, object>(part =>
           {
               part = part.Trim();

               // Check if it's a quoted string
               if ((part.StartsWith("\"") && part.EndsWith("\"")) ||
                   (part.StartsWith("'") && part.EndsWith("'")))
               {
                   return part.Substring(1, part.Length - 2);
               }

               // Try parsing as integer
               if (int.TryParse(part, out int intValue))
               {
                   return intValue;
               }

               // If not an integer or quoted string, treat as regular string
               return part;
           }).ToArray();

            // Return dynamic tuple based on number of elements
            switch (values.Length)
            {
                case 1:
                    return Tuple.Create(values[0]);
                case 2:
                    return Tuple.Create(values[0], values[1]);
                case 3:
                    return Tuple.Create(values[0], values[1], values[2]);
                case 4:
                    return Tuple.Create(values[0], values[1], values[2], values[3]);
                case 5:
                    return Tuple.Create(values[0], values[1], values[2], values[3], values[4]);
                case 6:
                    return Tuple.Create(values[0], values[1], values[2], values[3], values[4], values[5]);
                case 7:
                    return Tuple.Create(values[0], values[1], values[2], values[3], values[4], values[5], values[6]);
                default:
                    throw new ArgumentException($"Cannot create tuple with {values.Length} elements. Maximum supported is 7.");
            }
        }
    }
}
