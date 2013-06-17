using System;
using System.Diagnostics;
using System.Drawing;
using SlimDX.Direct3D9;
using SlimDX.DirectInput;
using Device = SlimDX.Direct3D9.Device;
using DeviceType = SlimDX.Direct3D9.DeviceType;
using Font = SlimDX.Direct3D9.Font;

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
            Input.Destroy();
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
            
            

            _mainWindow.Show();
            _mainWindow.Update();
            Input.Init(_mainWindow);
        }

        public void Main(string[] args) {
            Configuration.EnableObjectTracking = true;

            if (Init(800, 600, true).IsFailure) {
                return;
            }
            var startTime = Stopwatch.GetTimestamp();
            while (IsRunning) {

                Application.DoEvents();

                var t = Stopwatch.GetTimestamp();

                var dt = (t- startTime)/ (float)Stopwatch.Frequency;

                Update(dt);
                lock (syncRoot) {
                    Render();
                }
                startTime = t;
            }
            Cleanup();
        }

        protected Device CreateDevice(int width, int height, bool windowed) {
            var d3D9 = CreateDirect3D();
            if (d3D9 == null) {
                return null;
            }
            var caps = d3D9.GetDeviceCaps(0, DeviceType.Hardware);
            var vp = CreateFlags.Multithreaded | (caps.DeviceCaps.HasFlag(DeviceCaps.HWTransformAndLight) ?
                                 CreateFlags.HardwareVertexProcessing  :
                                 CreateFlags.SoftwareVertexProcessing);

            if (caps.VertexShaderVersion < new Version(2, 0) || caps.PixelShaderVersion < new Version(2, 0)) {
                Debug.Print("Warning - Your graphic card does not support vertex and pixel shaders version 2.0");
            }

            var format = (windowed) ? Format.Unknown : Format.A8R8G8B8;
            var pp = new PresentParameters {
                BackBufferWidth = width,
                BackBufferHeight = height,
                BackBufferFormat = format,
                BackBufferCount = 1,
                Multisample = MultisampleType.None,
                MultisampleQuality = 0,
                SwapEffect = SwapEffect.Discard,
                DeviceWindowHandle = _mainWindow.Handle,
                Windowed = windowed,
                EnableAutoDepthStencil = true,
                AutoDepthStencilFormat = Format.D24S8,
                PresentFlags = PresentFlags.None,
                FullScreenRefreshRateInHertz = 0,
                PresentationInterval = PresentInterval.Immediate
            };
            Device device = null;
            try {
                device = new Device(d3D9, 0, DeviceType.Hardware, _mainWindow.Handle, vp, pp);
            } catch (Exception ex) {
                Debug.Print("Failed to create Device - {0}", ex.Message);
            }
            d3D9.Dispose();
            System.Diagnostics.Debug.Assert(device != null, "device != null");
            System.Diagnostics.Debug.Assert(device.ComPointer != IntPtr.Zero);
            try {
                device.GetRenderState(RenderState.Lighting);

            } catch (Exception ex) {
                Debug.Print("Error in {0} - {1}\n{2}", ex.TargetSite, ex.Message, ex.StackTrace);
                return null;
            }
            device.Material = new Material() {
                Diffuse = Color.White
            };

            return device;
        }
    }
}
