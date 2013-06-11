using System;
using System.Diagnostics;
using SlimDX.Direct3D9;

namespace Core {
    using System.Windows.Forms;

    using SlimDX;
    using Device = Device;
    using Font = Font;


    public abstract class App : GameObject {
        private readonly object syncRoot = new object();
        protected Device _device;
        protected Font _font;
        protected Form _mainWindow;

        protected App() {
            _mainWindow = null;
            _device = null;
            _font = null;
        }

        protected bool IsRunning { get; set; }

        // framework methods
        public abstract Result Init(int width, int height, bool windowed);
        public abstract Result Update(float dt);
        public abstract Result Render();
        public abstract Result Cleanup();

        public void Quit() {
                IsRunning = false;
                _mainWindow.Close();
                _mainWindow = null;
            //Application.Exit();
        }

        protected static Direct3D CreateDirect3D() {
            Direct3D d3D9 = null;
            try {
                d3D9 = new Direct3D();
            } catch (Exception ex) {
                Debug.Print("Direct3D Creation Failed - {0}", ex.Message);
            }
            return d3D9;
        }

        public abstract string GetName();

        protected void CreateWindow(int width, int height) {
            Debug.Print("Application initiated");
            _mainWindow = new Form {
                Width = width,
                Height = height,
                Text = GetName(),
                FormBorderStyle = FormBorderStyle.None
            };
            _mainWindow.KeyDown += (sender, args) => {
                if (args.KeyCode == Keys.Escape) {
                    Quit();
                }
            };
            

            _mainWindow.Show();
            _mainWindow.Update();
        }

        public void Main(string[] args) {
            

            if (Init(800, 600, true).IsFailure) {
                return;
            }
            var timer = new Stopwatch();
            timer.Start();
            while (IsRunning) {
                
                    Application.DoEvents();
                    var dt = timer.ElapsedMilliseconds*0.001f;
                
                    Update(dt);
                lock (syncRoot) {
                    Render();
                }
                timer.Restart();
            }
            Cleanup();
        }
    }
}
