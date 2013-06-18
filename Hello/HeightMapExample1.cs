using System.Drawing;
using System.Threading;

using Core;
using SlimDX;
using SlimDX.Direct3D9;
using SlimDX.DirectInput;
using Font = SlimDX.Direct3D9.Font;
using Point = Core.Point;
using ResultCode = SlimDX.Direct3D9.ResultCode;

namespace Hello {
    class HeightMapExample1 :App {
        private float _angle;
        private int _image;
        private HeightMap _heightmap;

        public HeightMapExample1() {
            _angle = 0.0f;
            _image = 0;
        }

        public override Result Init(int width, int height, bool windowed) {
            CreateWindow(width, height);
            Device = CreateDevice(width, height, windowed);
            if (Device == null) return ResultCode.Failure;

            Font = new Font(Device, 18, 0, FontWeight.Bold, 1, false, CharacterSet.Default, Precision.Default, FontQuality.Default, PitchAndFamily.Default | PitchAndFamily.DontCare, "Arial");

            _heightmap = new HeightMap(Device, new Point(100, 100));
            if (_heightmap.LoadFromFile("images/abe.jpg").IsFailure) {
                Debug.Print("failed to load from file");
                Quit();
            }
            if (_heightmap.CreateParticles().IsFailure) {
                Debug.Print("Failed to create particles");
                Quit();
            }

            IsRunning = true;

            return ResultCode.Success;
        }

        public override Result Update(float dt) {
            if (_heightmap == null) {
                
            } else {
                _angle += dt*0.5f;
                var center = _heightmap.Center;
                var eye = new Vector3(center.X + MathF.Cos(_angle)*center.X*2.0f, _heightmap.MaxHeight*8.0f, -center.Y + MathF.Sin(_angle)*center.Y*2.0f);
                var lookAt = new Vector3(center.X, 0.0f, -center.Y);

                var world = Matrix.Identity;
                var view = Matrix.LookAtLH(eye, lookAt, new Vector3(0, 1, 0));
                var proj = Matrix.PerspectiveFovLH(MathF.Pi/4, 1.3333f, 1.0f, 1000.0f);

                Device.SetTransform(TransformState.World, world);
                Device.SetTransform(TransformState.View, view);
                Device.SetTransform(TransformState.Projection, proj);

                if (Input.IsKeyDown(Key.Space)) {
                    _image++;
                    if (_image > 2) _image = 0;
                    switch (_image) {
                        case 0:
                            _heightmap.LoadFromFile("images/abe.jpg");
                            break;
                        case 1:
                            _heightmap.LoadFromFile("images/smiley.bmp");
                            break;
                        case 2:
                            _heightmap.LoadFromFile("images/heightmap.jpg");
                            break;
                    }
                    _heightmap.CreateParticles();
                    Thread.Sleep(300);
                }
            }
            if (Input.IsKeyDown(Key.Escape)) {
                Quit();
            }
            return ResultCode.Success;
        }

        public override Result Render() {
            Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);

            if (Device.BeginScene().IsSuccess && MainWindow != null) {

                if (_heightmap != null) _heightmap.Render();

                var r = MainWindow.ClientRectangle;

                Font.DrawString(null, "Space: Change Image", r, DrawTextFormat.Left| DrawTextFormat.NoClip | DrawTextFormat.Top, Color.White);

                Device.EndScene();
                Device.Present();
            }
            return ResultCode.Success;
        }

        public override Result Cleanup() {
            ReleaseCom(Font);
            ReleaseCom(Device);
            _heightmap.Release();
            Debug.Print("Application terminated");
            return ResultCode.Success;
        }

        public override string GetName() {
            return "HeightMap Example 1";
        }
    }
}
