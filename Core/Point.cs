using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX;

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

    public class Debug {
        private static volatile Debug _debug;
        private static readonly object SyncRoot = new object();
        public static Debug Logger{
            get {
                if (_debug == null) {
                    lock (SyncRoot) {
                        if (_debug == null) {
                            _debug = new Debug();
                        }
                    }
                }
                return _debug;
            }

        }
        private readonly StreamWriter _out;

        private Debug() {
            _out = new StreamWriter( new FileStream("out.txt", FileMode.OpenOrCreate));
        }
        ~Debug() {
            if (_out != null) {
                _out.Close();
            }
        }

        public void Print(string fmt, params object[] args) {
            _out.WriteLine(fmt, args);
        }
        public void Endl(int nr = 1) {
            for (int i = 0; i < nr; i++) {
                _out.WriteLine();
            }
        }
        public void Write(Vector3 v) {
            _out.Write("x: {0} y: {1} z:{2}", v.X, v.Y, v.Z);
        }
        public void Write(bool b) {
            _out.Write(b? "True":"False");
        }
        public void Write(string s) {
            _out.Write(s);
        }
        public void Write(float f) {
            _out.Write(f);
        }
        public void Write(int i) {
            _out.Write(i);
        }

    }
}

