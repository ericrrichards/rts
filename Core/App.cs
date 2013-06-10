using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core {
    using System.Drawing;
    using System.Threading;
    using System.Windows.Forms;

    using SlimDX;
    using SlimDX.Direct3D9;

    public class Program {
        public static void Main(string[] args) {
            
        }
    }

    public class App {
        private Device _device;
        private SlimDX.Direct3D9.Font _font;
        private Form _mainWindow;
        
        public App() {
            
        }
        public void Init(int width, int height, bool windowed) {
            
        }
        public void Update(float dt) {
            
        }
        public void Render() {
            
        }
        public void Cleanup() {
            
        }
        public void Quit() {
            
        }

        protected static void ReleaseCom(ComObject o) {
            if (o != null) {
                o.Dispose();
                o = null;
            }
        }
    }
}
