using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Core;
using SlimDX;
using SlimDX.Direct3D9;
using SlimDX.DirectInput;
using Font = SlimDX.Direct3D9.Font;
using Point = Core.Point;
using ResultCode = SlimDX.Direct3D9.ResultCode;

namespace Hello {
    class HeightMapExample3 : App {
        private HeightMap _heightmap;
        private float _angle, _angleB;
        public override Result Init(int width, int height, bool windowed) {
            CreateWindow(width, height);
            _device = CreateDevice(width, height, windowed);
            if (_device == null) return ResultCode.Failure;

            _font = new Font(_device, 18, 0, FontWeight.Bold, 1, false, CharacterSet.Default, Precision.Default, FontQuality.Default, PitchAndFamily.Default | PitchAndFamily.DontCare, "Arial");

            _heightmap = new HeightMap(_device, new Point(50, 50)) {
                ShowSelection = true
            };
            if (_heightmap.CreateParticles().IsFailure) {
                Debug.Print("Failed to create particles");
                Quit();
            }

            IsRunning = true;


            return ResultCode.Success;
        }

        public override Result Update(float dt) {
            if (_heightmap != null) {
                var center = _heightmap.Center;
                var eye = new Vector3(center.X + MathF.Cos(_angle)*MathF.Cos(_angleB)*center.X*1.5f,
                                      MathF.Sin(_angleB)*_heightmap.MaxHeight*5.0f,
                                      -center.Y + MathF.Sin(_angle)*MathF.Cos(_angleB)*center.Y*1.5f);

                var lookAt = new Vector3(center.X, 0.0f, -center.Y);

                var world = Matrix.Identity;
                var view = Matrix.LookAtLH(eye, lookAt, new Vector3(0, 1, 0));
                var fov = MathF.Pi/180*45.0f;
                var proj = Matrix.PerspectiveFovLH(fov, 1.3333f, 1.0f, 1000.0f);

                _device.SetTransform(TransformState.World, world);
                _device.SetTransform(TransformState.View, view);
                _device.SetTransform(TransformState.Projection, proj);

                if (Input.IsKeyDown(Key.A) && _heightmap.SelectionRect.Left > 0) {
                    _heightmap.MoveRect(Direction.Left);
                }
                if (Input.IsKeyDown(Key.D) && _heightmap.SelectionRect.Right < _heightmap.Size.X -1) {
                    _heightmap.MoveRect(Direction.Right);
                }
                if (Input.IsKeyDown(Key.W) && _heightmap.SelectionRect.Top > 0) {
                    _heightmap.MoveRect(Direction.Up);
                }
                if (Input.IsKeyDown(Key.S) && _heightmap.SelectionRect.Bottom < _heightmap.Size.Y -1) {
                    _heightmap.MoveRect(Direction.Down);
                }
                if (Input.IsKeyDown(Key.NumberPadPlus)) {
                    _heightmap.RaiseTerrain(_heightmap.SelectionRect, dt * 3.0f);
                }
                if (Input.IsKeyDown(Key.NumberPadMinus)) {
                    _heightmap.RaiseTerrain(_heightmap.SelectionRect, -dt*3.0f);
                }

                if (Input.IsKeyDown(Key.Space)) {
                    _heightmap.SmoothTerrain();
                    Thread.Sleep(100);
                }
            }
            if (Input.IsKeyDown(Key.Escape)) {
                Quit();
                return ResultCode.Success;
            }
            if (Input.IsKeyDown(Key.DownArrow) && _angleB < MathF.Pi * 0.4f) {
                _angleB += dt*0.5f;
            }
            if (Input.IsKeyDown(Key.UpArrow) && _angleB > 0.1f) {
                _angleB -= dt * 0.5f;
            }
            if (Input.IsKeyDown(Key.LeftArrow) ) {
                _angle -= dt*0.5f;
            }
            if (Input.IsKeyDown(Key.RightArrow) ) {
                _angle += dt * 0.5f;
            }



            return ResultCode.Success;
        }

        public override Result Render() {
            _device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            if (_device.BeginScene().IsSuccess && _mainWindow != null) {
                if (_heightmap != null) {
                    _heightmap.Render();
                }

                var rs = new[] {
                    new Rectangle(10, 10, 0, 0),
                    new Rectangle(10, 30, 0,0),
                    new Rectangle(10, 50, 0,0),
                    new Rectangle(10, 70, 0,0), 
                };

                _font.DrawString(null, String.Format("Arrows: Move camera"), rs[0], DrawTextFormat.Left | DrawTextFormat.Top | DrawTextFormat.NoClip, Color.White);
                _font.DrawString(null, String.Format("WASD: Move selection"), rs[1], DrawTextFormat.Left | DrawTextFormat.Top | DrawTextFormat.NoClip, Color.White);
                _font.DrawString(null, String.Format("+/-: Raise/Lower terrain"), rs[2], DrawTextFormat.Left | DrawTextFormat.Top | DrawTextFormat.NoClip, Color.White);
                _font.DrawString(null, String.Format("Space: Smooth terrain"), rs[3], DrawTextFormat.Left | DrawTextFormat.Top | DrawTextFormat.NoClip, Color.White);
                _device.EndScene();
                _device.Present();
            }
            return ResultCode.Success;
        }

        public override Result Cleanup() {
            ReleaseCom(_font);
            ReleaseCom(_device);
            _heightmap.Release();
            Debug.Print("Application terminated");
            return ResultCode.Success;
        }

        public override string GetName() {
            return "Heightmap Editor";
        }
    }
}
