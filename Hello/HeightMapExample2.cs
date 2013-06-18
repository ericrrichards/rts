using System;
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
    class HeightMapExample2 : App {
        private float _angle;
        private int _size, _amplitude;
        private HeightMap _heightmap;
        public HeightMapExample2() {
            _heightmap = null;
            _angle = 0.0f;
            _size = 10;
            _amplitude = 5;
        }
        public override Result Init(int width, int height, bool windowed) {
            CreateWindow(width, height);
            Device = CreateDevice(width, height, windowed);
            if (Device == null) return ResultCode.Failure;

            Font = new Font(Device, 18, 0, FontWeight.Bold, 1, false, CharacterSet.Default, Precision.Default, FontQuality.Default, PitchAndFamily.Default | PitchAndFamily.DontCare, "Arial");

            _heightmap = new HeightMap(Device, new Point(100, 100));
            if (_heightmap.CreateRandomHeightMap(MathF.Rand(2000), _size / 10.0f, _amplitude / 10.0f, 9).IsFailure) {
                Debug.Print("Failed to create random heightmap");
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
                _angle += dt * 0.5f;
                var center = _heightmap.Center;
                var eye = new Vector3(center.X + MathF.Cos(_angle) * center.X * 2.0f, _heightmap.MaxHeight * 8.0f, -center.Y + MathF.Sin(_angle) * center.Y * 2.0f);
                var lookAt = new Vector3(center.X, 0.0f, -center.Y);

                var world = Matrix.Identity;
                var view = Matrix.LookAtLH(eye, lookAt, new Vector3(0, 1, 0));
                var proj = Matrix.PerspectiveFovLH(MathF.Pi / 4, 1.3333f, 1.0f, 1000.0f);

                Device.SetTransform(TransformState.World, world);
                Device.SetTransform(TransformState.View, view);
                Device.SetTransform(TransformState.Projection, proj);

                if (Input.IsKeyDown(Key.Space)) {
                    _heightmap.CreateRandomHeightMap(MathF.Rand(2000), _size/10.0f, _amplitude/10.0f, 9);
                    _heightmap.CreateParticles();
                    Thread.Sleep(100);
                }
            }
            if (Input.IsKeyDown(Key.Escape)) {
                Quit();
                return ResultCode.Success;
            }
            if (Input.IsKeyDown(Key.DownArrow) && _size > 1) {
                _size--; Thread.Sleep(100);
            }
            if (Input.IsKeyDown(Key.UpArrow) && _size <20) {
                _size++; Thread.Sleep(100);
            }
            if (Input.IsKeyDown(Key.LeftArrow) && _amplitude > 1) {
                _amplitude--; Thread.Sleep(100);
            }
            if (Input.IsKeyDown(Key.RightArrow) && _amplitude < 15) {
                _amplitude++; Thread.Sleep(100);
            }
            return ResultCode.Success;
        }

        public override Result Render() {
            Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            if (Device.BeginScene().IsSuccess && MainWindow != null) {
                if (_heightmap != null) {
                    _heightmap.Render();
                }

                Font.DrawString(null, String.Format("Size: {0} \t(UP/DOWN Arrow)", _size), new Rectangle(110, 10, 0, 0), DrawTextFormat.Left | DrawTextFormat.Top | DrawTextFormat.NoClip, Color.White);
                Font.DrawString(null, String.Format("Persistence: {0} \t(Left/Right Arrow)", _amplitude), new Rectangle(110, 30, 0, 0), DrawTextFormat.Left | DrawTextFormat.Top | DrawTextFormat.NoClip, Color.White);
                Font.DrawString(null, String.Format("Redraw: (SPACE)"), new Rectangle(110, 50, 0, 0), DrawTextFormat.Left | DrawTextFormat.Top | DrawTextFormat.NoClip, Color.White);
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
            return "Random heightmap example";
        }
    }
}
