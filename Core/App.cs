using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core {
    using System.Drawing;
    using System.Threading;
    using System.Windows.Forms;

    using SlimDX.Direct3D9;

    public abstract class App {
        public static App GApp;
        public static Device GDevice;
        public static Random Rand;

        protected string MainWindowCaption;
        protected DeviceType DevType;
        protected CreateFlags RequestedVertexProcessing;
        protected Direct3D D3DObject;
        protected bool Paused;
        protected PresentParameters PP;
        private bool _running = true;

        public IntPtr AppInstance { get; protected set; }
        public Form MainWindow { get; protected set; }

        protected App(IntPtr hinstance, string caption, DeviceType devType, CreateFlags requestedVP) {
            MainWindowCaption = caption;
            DevType = devType;
            RequestedVertexProcessing = requestedVP;

            AppInstance = hinstance;
            MainWindow = null;
            D3DObject = null;
            Paused = false;
            PP = new PresentParameters();
            Rand = new Random();
            InitMainWindow();
            InitDirect3D();
        }

         ~App() {
            if (D3DObject != null) {
                D3DObject.Dispose();
                D3DObject = null;
            }
            if (GDevice != null) {
                GDevice.Dispose();
                GDevice = null;
            }
        }

        public virtual bool CheckDeviceCaps() {return true;}
        public abstract void OnLostDevice();
        public abstract void OnResetDevice();
        public abstract void UpdateScene(float dt);
        public abstract void DrawScene();

        public virtual void InitMainWindow() {
            MainWindow = new Form() {
                Text = MainWindowCaption,
                Width = 800,
                Height = 600,
                FormBorderStyle = FormBorderStyle.FixedSingle,
                SizeGripStyle = SizeGripStyle.Hide,
                StartPosition = FormStartPosition.CenterScreen,

            };
            MainWindow.Show();
            MainWindow.Update();
        }
        public virtual void InitDirect3D() {
            D3DObject = new Direct3D();
            if (D3DObject == null) {
                MessageBox.Show("Failed to create Direct3D object");
                Exit();
            }

            var mode = D3DObject.GetAdapterDisplayMode(0);
            if (!D3DObject.CheckDeviceType(0, DevType, mode.Format, mode.Format, true)) {
                MessageBox.Show(string.Format("Display mode {0}x{1}-{2} is not available windowed", mode.Width, mode.Height, mode.Format));
            }
            if (!D3DObject.CheckDeviceType(0, DevType, Format.X8R8G8B8, Format.X8R8G8B8, false)) {
                MessageBox.Show(string.Format("Display mode {0}x{1}-{2} is not available fullscreen", mode.Width, mode.Height, mode.Format));
            }

            
            var caps = D3DObject.GetDeviceCaps(0, DevType);
            CreateFlags devBehaviorFlags = 0;
            if (caps.DeviceCaps.HasFlag(DeviceCaps.HWTransformAndLight)) {
                devBehaviorFlags |= RequestedVertexProcessing;
            } else {
                devBehaviorFlags |= CreateFlags.SoftwareVertexProcessing;
            }
            if (caps.DeviceCaps.HasFlag(DeviceCaps.PureDevice) && devBehaviorFlags.HasFlag(CreateFlags.HardwareVertexProcessing)) {
                devBehaviorFlags |= CreateFlags.PureDevice;
            }

            PP = new PresentParameters() {
                BackBufferWidth = 0,
                BackBufferHeight = 0,
                BackBufferFormat = Format.Unknown,
                BackBufferCount = 1,
                Multisample = MultisampleType.None,
                MultisampleQuality = 0,
                SwapEffect = SwapEffect.Discard,
                DeviceWindowHandle = MainWindow.Handle,
                Windowed = true,
                EnableAutoDepthStencil = true,
                AutoDepthStencilFormat = Format.D24S8,
                PresentFlags = PresentFlags.None,
                FullScreenRefreshRateInHertz = 0,
                PresentationInterval = PresentInterval.Immediate
            };

            GDevice = new Device(D3DObject, 0, DevType, MainWindow.Handle, devBehaviorFlags, PP );

        }

        protected void Exit() {
            _running = false;
            Application.Exit();
        }

        public virtual int Run() {
            while (_running) {
                
                Application.DoEvents();
                if (Paused) {
                    Thread.Sleep(20);
                    continue;
                }
                if (!IsDeviceLost()) {
                    UpdateScene(0.0f);
                    DrawScene();
                }
            }
            return 0;
        }
        public void EnableFullScreenMode(bool enable) {
            if (enable) {
                if (!PP.Windowed) return;

                var width = Screen.PrimaryScreen.Bounds.Width;
                var height = Screen.PrimaryScreen.Bounds.Height;

                PP.BackBufferFormat = Format.X8R8G8B8;
                PP.BackBufferWidth = width;
                PP.BackBufferHeight = height;
                PP.Windowed = false;

                MainWindow.FormBorderStyle = FormBorderStyle.None;
                MainWindow.SetDesktopLocation(0,0);


            } else {
                if (PP.Windowed) return;
                MainWindow.ClientSize = new Size(800,600);
                PP.BackBufferFormat = Format.Unknown;
                PP.BackBufferWidth = 800;
                PP.BackBufferHeight = 600;
                PP.Windowed = true;

                MainWindow.FormBorderStyle = FormBorderStyle.FixedSingle;
                MainWindow.SetDesktopLocation(100, 100);
            }
            OnLostDevice();
            GDevice.Reset(PP);
            OnResetDevice();
        }
        public bool IsDeviceLost() {
            var hr = GDevice.TestCooperativeLevel();
            if (hr == ResultCode.DeviceLost) {
                Thread.Sleep(20);
                return true;
            }
            if (hr == ResultCode.DriverInternalError) {
                MessageBox.Show("Driver internal error, exiting...");
                Exit();
                return true;
            }
            if (hr == ResultCode.DeviceNotReset) {
                OnLostDevice();
                GDevice.Reset(PP);
                OnResetDevice();
                return false;
            }
            return false;
        } 
    }
}
