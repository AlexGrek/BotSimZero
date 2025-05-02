using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimuliEngine.Basic
{
    public struct Point
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static Point operator +(Point a, Point b)
        {
            return new Point(a.X + b.X, a.Y + b.Y);
        }

        public static Point operator -(Point a, Point b)
        {
            return new Point(a.X - b.X, a.Y - b.Y);
        }

        public static bool operator ==(Point a, Point b)
        {
            return a.X == b.X && a.Y == b.Y;
        }

        public static bool operator !=(Point a, Point b)
        {
            return !(a == b);
        }

        public override bool Equals(object? obj) // Fix for CS8765: Allow obj to be nullable
        {
            if (obj is Point point)
                return this == point;
            return false;
        }

        public override int GetHashCode()
        {
            return (X, Y).GetHashCode();
        }

        public Point((int x, int y) tuple)
        {
            X = tuple.x;
            Y = tuple.y;
        }

        public static implicit operator (int x, int y)(Point point)
        {
            return (point.X, point.Y);
        }
    }
}
