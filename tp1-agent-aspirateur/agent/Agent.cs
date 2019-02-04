using System.Collections.Generic;
using System.Threading;

namespace tp1_agent_aspirateur
{
    public class Agent
    {
        private const int BATTERY_MAX = 100;
        private int x;
        private int y;

        private Thread thread;
        private Environment environment;

        private Sensor lidar;
        private readonly Wheels wheels;
        private readonly Cleaner cleaner;
        private readonly Brush brush;

        public enum Action
        {
            MOVE_UP,
            MOVE_DOWN,
            MOVE_LEFT,
            MOVE_RIGHT,
            CLEAN,
            PICKUP,
            STAY
        }

        private Cell belief; // cellule la plus proche ayant un intérêt pour nous
        private (Action, Cell) desire; // l'action que l'on désire faire
        private Action intention; // l'action que l'on va faire au prochain tour pour accomplir le desire

        private readonly int battery = BATTERY_MAX;
        private bool isAlive = true;

        public Agent(Environment environment)
        {
            this.environment = environment;
            wheels = new Wheels();
            lidar = new Sensor();
            brush = new Brush();
            cleaner = new Cleaner();
        }

        public void start()
        {
            thread = new Thread(update);

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        private void update()
        {
            while (isAlive)
            {
                // Debug.WriteLine($"Agent's battery is at {battery--}%.");
                if (battery == 0)
                {
                    isAlive = false;
                    // Debug.WriteLine("Agent is dead :(");
                }

                Thread.Sleep(1500);
            }
        }

        public void move(Action action)
        {
            switch (action)
            {
                case Action.MOVE_UP:
                    y--;
                    break;
                case Action.MOVE_RIGHT:
                    x++;
                    break;
                case Action.MOVE_DOWN:
                    y++;
                    break;
                case Action.MOVE_LEFT:
                    x--;
                    break;
            }
            wheels.move(environment, action);
        }

        public void clean()
        {
            cleaner.clean(environment, x, y);
        }

        public void pickup()
        {
            brush.pickup(environment, x, y);
        }

        private List<Action> getPossibleActions()
        {
            var actions = new List<Action>();

            if (x != 0) actions.Add(Action.MOVE_LEFT);
            if (x != 9) actions.Add(Action.MOVE_RIGHT);
            if (y != 0) actions.Add(Action.MOVE_DOWN);
            if (y != 9) actions.Add(Action.MOVE_UP);

            actions.Add(Action.PICKUP);
            actions.Add(Action.CLEAN);
            actions.Add(Action.STAY);

            return actions;
        }

        private bool matchGoal(Action a)
        {
            if (a == desire.Item1) return true;

            //test pour voir si le move est bien.
            return true;
        }

        private void doAction(Cell[,] map, Environment environment)
        {
            switch (map[x, y].state)
            {
                case Cell.State.DUST:
                    cleaner.clean(environment, x, y);
                    break;
                case Cell.State.JEWEL:
                    brush.pickup(environment, x, y);
                    break;
                case Cell.State.DUST_AND_JEWEL:
                    brush.pickup(environment, x, y);
                    break;
                case Cell.State.EMPTY:
                    //wheels.move(myEnv, myRow + 1, myColumn);
                    break;
            }
        }
    }
}