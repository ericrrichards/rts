using System;
using System.Drawing;
using Core;
using SlimDX;
using SlimDX.Direct3D9;
using SlimDX.DirectInput;
using Font = SlimDX.Direct3D9.Font;
using Point = Core.Point;
using ResultCode = SlimDX.Direct3D9.ResultCode;

namespace Hello {
    public class HelloD3D : App {
        


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

        public override Result Update(float dt) {
            if (Input.IsKeyDown(Key.Escape)) {
                Quit();
            }
            
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
