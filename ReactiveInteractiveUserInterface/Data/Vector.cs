using System;

namespace TP.ConcurrentProgramming.Data
{
    public record Vector : IVector
    {
        public double x { get; init; }
        public double y { get; init; }

        public Vector(double XComponent, double YComponent)
        {
            x = XComponent;
            y = YComponent;
        }

        public double DistanceTo(IVector other)
        {
            double dx = x - other.x;
            double dy = y - other.y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public static Vector operator +(Vector a, Vector b) => new Vector(a.x + b.x, a.y + b.y);
        public static Vector operator -(Vector a, Vector b) => new Vector(a.x - b.x, a.y - b.y);
        public static Vector operator *(Vector a, double scalar) => new Vector(a.x * scalar, a.y * scalar);
        public static Vector operator /(Vector a, double scalar) => new Vector(a.x / scalar, a.y / scalar);

        public static double Dot(Vector a, Vector b) => a.x * b.x + a.y * b.y;
    }
}