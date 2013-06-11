using SlimDX;

namespace Core {
    public class GameObject {
        protected static void ReleaseCom(ComObject o) {
            if (o == null) return;
            o.Dispose();
            o = null;
        }
    }
}