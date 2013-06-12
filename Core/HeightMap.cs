using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SlimDX;
using SlimDX.Direct3D9;

namespace Core {
    [StructLayout(LayoutKind.Sequential)]
    internal struct Particle {
        public Vector3 Position;
        public Color4 Color;
        public static readonly VertexFormat FVF = VertexFormat.Position | VertexFormat.Diffuse;
        public const int Size = 28;
    }


    public class HeightMap :GameObject{
        private Point _size;
        private float _maxHeight;
        private float[] _heightMap;

        // temp rendering stuff
        private VertexBuffer _vb;
        private Device _device;
        private Sprite _sprite;
        private Texture _heightMapTexture;

        public HeightMap(Device device, Point size) {
            System.Diagnostics.Debug.Assert(device != null);
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

                _heightMapTexture = Texture.FromFile(_device, filename,_size.X, _size.Y, 1, Usage.Dynamic, Format.L8, Pool.Default, Filter.Default, Filter.Default, 0);

                var dr = _heightMapTexture.LockRectangle(0, LockFlags.None);
                for (int y = 0; y < _size.Y; y++) {
                    for (int x = 0; x < _size.X; x++) {
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
        public Result CreateParticles() {
            try {
                ReleaseCom(_vb);
                _vb = new VertexBuffer(_device, _size.X*_size.Y*Particle.Size, Usage.Dynamic | Usage.Points | Usage.WriteOnly, Particle.FVF, Pool.Default);

                var ds = _vb.Lock(0, 0, LockFlags.Discard);
                for (int y = 0; y < _size.Y; y++) {
                    for (int x = 0; x < _size.X; x++) {
                        var prc = _heightMap[x + y*_size.X]/_maxHeight;
                        var red = prc;
                        var green =1.0f - prc;

                        var v = new Particle() {
                            Color = new Color4(1.0f, red, green, 0.0f),
                            Position = new Vector3((float)x, _heightMap[x+y*_size.X], -(float)y)
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
                    _sprite.Draw(_heightMapTexture, null, null, new Vector3(1.0f, 1.0f, 1.0f), Color.White);
                    _sprite.End();
                }
            } catch (Exception ex) {
                Debug.Print("Error in {0} - {1}\n{2}", ex.TargetSite, ex.Message, ex.StackTrace);
            }
        }
        public Vector2 Center { get { return new Vector2(_size.X /2.0f, _size.Y / 2.0f);}}
        public float MaxHeight { get { return _maxHeight; } set { _maxHeight = value; } }
    }
    public static class MathF {
        public static float Sin(float a) {
            return (float) Math.Sin(a);
        }
        public static float Cos(float a) {
            return (float)Math.Cos(a);
        }

        public static float Pi {
            get { return (float) Math.PI; }
        }
    }

}
