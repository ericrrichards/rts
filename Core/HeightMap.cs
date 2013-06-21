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
        private readonly Device _device;
        public HeightMapRenderer Renderer { private get; set; }


        public HeightMap(Device device, Point size, float maxHeight) {
            System.Diagnostics.Debug.Assert(device != null);
            try {
                _size = size;
                _device = device;
                _maxHeight = maxHeight;

                _heightMap = new float[_size.X*_size.Y];
            } catch (Exception ex) {
                Debug.Print("Error in {0} - {1}\n{2}", ex.TargetSite, ex.Message, ex.StackTrace);
            }
        }
        ~HeightMap() {
            Release();
        }
        public void Release() {
            _heightMap = null;
            ReleaseCom(HeightMapTexture);
        }
        public static HeightMap operator *(HeightMap left, HeightMap right) {
            var ret = new HeightMap(left._device, left.Size, left.MaxHeight);
            for (int y = 0; y < left.Size.Y; y++) {
                for (int x = 0; x < left.Size.X; x++) {
                    var a = left[x + y*left.Size.X]/left.MaxHeight;
                    var b = 1.0f;
                    if (x <= right.Size.X && y <= right.Size.Y) {
                        b = right[x + y*left.Size.X]/right.MaxHeight;
                    }
                    ret[x + y*ret.Size.X] = a*b*left.MaxHeight;
                }
            }
            return ret;
        }

        public Result LoadFromFile(string filename) {
            try {
                _heightMap = new float[_size.X*_size.Y];

                ReleaseCom(HeightMapTexture);

                HeightMapTexture = Texture.FromFile(_device, filename,_size.X, _size.Y, 1, Usage.Dynamic, Format.L8, Pool.Default, Filter.Default, Filter.Default, 0);

                var dr = HeightMapTexture.LockRectangle(0, LockFlags.None);
                for (var y = 0; y < _size.Y; y++) {
                    for (var x = 0; x < _size.X; x++) {
                        dr.Data.Seek(y*dr.Pitch + x, SeekOrigin.Begin);
                        
                        var b = dr.Data.Read<byte>();
                        _heightMap[x + y*_size.X] = b/255.0f*_maxHeight;
                    }
                }
                HeightMapTexture.UnlockRectangle(0);
            } catch (Exception ex) {
                Debug.Print("Error in {0} - {1}\n{2}", ex.TargetSite, ex.Message, ex.StackTrace);
                return ResultCode.Failure;
            }
            return ResultCode.Success;
        }
        public Result CreateRandomHeightMap(int seed, float noiseSize, float persistence, int octaves) {
            ReleaseCom(HeightMapTexture);
            HeightMapTexture = new Texture(_device, _size.X, _size.Y, 1, Usage.Dynamic, Format.L8, Pool.Default);

            var dr = HeightMapTexture.LockRectangle(0, LockFlags.None);
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
            HeightMapTexture.UnlockRectangle(0);
            return ResultCode.Success;
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
            if (Renderer != null) {
                Renderer.CreateParticles();
            }
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
            if (Renderer != null) {
                Renderer.CreateParticles();
            }
            Thread.Sleep(500);
        }

        public void Cap(float capHeight) {
            MaxHeight = 0.0f;
            for (var y = 0; y < _size.X; y++) {
                for (var x = 0; x < _size.X; x++) {
                    int i = x + y*Size.X;
                    this[i] -= capHeight;
                    if (this[i] < 0.0f) {
                        this[i] = 0.0f;
                    }
                    if (this[i] > MaxHeight) {
                        MaxHeight = this[i];
                    }
                }
            }
        }

        public Vector2 Center { get { return new Vector2(_size.X /2.0f, _size.Y / 2.0f);}}
        public float MaxHeight { get { return _maxHeight; } private set { _maxHeight = value; } }
        
        
        public Point Size { get { return _size; } }
        public Texture HeightMapTexture { get; private set; }

        public float this[int i] { get { return _heightMap[i]; } private set { _heightMap[i] = value; } }
    }

    public enum Direction {
        Left,Right, Up, Down
    }
}
