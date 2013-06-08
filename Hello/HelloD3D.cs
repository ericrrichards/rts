using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hello {
    using System.Diagnostics;
    using System.Drawing;
    using System.Windows.Forms;

    using Core;

    using SlimDX;
    using SlimDX.Direct3D9;

    using Font = SlimDX.Direct3D9.Font;

    internal class HelloD3D : App {
        private Font _font;

        private static void Main(string[] args) {
            var app = new HelloD3D(Process.GetCurrentProcess().Handle, "Hello Direct3D", DeviceType.Hardware, CreateFlags.HardwareVertexProcessing);
            App.GApp = app;
            app.Run();

        }

        public HelloD3D(IntPtr hinstance, string caption, DeviceType devType, CreateFlags requestedVP)
            : base(hinstance, caption, devType, requestedVP) {
            if (!CheckDeviceCaps()) {
                MessageBox.Show("CheckDeviceCaps faileds");
                Exit();
            }

            _font = new Font(GDevice, 80, 40, FontWeight.Bold, 0, true, CharacterSet.Default, Precision.Default, FontQuality.Default, PitchAndFamily.Default | PitchAndFamily.DontCare, "Times New Roman");

        }

        ~HelloD3D() {
            ReleaseCom(_font);
        }

        public override void OnLostDevice() {
            _font.OnLostDevice();
        }

        public override void OnResetDevice() {
            _font.OnResetDevice();
        }

        public override void UpdateScene(float dt) {
            
        }

        public override void DrawScene() {
            GDevice.Clear(ClearFlags.All, Color.Black, 1.0f, 0);
            var formatRect = MainWindow.ClientRectangle;


            GDevice.BeginScene();
            _font.DrawString(null, "Hello Direct3D", formatRect, DrawTextFormat.Center | DrawTextFormat.VerticalCenter, new Color4(1.0f,Rand.Next(256), Rand.Next(256), Rand.Next(256)));

            GDevice.EndScene();
            GDevice.Present();
        }
    }
}
