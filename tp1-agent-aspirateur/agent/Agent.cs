using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
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

        // Caract�ristiques propres � l'agent
        private const int BATTERY_MAX = 100;
        private int battery = BATTERY_MAX;
        private Position position;
        private bool isAlive = true;
        private int n;

        // Fil d'ex�cution et environnement auquel l'agent est li�
        private Thread thread;
        private readonly Environment environment;
        private const int UPDATE_TIME = 1500;

        // Les diff�rents senseurs et effecteurs
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

        // Action finale souhait�e sur la cellule
        private Stack<Action> intention;

        public Agent(Environment environment, Exploration exploration, int n)
        {
            this.environment = environment;
            this.exploration = exploration;
            this.n = n;

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

            var perceivedGrid = observe(environment);
            updateInternalState(perceivedGrid);
        }

        private void update()
        {
            var count = 0;
            while (isAlive)
            {
                if (count <= n || intention.ToArray().Length == 0)
                {
                    var perceivedGrid = observe(environment);
                    updateInternalState(perceivedGrid);
                    count = 0;
                }

                var action = chooseAction();
                doAction(action);
                count++;

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
            intention = getIntention(desire, perceivedGrid);
        }

        // Retourne la liste de toutes les cellules non vides per�ues par l'agent
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
         * Retourne la cellule non-vide la mieux �valu�e et sa performance potentielle.
         * Si l'agent se trouve d�j� sur une case non-vide elle deviendra sa priorit�.
         * Performance potentielle = (Gain en performance de l'action) - (Co�t de distance et de l'action en �lectricit�)
         */
        private (Cell, int) getDesire(IEnumerable<Cell> nonEmptyCells)
        {
            var desiredCell = new Cell();
            var desiredPerformance = int.MinValue;

            foreach (var cell in nonEmptyCells)
            {
                var potential = getPotential(cell);

                var distance = Utils.getDistance(position, cell.position);
                var actionCost = cell.state == Cell.State.DUST_AND_JEWEL ? 2 : 1;
                var cost = distance + actionCost;

                var performance = potential - cost;

                // L'agent se trouve d�j� sur une case non-vide.
                if (distance == 0) return (cell, performance);

                // Stocke la cellule ayant la meilleure performance.
                if (performance <= desiredPerformance) continue;
                desiredCell = cell;
                desiredPerformance = performance;
            }

            return (desiredCell, desiredPerformance);
        }

        // Retourne les actions que va effectuer l'agent pour atteindre son but.
        private Stack<Action> getIntention((Cell, int) desiredCell, Cell[,] perceivedGrid)
        {
            var intendedActions = new Stack<Action>();

            if (performanceIsTooLow(desiredCell))
            {
                intendedActions.Push(Action.STAY);
                return intendedActions;
            }


            switch (exploration)
            {
                case Exploration.BFS:
                    intendedActions = exploreBfs(desiredCell.Item1, perceivedGrid);
                    break;
                case Exploration.GREEDY_SEARCH:
                    intendedActions = exploreGreedy(desiredCell.Item1, perceivedGrid);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Debug.WriteLine("Actions: {");
            foreach (var action in intendedActions)
            {
                Debug.WriteLine(action);
            }

            Debug.WriteLine("}");

            return intendedActions;
        }

        private Stack<Action> exploreBfs(Cell destination, Cell[,] grid)
        {
            // Initialisation
            var startCell = getRobotCell(grid);
            var frontier = new List<Cell> {startCell};
            var cameFrom = new Dictionary<Cell, Cell> {{startCell, null}};

            // Exploration
            while (frontier.Count != 0)
            {
                var currentCell = frontier[0];
                frontier.RemoveAt(0);

                // TODO: peut-�tre changer cela, car on impl�mente une heuristique dans BFS alors que l'on veut du Greedy !
                if (currentCell == destination) break;

                foreach (var cell in Cell.getNeighborCells(currentCell, grid))
                {
                    // Si la cellule n'a pas d�j� �t� visit�e, on l'ajoute � la fronti�re et on ajoute sa provenance.
                    if (cameFrom.ContainsKey(cell)) continue;
                    frontier.Add(cell);
                    cameFrom.Add(cell,
                        currentCell); //on ajoute tous les voisins alors que certains ne seront pas visit�s ?
                }
            }

            // On r�cup�re le chemin en terme de cellules qu'on traduit par des actions.
            var cellPath = Cell.getCellPath(startCell, destination, cameFrom);
            var actions = getActions(cellPath);

            return actions;
        }

        private Stack<Action> exploreGreedy(Cell destination, Cell[,] grid)
        {
            // Initialisation
            var startCell = getRobotCell(grid);
            var frontier = new List<(Cell, int)> {(startCell, 1000)};
            var cameFrom = new Dictionary<Cell, Cell> {{startCell, null}};

            // Exploration
            while (frontier.Count != 0)
            {
                var currentCell = frontier[0].Item1;
                frontier.RemoveAt(0);

                if (currentCell == destination) break;


                foreach (var cell in Cell.getNeighborCells(currentCell, grid))
                {
                    // Si la cellule n'a pas d�j� �t� visit�e, on l'ajoute � la fronti�re et on ajoute sa provenance.
                    if (cameFrom.ContainsKey(cell)) continue;
                    frontier.Add((cell, Utils.getDistance(destination.position, cell.position)));
                    cameFrom.Add(cell, currentCell);
                }

                frontier = sortFrontier(frontier);
            }


            // On r�cup�re le chemin en terme de cellules qu'on traduit par des actions.
            var cellPath = Cell.getCellPath(startCell, destination, cameFrom);
            var actions = getActions(cellPath);

            return actions;
        }


        // On traduit L'ensemble des cellules en une s�rie d'actions.
        private static Stack<Action> getActions(Stack<Cell> path)
        {
            var actionPath = new Stack<Action>();
            actionPath.Push(getAction(path.Peek()));

            while (path.Count > 1)
            {
                var cell = path.Pop();
                var nextCell = path.Peek();
                if (cell.isBelow(nextCell)) actionPath.Push(Action.MOVE_UP);
                if (cell.isLeftFrom(nextCell)) actionPath.Push(Action.MOVE_RIGHT);
                if (cell.isAbove(nextCell)) actionPath.Push(Action.MOVE_DOWN);
                if (cell.isRightFrom(nextCell)) actionPath.Push(Action.MOVE_LEFT);
            }

            return actionPath;
        }

        // Retourne le gain de performance brut en fonction de l'�tat de la cellule.
        private static int getPotential(Cell cell)
        {
            switch (cell.state)
            {
                case Cell.State.DUST:
                    return 8;

                case Cell.State.JEWEL:
                    return 10;

                case Cell.State.DUST_AND_JEWEL:
                    return 18;

                case Cell.State.EMPTY:
                    return 0;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private Action chooseAction()
        {
            return intention.Pop();
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

        private void checkBatteryLevel()
        {
            if (battery == 0)
            {
                isAlive = false;
                Debug.WriteLine("Agent is dead :(");
            }
        }

        private static bool performanceIsTooLow((Cell, int) cell)
        {
            return cell.Item2 <= 0;
        }

        private static Action getAction(Cell cell)
        {
            Action action;

            switch (cell.state)
            {
                case Cell.State.DUST:
                    action = Action.CLEAN;
                    break;

                case Cell.State.EMPTY:
                    action = Action.STAY;
                    break;

                case Cell.State.JEWEL:
                case Cell.State.DUST_AND_JEWEL:
                    action = Action.PICKUP;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return action;
        }

        private Cell getRobotCell(Cell[,] grid)
        {
            return grid[position.x, position.y];
        }

        private static List<(Cell, int)> sortFrontier(List<(Cell, int)> frontier)
        {
            (Cell, int) temp;

            for (var i = 1; i <= frontier.Count; i++)
            for (var j = 0; j < frontier.Count - i; j++)
                if (frontier[j].Item2 > frontier[j + 1].Item2)
                {
                    temp = frontier[j];
                    frontier[j] = frontier[j + 1];
                    frontier[j + 1] = temp;
                }

            return frontier;
        }

        private void setExploration(Exploration exp)
        {
            Debug.WriteLine($"Exploration mode: {exp}");
            exploration = exp;
        }

        public void switchExploration()
        {
            setExploration(exploration == Exploration.BFS ? Exploration.GREEDY_SEARCH : Exploration.BFS);
        }
    }
}