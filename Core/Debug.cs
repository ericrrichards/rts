using System;

namespace Core {
    using System.IO;

    using SlimDX;

    public class Debug {
        private static readonly object SyncRoot = new object();
        private static readonly StreamWriter Out;

        static  Debug() {
            Out = new StreamWriter( new FileStream("out.txt", FileMode.OpenOrCreate)) {AutoFlush = true};
        }

        public static void Print(string fmt, params object[] args) {
            Console.WriteLine(fmt, args);
            lock (SyncRoot) {
                Out.WriteLine(fmt, args);
            }
        }
        public static void Endl(int nr = 1) {
            lock (SyncRoot) {
                for (int i = 0; i < nr; i++) {
                    Out.WriteLine();
                    Console.WriteLine();
                }
            }
            
        }
        public static void Write(Vector3 v) {
            lock (SyncRoot) {
                Out.Write("x: {0} y: {1} z:{2}", v.X, v.Y, v.Z);
                Console.Write("x: {0} y: {1} z:{2}", v.X, v.Y, v.Z);
            }
        }
        public static void Write(bool b) {
            lock (SyncRoot) {
                Out.Write(b ? "True" : "False");
                Console.Write(b ? "True" : "False");
            }
        }
        public static void Write(string s) {
            lock (SyncRoot) {
                Out.Write(s);
                Console.Write(s);
            }
        }
        public static void Write(float f) {
            lock (SyncRoot) {
                Out.Write(f);
                Console.Write(f);
            }
        }
        public static void Write(int i) {
            lock (SyncRoot) {
                Out.Write(i);
                Console.Write(i);
            }
        }

    }
}