using System;

namespace Core {
    public static class MathF {
        private static Random _rand = new Random();
        public static float Sin(float a) {
            return (float) Math.Sin(a);
        }
        public static float Cos(float a) {
            return (float)Math.Cos(a);
        }

        public static float Pi {
            get { return (float) Math.PI; }
        }

        public static float Noise(int x) {
            x = (x << 13) ^ x;
            return (1.0f - ((x*(x*x*15731 + 789221) + 1376312589) & 0x7fffffff)/1073741824.0f);
        }
        public static float CosInterpolate(float v1, float v2, float a) {
            var angle = a*Pi;
            var prc = (1.0f - Cos(angle))*0.5f;
            return v1*(1.0f - prc) + v2*prc;
        }

        public static float Pow(float x, int y) {
            return (float) Math.Pow(x, y);
        }
        public static int Rand(int range = int.MaxValue) {
            return _rand.Next(range);
        }
    }
}