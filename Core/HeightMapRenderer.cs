using System;
using System.Drawing;
using SlimDX;
using SlimDX.Direct3D9;

namespace Core {
    public class HeightMapRenderer : GameObject {
        private HeightMap _hm;

        // temp rendering stuff
        private VertexBuffer _vb;
        private readonly Device _device;
        private readonly Sprite _sprite;
        public bool ShowSelection { get; set; }
        public HeightMapEditorSelection Editor { get; set; }

        public HeightMapRenderer(HeightMap hm, Device device) {
            _device = device;
            _hm = hm;
            _vb = null;
            _sprite = new Sprite(device);
        }
        public void Release() {
            ReleaseCom(_vb);
            ReleaseCom(_sprite);
        }

        public Result CreateParticles() {
            try {
                ReleaseCom(_vb);
                _vb = new VertexBuffer(_device, _hm.Size.X * _hm.Size.Y * Particle.Size, Usage.Dynamic | Usage.Points | Usage.WriteOnly, Particle.FVF, Pool.Default);

                var ds = _vb.Lock(0, 0, LockFlags.Discard);
                for (var y = 0; y < _hm.Size.Y; y++) {
                    for (var x = 0; x < _hm.Size.X; x++) {
                        var prc = _hm[x + y * _hm.Size.X] / _hm.MaxHeight;
                        //Debug.Print("prc: {0}", prc);
                        var red = prc;
                        var green = 1.0f - prc;

                        bool contains = false;
                        if (Editor != null) {
                            contains = x >= Editor.SelectionRect.Left && x <= Editor.SelectionRect.Right && y >= Editor.SelectionRect.Top && y <= Editor.SelectionRect.Bottom;
                        }
                        var v = new Particle {
                            Position = new Vector3(x, _hm[x + y * _hm.Size.X], -y),
                            Color = (ShowSelection && contains) ? new Color4(1.0f, 0, 0, 1.0f).ToArgb() : new Color4(1.0f, red, green, 0.0f).ToArgb()
                        };
                        ds.Write(v);

                    }
                }
                _vb.Unlock();
            } catch (Exception ex) {
                Debug.Print("Error in {0} - {1}\n{2}", ex.TargetSite, ex.Message, ex.StackTrace);
                return ResultCode.Failure;
            }
            return ResultCode.Success;
        }
        public void Render() {
            try {
                if (_vb != null) {
                    _device.SetRenderState(RenderState.Lighting, false);
                    _device.SetRenderState(RenderState.PointSpriteEnable, true);
                    _device.SetRenderState(RenderState.PointScaleEnable, true);

                    _device.SetRenderState(RenderState.PointSize, 0.7f);
                    _device.SetRenderState(RenderState.PointSizeMin, 0.0f);
                    _device.SetRenderState(RenderState.PointScaleA, 0);
                    _device.SetRenderState(RenderState.PointScaleB, 0.0f);
                    _device.SetRenderState(RenderState.PointScaleC, 1.0f);
                    _device.SetRenderState(RenderState.ZWriteEnable, true);

                    _device.SetTexture(0, null);
                    _device.VertexFormat = Particle.FVF;
                    _device.SetStreamSource(0, _vb, 0, Particle.Size);
                    _device.DrawPrimitives(PrimitiveType.PointList, 0, _hm.Size.X * _hm.Size.Y);
                }
                if (_sprite != null && _hm.HeightMapTexture != null) {
                    _sprite.Begin(SpriteFlags.None);
                    _sprite.Draw(_hm.HeightMapTexture, null, null, new Vector3(1.0f, 1.0f, 1.0f), Color.White);
                    _sprite.End();
                }
            } catch (Exception ex) {
                Debug.Print("Error in {0} - {1}\n{2}", ex.TargetSite, ex.Message, ex.StackTrace);
            }
        }
    }
}