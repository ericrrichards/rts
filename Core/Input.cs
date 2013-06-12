using System.Windows.Forms;
using SlimDX.DirectInput;

namespace Core {
    public static class Input {
        private static Keyboard _keyboard;
        private static Mouse _mouse;

        public static void Init(Form mainWindow) {

            var dinput = new DirectInput();
            _keyboard = new Keyboard(dinput);
            _keyboard.Acquire();
            _mouse = new Mouse(dinput);
            _mouse.Acquire();
            dinput.Dispose();
            dinput = null;
        }

        public static void Destroy() {
            if (_keyboard != null) {
                _keyboard.Dispose();
                _keyboard = null;
            }
            if (_mouse != null) {
                _mouse.Dispose();
                _mouse = null;
            }
        }
        public static bool IsKeyDown(Key k) {
            var ks = _keyboard.GetCurrentState();
            return ks.IsPressed(k);
        }
        public static bool IsMouseDown(int b) {
            var ms = _mouse.GetCurrentState();
            return ms.IsPressed(b);
        }
        public static Point GetMousePosition() {
            var ms = _mouse.GetCurrentState();
            return new Point(ms.X, ms.Y);
        }
        public static int GetMouseWheelState() {
            var ms = _mouse.GetCurrentState();
            return ms.Z;
        }


    }
}