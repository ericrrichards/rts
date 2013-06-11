namespace Core {
    using System.IO;

    using SlimDX;

    public class Debug {
        private static readonly object SyncRoot = new object();
        private static readonly StreamWriter Out;

        static  Debug() {
            Out = new StreamWriter( new FileStream("out.txt", FileMode.OpenOrCreate));
        }

        public static void Print(string fmt, params object[] args) {
            lock (SyncRoot) {
                Out.WriteLine(fmt, args);
            }
        }
        public static void Endl(int nr = 1) {
            lock (SyncRoot) {
                for (int i = 0; i < nr; i++) {
                    Out.WriteLine();
                }
            }
            
        }
        public static void Write(Vector3 v) {
            lock (SyncRoot) {
                Out.Write("x: {0} y: {1} z:{2}", v.X, v.Y, v.Z);
            }
        }
        public static void Write(bool b) {
            lock (SyncRoot) {
                Out.Write(b ? "True" : "False");
            }
        }
        public static void Write(string s) {
            lock (SyncRoot) {
                Out.Write(s);
            }
        }
        public static void Write(float f) {
            lock (SyncRoot) {
                Out.Write(f);
            }
        }
        public static void Write(int i) {
            lock (SyncRoot) {
                Out.Write(i);
            }
        }

    }
}