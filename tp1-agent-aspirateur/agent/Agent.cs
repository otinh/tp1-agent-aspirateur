using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using static tp1_agent_aspirateur.Environment;

namespace tp1_agent_aspirateur
{
    public class Agent
    {
        public enum Action
        {
            MOVE_UP,
            MOVE_RIGHT,
            MOVE_DOWN,
            MOVE_LEFT,
            CLEAN,
            PICKUP,
            STAY
        }

        public enum Exploration
        {
            BFS,
            GREEDY_SEARCH
        }

        // Caractéristiques propres à l'agent
        private const int BATTERY_MAX = 100;
        private int battery = BATTERY_MAX;
        private Position position;
        private bool isAlive = true;

        // Fil d'exécution et environnement auquel l'agent est lié
        private Thread thread;
        private readonly Environment environment;
        private const int UPDATE_TIME = 1500;

        // Les différents senseurs et effecteurs
        private readonly Sensor sensor;
        private readonly Wheels wheels;
        private readonly Vacuum vacuum;
        private readonly Arm arm;

        // Algorithme d'exploration
        private Exploration exploration;

        // Perception de l'environnement vue par l'agent
        private List<Cell> belief;

        // (Cellule la plus proche, gain en performance potentiel)
        private (Cell, int) desire;

        // Action finale souhaitée sur la cellule
        private Action intention;

        public Agent(Environment environment, Exploration exploration = Exploration.BFS)
        {
            this.environment = environment;
            this.exploration = exploration;

            sensor = new Sensor();
            wheels = new Wheels();
            arm = new Arm();
            vacuum = new Vacuum();
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
                var perceivedGrid = observe(environment);
                updateInternalState(perceivedGrid);

                // var action = chooseAction();
                // doAction(action);

                checkBatteryLevel();
                Thread.Sleep(UPDATE_TIME);
            }
        }

        private Cell[,] observe(Environment env)
        {
            return sensor.observe(env);
        }

        private void updateInternalState(Cell[,] perceivedGrid)
        {
            belief = getBelief(perceivedGrid);
            desire = getDesire(belief);
            intention = getIntention(desire);
        }

        // Retourne la liste de toutes les cellules non vides perçues par l'agent
        private static List<Cell> getBelief(Cell[,] grid)
        {
            var nonEmptyCells = new List<Cell>();

            for (var x = 0; x < MAX_X + 1; ++x)
            for (var y = 0; y < MAX_Y + 1; ++y)
                if (grid[x, y].state != Cell.State.EMPTY)
                    nonEmptyCells.Add(grid[x, y]);

            return nonEmptyCells;
        }

        /*
         * Performance potentielle = (Gain en performance de l'action) - (Coût de distance et de l'action en électricité)
         * Retourne la cellule non-vide la mieux évaluée et sa performance potentielle.
         * Si l'agent se trouve déjà sur une case non-vide elle deviendra sa priorité.
         */
        private (Cell, int) getDesire(IEnumerable<Cell> cells)
        {
            var desiredCell = new Cell();
            var maxPerformance = int.MinValue;
            var performance = 0;
            
            foreach (var cell in cells)
            {
                var potential = getPotential(cell);
                
                var distance = Utils.getDistance(position, cell.position);
                var actionCost = cell.state == Cell.State.DUST_AND_JEWEL ? 2 : 1;
                var cost = distance + actionCost;
                
                performance = potential - cost;

                if (distance == 0) return (cell, performance);

                if (performance <= maxPerformance) continue;
                desiredCell = cell;
                maxPerformance = performance;
            }

            return (desiredCell, performance);
        }

        // Retourne l'action finale souhaitée sur la cellule désirée.
        // TODO: peut-être plutôt implémenter une List<Action> qu'on construit avec les algorithmes d'exploration ?
        private static Action getIntention((Cell, int) desire)
        {
            if (desire.Item2 <= 0) return Action.STAY;

            switch (desire.Item1.state)
            {
                case Cell.State.DUST:
                    return Action.CLEAN;

                case Cell.State.JEWEL:
                case Cell.State.DUST_AND_JEWEL:
                    return Action.PICKUP;

                case Cell.State.EMPTY:
                    return Action.STAY;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        // Retourne le gain de performance brut en fonction de l'état de la cellule.
        private static int getPotential(Cell cell)
        {
            switch (cell.state)
            {
                case Cell.State.DUST:
                    return 3;

                case Cell.State.JEWEL:
                    return 7;

                case Cell.State.DUST_AND_JEWEL:
                    return 10;

                case Cell.State.EMPTY:
                    return 0;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private Action chooseAction()
        {
            var possibleActions = getPossibleActions();

            foreach (var action in possibleActions)
            {
                if (isReachable(action, desire))
                {
                    return action;
                }
            }

            return Action.STAY;
        }

        private IEnumerable<Action> getPossibleActions()
        {
            var actions = new List<Action>
            {
                Action.CLEAN,
                Action.PICKUP,
                Action.STAY
            };

            if (position.x != MIN_X) actions.Add(Action.MOVE_LEFT);
            if (position.x != MAX_X) actions.Add(Action.MOVE_RIGHT);
            if (position.y != MIN_Y) actions.Add(Action.MOVE_DOWN);
            if (position.y != MAX_Y) actions.Add(Action.MOVE_UP);

            return actions;
        }

        private bool isReachable(Action action, (Cell, int) desire)
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
            vacuum.clean(environment, position);
        }

        public void pickup()
        {
            consumeBattery();
            arm.pickup(environment, position);
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

        // TODO: gérer la fin de vie de l'agent
        private void checkBatteryLevel()
        {
            if (battery == 0)
            {
                isAlive = false;
                Debug.WriteLine("Agent is dead :(");
            }
        }
    }
}