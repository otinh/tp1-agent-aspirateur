using System.Diagnostics;
using System.Windows.Input;

namespace tp1_agent_aspirateur
{
    public partial class MainWindow
    {
        private static Agent agent;

        public MainWindow()
        {
            InitializeComponent();

            KeyDown += MainWindow.OnKeyDown;

            var environment = new Environment(RoomGrid);
            environment.start();

            agent = new Agent(environment);
            agent.start();
        }

        private static void OnKeyDown(object sender, KeyEventArgs e)
        { 
            Debug.WriteLine("moving");
            if (e.Key == Key.Space)
            {
                agent.move();
            }
        }
    }
}