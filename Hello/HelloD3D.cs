using System;
using System.Drawing;
using Core;
using SlimDX;
using SlimDX.Direct3D9;
using Font = SlimDX.Direct3D9.Font;

namespace Hello {
    using System.Diagnostics;
    using System.Windows.Forms;


    public class HelloD3D : App {
        public HelloD3D(){}

    public override string GetName() {
            return "Hello D3D";
        }
        

        public override Result Init(int width, int height, bool windowed) {
            CreateWindow(width, height);

            _device = CreateDevice(width, height, windowed);
            if (_device == null) return ResultCode.Failure;

            _font = new Font(_device, 48, 0, FontWeight.Bold, 1, false, CharacterSet.Default, Precision.Default, FontQuality.Default, PitchAndFamily.Default | PitchAndFamily.DontCare, "Arial");

            IsRunning = true;

            return ResultCode.Success;
        }

        protected Device CreateDevice(int width, int height, bool windowed) {
            var d3D9 = CreateDirect3D();
            if (d3D9 == null) {
                return null;
            }
            var caps = d3D9.GetDeviceCaps(0, DeviceType.Hardware);
            CreateFlags vp = caps.DeviceCaps.HasFlag(DeviceCaps.HWTransformAndLight) ?
                                 CreateFlags.HardwareVertexProcessing :
                                 CreateFlags.SoftwareVertexProcessing;

            if (caps.VertexShaderVersion < new Version(2, 0) || caps.PixelShaderVersion < new Version(2, 0)) {
                Core.Debug.Print("Warning - Your graphic card does not support vertex and pixel shaders version 2.0");
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
            Device device = null;
            try {
                device = new Device(d3D9, 0, DeviceType.Hardware, _mainWindow.Handle, vp, pp);
            } catch (Exception ex) {
                Core.Debug.Print("Failed to create Device - {0}", ex.Message);
            }
            d3D9.Dispose();
            return device;
        }

        public override Result Update(float dt) {
            return ResultCode.Success;
        }
        public override Result Render() {
            _device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);

            if (_device.BeginScene().IsSuccess && _mainWindow != null) {
                var r = _mainWindow.ClientRectangle;

                _font.DrawString(null, "Hello World!", r, DrawTextFormat.Center | DrawTextFormat.NoClip | DrawTextFormat.VerticalCenter, Color.White);

                _device.EndScene();
                _device.Present();
            }
            return ResultCode.Success;
        }
        public override Result Cleanup() {
            try {
                ReleaseCom(_font);
                ReleaseCom(_device);

                Core.Debug.Print("Application terminated");
            } catch (Exception ex) {
                Core.Debug.Print("Exception in Cleanup - {0}\n{1}", ex.Message, ex.StackTrace);
                return ResultCode.Failure;
            }
            return ResultCode.Success;

        }
    }
    
}
