using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

using SlimDX;
using SlimDX.Direct3D9;

namespace Core {
    using System.Threading;

    [StructLayout(LayoutKind.Sequential)]
    internal struct Particle {
        public Vector3 Position;
        public int Color;
        public static readonly VertexFormat FVF = VertexFormat.Position | VertexFormat.Diffuse;
        public const int Size = 16;
    }


    public class HeightMap :GameObject{
        private Point _size;
        private float _maxHeight;
        private float[] _heightMap;
        private Rectangle _selectRect;

        // temp rendering stuff
        private VertexBuffer _vb;
        private readonly Device _device;
        private readonly Sprite _sprite;
        private Texture _heightMapTexture;

        public HeightMap(Device device, Point size) {
            System.Diagnostics.Debug.Assert(device != null);
            try {
                _device = device;
                _size = size;

                _sprite = new Sprite(_device);

                _maxHeight = 15.0f;

                _heightMap = new float[_size.X*_size.Y];

                _selectRect = new Rectangle(_size.X/ 2 - 5,_size.Y / 2 - 5,10,10);
                
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

                _heightMapTexture = Texture.FromFile(_device, filename,_size.X, _size.Y, 1, Usage.Dynamic, Format.L8, Pool.Default, Filter.Default, Filter.Default, 0);

                var dr = _heightMapTexture.LockRectangle(0, LockFlags.None);
                for (var y = 0; y < _size.Y; y++) {
                    for (var x = 0; x < _size.X; x++) {
                        dr.Data.Seek(y*dr.Pitch + x, SeekOrigin.Begin);
                        
                        var b = dr.Data.Read<byte>();
                        _heightMap[x + y*_size.X] = b/255.0f*_maxHeight;
                    }
                }
                _heightMapTexture.UnlockRectangle(0);
            } catch (Exception ex) {
                Debug.Print("Error in {0} - {1}\n{2}", ex.TargetSite, ex.Message, ex.StackTrace);
                return ResultCode.Failure;
            }
            return ResultCode.Success;
        }
        public Result CreateRandomHeightMap(int seed, float noiseSize, float persistence, int octaves) {
            ReleaseCom(_heightMapTexture);
            _heightMapTexture = new Texture(_device, _size.X, _size.Y, 1, Usage.Dynamic, Format.L8, Pool.Default);

            var dr = _heightMapTexture.LockRectangle(0, LockFlags.None);
            for (int y = 0; y < _size.Y; y++) {
                for (int x = 0; x < _size.X; x++) {
                    var xf = (x/(float) _size.X)*noiseSize;
                    var yf = (y/(float) _size.Y)*noiseSize;
                    var total = 0.0f;

                    for (int i = 0; i < octaves; i++) {
                        var freq = MathF.Pow(2.0f, i);
                        var amp = MathF.Pow(persistence, i);

                        var tx = xf*freq;
                        var ty = yf*freq;
                        var txi = (int) tx;
                        var tyi = (int) ty;
                        var fracX = tx -txi;
                        var fracY = ty - tyi;

                        var v1 = MathF.Noise(txi + tyi*57 + seed);
                        var v2 = MathF.Noise(txi + 1 + tyi*57 + seed);
                        var v3 = MathF.Noise(txi + (tyi+1) * 57 + seed);
                        var v4 = MathF.Noise(txi+1 + (tyi + 1) * 57 + seed);

                        var i1 = MathF.CosInterpolate(v1, v2, fracX);
                        var i2 = MathF.CosInterpolate(v3, v4, fracX);
                        total += MathF.CosInterpolate(i1, i2, fracY)*amp;
                    }
                    var b = (int) (128 + total*128.0f);
                    if (b < 0) b = 0;
                    if (b > 255) b = 255;
                    dr.Data.Seek(y*dr.Pitch + x, SeekOrigin.Begin);
                    dr.Data.Write((byte) b);

                    _heightMap[x + y*_size.X] = (b/255.0f)*_maxHeight;
                }
            }
            _heightMapTexture.UnlockRectangle(0);
            return ResultCode.Success;
        }
        public Result CreateParticles() {
            try {
                ReleaseCom(_vb);
                _vb = new VertexBuffer(_device, _size.X*_size.Y*Particle.Size, Usage.Dynamic | Usage.Points | Usage.WriteOnly, Particle.FVF, Pool.Default);
                
                var ds = _vb.Lock(0, 0, LockFlags.Discard);
                for (var y = 0; y < _size.Y; y++) {
                    for (var x = 0; x < _size.X; x++) {
                        var prc = _heightMap[x + y*_size.X]/_maxHeight;
                        //Debug.Print("prc: {0}", prc);
                        var red = prc;
                        var green =1.0f - prc;

                        bool contains = x >= _selectRect.Left && x <= _selectRect.Right && y >= _selectRect.Top && y <= _selectRect.Bottom;
                        var v = new Particle {
                            Position = new Vector3(x, _heightMap[x+y*_size.X], -y),
                            Color = (ShowSelection && contains) ? new Color4(1.0f, 0,0,1.0f).ToArgb() : new Color4(1.0f, red, green, 0.0f).ToArgb()
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
                    _device.DrawPrimitives(PrimitiveType.PointList, 0, _size.X*_size.Y);
                }
                if (_sprite != null  && _heightMapTexture != null) {
                    _sprite.Begin(SpriteFlags.None);
                    _sprite.Draw(_heightMapTexture, null, null, new Vector3(1.0f, 1.0f, 1.0f), Color.White);
                    _sprite.End();
                }
            } catch (Exception ex) {
                Debug.Print("Error in {0} - {1}\n{2}", ex.TargetSite, ex.Message, ex.StackTrace);
            }
        }

        // Editor functions
        public void MoveRect(Direction dir) {
            switch (dir) {
                case Direction.Left:
                    _selectRect.X--;
                    break;
                case Direction.Right:
                    _selectRect.X++;
                    break;
                case Direction.Up:
                    _selectRect.Y--;
                    break;
                case Direction.Down:
                    _selectRect.Y++;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("dir");
            }
            Thread.Sleep(100);
            CreateParticles();
        }
        public void RaiseTerrain(Rectangle r, float f) {
            for (var y = r.Top; y <= r.Bottom; y++) {
                for (var x = r.Left; x <= r.Right; x++) {
                    var i = x + y * _size.X;
                    _heightMap[i] += f;
                    if (_heightMap[i] < -_maxHeight) _heightMap[i] = -_maxHeight;
                    if ( _heightMap[i] > _maxHeight )_heightMap[i] = _maxHeight;
                }
            }
            CreateParticles();
        }
        public void SmoothTerrain() {
            var hm = new float[_size.X * _size.Y];
            for (var y = 0; y < _size.X; y++) {
                for (var x = 0; x < _size.X; x++) {
                    var totalHeight = 0.0f;
                    var numNodes = 0;
                    for (var y1 = y-1; y1 <= y+1; y1++) {
                        for (var x1 = x-1; x1 <= x+1; x1++) {
                            if (x1 < 0 || x1 >= _size.X || y1 < 0 || y1 >= _size.Y) {
                                continue;
                            }
                            totalHeight += _heightMap[x1 + y1 * _size.X];
                            numNodes++;
                        }
                    }
                    hm[x + y * _size.X] = totalHeight / numNodes;
                }
            }
            _heightMap = hm;
            CreateParticles();
            Thread.Sleep(500);
        }

        public Vector2 Center { get { return new Vector2(_size.X /2.0f, _size.Y / 2.0f);}}
        public float MaxHeight { get { return _maxHeight; } private set { _maxHeight = value; } }
        public bool ShowSelection { get; set; }
        public Rectangle SelectionRect { get { return _selectRect; } set { _selectRect = value; } }
        public Point Size { get { return _size; } }
    }

    public enum Direction {
        Left,Right, Up, Down
    }
}
