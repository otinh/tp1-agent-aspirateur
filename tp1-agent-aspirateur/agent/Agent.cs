using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace tp1_agent_aspirateur
{
    public class Agent
    {
        // Caractéristiques propres à l'agent
        private const int BATTERY_MAX = 100;
        private int battery = BATTERY_MAX;
        private Environment.Position position;
        private bool isAlive = true;

        // Fil d'exécution et environnement auquel l'agent est lié
        private Thread thread;
        private readonly Environment environment;
        private const int UPDATE_TIME = 1500;

        // Les différents senseurs et effecteurs
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

        // Cellule la plus proche ayant un intérêt pour nous
        private Cell belief;

        // L'action que l'on désire faire
        private (Action, Cell) desire;

        // L'action que l'on va faire au prochain tour pour accomplir le desire
        private Action intention;

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
                observe(environment);
                updateState(environment);
                
                var action = chooseAction();
                doAction(action);
                
                checkBatteryLevel();
                Thread.Sleep(UPDATE_TIME);
            }
        }

        private void observe(Environment env)
        {
            throw new NotImplementedException();
        }

        private void updateState(Environment env)
        {
            throw new NotImplementedException();
        }

        private Action chooseAction()
        {
            throw new NotImplementedException();
        }

        private void doAction(Action action)
        {
            switch (action)
            {
                case Action.MOVE_UP:
                case Action.MOVE_RIGHT:
                case Action.MOVE_DOWN:
                case Action.MOVE_LEFT:
                    move(action);
                    break;

                case Action.CLEAN:
                    clean();
                    break;

                case Action.PICKUP:
                    pickup();
                    break;

                case Action.STAY:
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }
        }

        public void move(Action action)
        {
            updateRobotPosition(action);
            consumeBattery();
            wheels.move(environment, action);
        }

        public void clean()
        {
            consumeBattery();
            cleaner.clean(environment, position);
        }

        public void pickup()
        {
            consumeBattery();
            brush.pickup(environment, position);
        }

        private void updateRobotPosition(Action action)
        {
            switch (action)
            {
                case Action.MOVE_UP:
                    position.y--;
                    break;
                case Action.MOVE_RIGHT:
                    position.x++;
                    break;
                case Action.MOVE_DOWN:
                    position.y++;
                    break;
                case Action.MOVE_LEFT:
                    position.x--;
                    break;
                case Action.CLEAN:
                case Action.PICKUP:
                case Action.STAY:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }
        }

        private void consumeBattery()
        {
            battery--;
            Debug.WriteLine($"Battery: {battery}%");
        }

        private void checkBatteryLevel()
        {
            if (battery == 0)
            {
                isAlive = false;
                // Debug.WriteLine("Agent is dead :(");
            }
        }

        private List<Action> getPossibleActions()
        {
            var actions = new List<Action>();

            if (position.x != Environment.MIN_X) actions.Add(Action.MOVE_LEFT);
            if (position.x != Environment.MAX_X) actions.Add(Action.MOVE_RIGHT);
            if (position.y != Environment.MIN_Y) actions.Add(Action.MOVE_DOWN);
            if (position.y != Environment.MAX_Y) actions.Add(Action.MOVE_UP);

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
    }
}