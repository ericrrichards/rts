using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core {
    public struct Point : IEquatable<Point> {
        public bool Equals(Point other) {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Point && Equals((Point) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return (X*397) ^ Y;
            }
        }

        public static bool operator ==(Point left, Point right) {
            return left.Equals(right);
        }

        public static bool operator !=(Point left, Point right) {
            return !left.Equals(right);
        }

        public int X { get; set; }
        public int Y { get; set; }
        

        public Point(int x, int y) : this() {
            X = x;
            Y = y;
        }

        public static Point operator +( Point p1, Point p2) {
            return new Point(p1.X + p2.X, p1.Y + p2.Y);
        }
        public static Point operator +(Point p1, int i) {
            return new Point(p1.X + i, p1.Y + i);
        }
        public static Point operator -(Point p1, Point p2) {
            return new Point(p1.X - p2.X, p1.Y - p2.Y);
        }
        public static Point operator -(Point p1, int i) {
            return new Point(p1.X -i, p1.Y -i);
        }

        public static Point operator /(Point p, int rhs) {
            return new Point(p.X/rhs, p.Y/rhs);
        }
        public static Point operator /(Point p, Point p2) {
            return new Point(p.X / p2.X, p.Y / p2.Y);
        }
        public static Point operator *(Point p, int rhs) {
            return new Point(p.X * rhs, p.Y * rhs);
        }


        public float Distance(Point p2) {
            return (float) Math.Sqrt((X-p2.X)*(X-p2.X) + (Y-p2.Y)*(Y-p2.Y));
        }
        public bool InRect(Rectangle r) {
            return r.Contains(X, Y);
        }

    }
}

