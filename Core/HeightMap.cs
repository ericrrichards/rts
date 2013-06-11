using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX;
using SlimDX.Direct3D9;

namespace Core {
    internal struct Particle {
        public Vector3 Position;
        public Color4 Color;
        public static readonly VertexFormat FVF = VertexFormat.Position | VertexFormat.Diffuse;
        public const int Size = 28;
    }


    class HeightMap :GameObject{
        private Point _size;
        private float _maxHeight;
        private float[] _heightMap;

        // temp rendering stuff
        private VertexBuffer _vb;
        private Device _device;
        private Sprite _sprite;
        private Texture _heightMapTexture;

        public HeightMap(Device device, Point size) {
            try {
                _device = device;
                _size = size;

                _sprite = new Sprite(_device);

                _maxHeight = 15.0f;

                _heightMap = new float[_size.X*_size.Y];

                _vb = null;
                _heightMapTexture = null;
            } catch (Exception ex) {
                Debug.Print("Error in {0} - {1}\n{2}", ex.TargetSite, ex.Message, ex.StackTrace);
            }
        }
        ~HeightMap() {
            Release();
        }
        public void Release() {
            _heightMap = null;
            ReleaseCom(_vb);
            ReleaseCom(_sprite);
            ReleaseCom(_heightMapTexture);
        }
        public Result LoadFromFile(string filename) {
            try {
                _heightMap = new float[_size.X*_size.Y];

                ReleaseCom(_heightMapTexture);

                _heightMapTexture = Texture.FromFile(_device, filename);

                var dr = _heightMapTexture.LockRectangle(0, LockFlags.None);
                for (int y = 0; y < _size.Y; y++) {
                    for (int x = 0; x < _size.X; x++) {
                        var c = dr.Data.Read<Color4>();
                        _heightMap[x + y*_size.X] = c.Red*_maxHeight;
                    }
                }
                _heightMapTexture.UnlockRectangle(0);
            } catch (Exception ex) {
                Debug.Print("Error in {0} - {1}\n{2}", ex.TargetSite, ex.Message, ex.StackTrace);
                return ResultCode.Failure;
            }
            return ResultCode.Success;
        }
        public Result CreateParticles() {
            try {
                ReleaseCom(_vb);
                _vb = new VertexBuffer(_device, _size.X*_size.Y*Particle.Size, Usage.Dynamic | Usage.Points | Usage.WriteOnly, Particle.FVF, Pool.Default);

                var ds = _vb.Lock(0, 0, LockFlags.Discard);
                for (int y = 0; y < _size.Y; y++) {
                    for (int x = 0; x < _size.X; x++) {
                        var prc = _heightMap[x + y*_size.X]/_maxHeight;
                        var red = prc;
                        var green = 1.0f - prc;

                        var v = new Particle() {
                            Color = new Color4(red, green, 0),
                            Position = new Vector3(x, _heightMap[x+y*_size.X], -y)
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
                    _device.SetRenderState(RenderState.PointScaleEnable, true);
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
                    _device.DrawPrimitives(PrimitiveType.PointList, 0, _size.X*_size.Y);
                }
                if (_sprite != null) {
                    _sprite.Begin(SpriteFlags.None);
                    _sprite.Draw(_heightMapTexture, Color.White);
                    _sprite.End();
                }
            } catch (Exception ex) {
                Debug.Print("Error in {0} - {1}\n{2}", ex.TargetSite, ex.Message, ex.StackTrace);
            }
        }
        public Vector2 Center { get { return new Vector2(_size.X /2.0f, _size.Y / 2.0f);}}

    }
}
