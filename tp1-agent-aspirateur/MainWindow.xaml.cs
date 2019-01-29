using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace tp1_agent_aspirateur
{
    public partial class MainWindow
    {
        internal static MainWindow main;

        public MainWindow()
        {
            InitializeComponent();

            main = this;

            var environment = new Environment(RoomGrid);
            environment.start();

            var agent = new Agent();
            agent.start();
        }

        // Alternative pour corriger le problème de thread en appelant cette fonction 
        // depuis Environment.cs qui ne marche pas non plus
        public void display(Image image)
        {
            if (Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal,
                    new ThreadStart(() => { RoomGrid.Children.Add(image); }));
            }
            else
            {
                Debug.WriteLine("doesn't have access");
            }
        }
    }
}