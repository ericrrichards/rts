using System;
using System.Drawing;
using Core;
using SlimDX;
using SlimDX.Direct3D9;
using SlimDX.DirectInput;
using Font = SlimDX.Direct3D9.Font;
using ResultCode = SlimDX.Direct3D9.ResultCode;

namespace Hello {
    public class HelloD3D : App {
        


        public override string GetName() {
            return "Hello D3D";
        }
        

        public override Result Init(int width, int height, bool windowed) {
            CreateWindow(width, height);

            Device = CreateDevice(width, height, windowed);
            if (Device == null) return ResultCode.Failure;

            Font = new Font(Device, 48, 0, FontWeight.Bold, 1, false, CharacterSet.Default, Precision.Default, FontQuality.Default, PitchAndFamily.Default | PitchAndFamily.DontCare, "Arial");

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
            Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);

            if (Device.BeginScene().IsSuccess && MainWindow != null) {
                var r = MainWindow.ClientRectangle;

                Font.DrawString(null, "Hello World!", r, DrawTextFormat.Center | DrawTextFormat.NoClip | DrawTextFormat.VerticalCenter, Color.White);

                Device.EndScene();
                Device.Present();
            }
            return ResultCode.Success;
        }
        public override Result Cleanup() {
            try {
                ReleaseCom(Font);
                ReleaseCom(Device);

                Debug.Print("Application terminated");
            } catch (Exception ex) {
                Debug.Print("Exception in Cleanup - {0}\n{1}", ex.Message, ex.StackTrace);
                return ResultCode.Failure;
            }
            return ResultCode.Success;

        }
    }
    
}
