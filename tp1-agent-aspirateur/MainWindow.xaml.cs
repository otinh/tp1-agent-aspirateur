using System.Windows.Input;

namespace tp1_agent_aspirateur
{
    public partial class MainWindow
    {
        private static Agent agent;

        public MainWindow()
        {
            InitializeComponent();

            KeyDown += onKeyDown;

            var environment = new Environment(RoomGrid);
            linkText(environment);
            environment.start();

            agent = new Agent(environment, Agent.Exploration.BFS, 1);
            linkText(agent);
            agent.start();
            
        }

        private static void onKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    agent.move(Agent.Action.MOVE_UP);
                    break;
                case Key.Right:
                    agent.move(Agent.Action.MOVE_RIGHT);
                    break;
                case Key.Down:
                    agent.move(Agent.Action.MOVE_DOWN);
                    break;
                case Key.Left:
                    agent.move(Agent.Action.MOVE_LEFT);
                    break;
                case Key.Space:
                    agent.switchExploration();
                    break;
            }
        }

        private void linkText(Environment e)
        {
            e.setTurnText(TurnText);
            e.setPerformanceText(PerformanceText);
        }
        
        private void linkText(Agent a)
        {
            a.setBatteryText(BatteryText);
            a.setExplorationText(ExplorationText);
            a.setActionText(ActionText);
            a.setLearningText(LearningText);
        }
    }
}