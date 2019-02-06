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
            }

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

        private void consumeBattery()
        {
            battery--;
            Debug.WriteLine($"Battery: {battery}%");
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

        private void doAction(Environment.Position position)
        {
            switch (environment.getState(position))
            {
                case Cell.State.DUST:
                    clean();
                    break;
                
                case Cell.State.JEWEL:
                case Cell.State.DUST_AND_JEWEL:
                    pickup();
                    break;
                
                case Cell.State.EMPTY:
                    //wheels.move(myEnv, myRow + 1, myColumn);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}