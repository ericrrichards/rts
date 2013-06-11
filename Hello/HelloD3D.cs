namespace Hello {
    using System.Diagnostics;
    using System.Windows.Forms;

    using Core;

    public class Program {
        public static void Main(string[] args) {
            var app = new App();
            if (app.Init(800, 600, true).IsFailure) {
                return;
            }
            var timer = new Stopwatch();
            timer.Start();
            while (app.IsRunning) {
                Application.DoEvents();
                var dt = timer.ElapsedMilliseconds * 0.001f;
                app.Update(dt);
                app.Render();

                timer.Restart();
            }
            app.Cleanup();
        }
    }
    
}
