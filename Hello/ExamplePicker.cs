using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Core;

namespace Hello {
    public partial class ExamplePicker : Form {
        [STAThread]
        public static void Main(string[] args) {
            Application.ThreadException += (sender, eventArgs) => Debugger.Break();
            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) => Debugger.Break();
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ExamplePicker());
        }

        private readonly List<App> _examples = new List<App>(); 
        public ExamplePicker() {
            InitializeComponent();
            var types = GetType().Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof (App)));
            foreach (var type in types) {
                tableLayoutPanel1.RowCount++;
                var i = tableLayoutPanel1.RowStyles.Add(new RowStyle());
                var example = (App) Activator.CreateInstance(type);
                _examples.Add(example);
                tableLayoutPanel1.Controls.Add(new Label(){Text = example.GetName()}, 0,i);
                var button = new Button() {
                    Text = "Launch"
                };
                button.Click += (sender, args) => 
                    example.Main(new string[0]);
                tableLayoutPanel1.Controls.Add(button, 1, i);

            }


        }
    }
}
