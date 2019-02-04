﻿using System.Windows.Input;

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
            environment.start();

            agent = new Agent(environment);
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
                    agent.clean();
                    break;
                case Key.C:
                    agent.pickup();
                    break;
            }
        }
    }
}