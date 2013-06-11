using System;

namespace Core {
    using System.Diagnostics;
    using System.Drawing;
    using System.Windows.Forms;

    using SlimDX;
    using SlimDX.Direct3D9;

    using Device = SlimDX.Direct3D9.Device;
    using DeviceType = SlimDX.Direct3D9.DeviceType;
    using Font = SlimDX.Direct3D9.Font;
    using ResultCode = SlimDX.Direct3D9.ResultCode;

    

    public class App {
        private Device _device;
        private Font _font;
        private Form _mainWindow;
        
        public App() {
            _mainWindow = null;
            _device = null;
            _font = null;
        }

        public bool IsRunning { get; private set; }

        public Result Init(int width, int height, bool windowed) {
            Debug.Print("Application initiated");
            _mainWindow = new Form { Name = "D3DWND", Width = width, Height = height };
            _mainWindow.KeyDown += (sender, args) => {
                if (args.KeyCode == Keys.Escape) {
                    Quit();
                }
            };

            _mainWindow.Show();
            _mainWindow.Update();

            Direct3D d3D9;
            try {
                d3D9 = new Direct3D();
            } catch (Exception ex) {
                Debug.Print("Direct3D Creation Failed - {0}", ex.Message);
                return ResultCode.Failure;
            }

            var caps = d3D9.GetDeviceCaps(0, DeviceType.Hardware);
            var vp = CreateFlags.None;
            if (caps.DeviceCaps.HasFlag(DeviceCaps.HWTransformAndLight)) {
                vp = CreateFlags.HardwareVertexProcessing;
            } else {
                vp = CreateFlags.SoftwareVertexProcessing;
            }

            if (caps.VertexShaderVersion < new Version(2, 0) || caps.PixelShaderVersion < new Version(2, 0)) {
                Debug.Print("Warning - Your graphic card does not support vertex and pixel shaders version 2.0");
            }

            var pp = new PresentParameters {
                BackBufferWidth = width,
                BackBufferHeight = height,
                BackBufferFormat = Format.A8R8G8B8,
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
            try {
                _device = new Device(d3D9, 0, DeviceType.Hardware, _mainWindow.Handle, vp, pp);
            } catch (Exception ex) {
                Debug.Print("Failed to create Device - {0}", ex.Message);
                return ResultCode.Failure;
            }
            d3D9.Dispose();

            _font = new Font(_device, 48, 0, FontWeight.Bold, 1, false, CharacterSet.Default, Precision.Default, FontQuality.Default, PitchAndFamily.Default | PitchAndFamily.DontCare, "Arial");

            IsRunning = true;

            return ResultCode.Success;
        }
        public Result Update(float dt) {
            return ResultCode.Success;
        }
        public Result Render() {
            _device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);

            if (_device.BeginScene().IsSuccess) {
                var r = _mainWindow.ClientRectangle;

                _font.DrawString(null, "Hello World!", r, DrawTextFormat.Center | DrawTextFormat.NoClip | DrawTextFormat.VerticalCenter, Color.White);

                _device.EndScene();
                _device.Present();
            }
            return ResultCode.Success;
        }
        public Result Cleanup() {
            try {
                ReleaseCom(_font);
                ReleaseCom(_device);

                Debug.Print("Application terminated");
            }catch (Exception ex) {
                Debug.Print("Exception in Cleanup - {0}\n{1}", ex.Message, ex.StackTrace );
                return ResultCode.Failure;
            }
            return ResultCode.Success;

        }
        public void Quit() {
            IsRunning = false;
            _mainWindow.Close();
            Application.Exit();
        }

        protected static void ReleaseCom(ComObject o) {
            if (o != null) {
                o.Dispose();
                o = null;
            }
        }
    }
}
