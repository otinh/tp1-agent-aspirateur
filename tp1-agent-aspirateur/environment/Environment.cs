using System;
using System.Data;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using static tp1_agent_aspirateur.Agent.Action;
using static tp1_agent_aspirateur.Cell.State;
using static tp1_agent_aspirateur.Utils;

namespace tp1_agent_aspirateur
{
    public class Environment
    {
        public struct Position
        {
            public Position(int x, int y)
            {
                this.x = x;
                this.y = y;
            }

            public void update(Position position)
            {
                x += position.x;
                y += position.y;
            }

            internal int x { get; set; }
            internal int y { get; set; }
        }

        // Coordonnées de la grille
        public const int MIN_X = 0;
        public const int MAX_X = 9;
        public const int MIN_Y = 0;
        public const int MAX_Y = 9;

        // Chance d'apparaître chaque tour en %
        private const int DUST_SPAWN_CHANCE = 15;
        private const int JEWEL_SPAWN_CHANCE = 5;

        // La grille et son affichage
        private Thread thread;
        private readonly Grid gridDisplay;
        private readonly Cell[,] grid = new Cell[MAX_X + 1, MAX_Y + 1];

        // Variables concernant le robot
        private int performance;
        private Position robotPosition = new Position();

        public Environment(Grid gridDisplay)
        {
            this.gridDisplay = gridDisplay;
        }

        /* ------------------------------------------------------------------------- */

        // Initialise les cellules de la grille et lance le thread de l'environnement
        public void start()
        {
            initGrid();
            thread = new Thread(update);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        // Boucle lancée à chaque tour, avec 500ms de délai entre chacun de ces tours
        private void update()
        {
            var turn = 0;
            while (true)
            {
                generateDust();
                generateJewel();
                ++turn;
                Thread.Sleep(500);
            }
        }

        /* ------------------------------------------------------------------------- */

        private void initGrid()
        {
            for (var i = 0; i < MAX_X + 1; ++i)
            for (var j = 0; j < MAX_Y + 1; ++j)
                grid[i, j] = new Cell(EMPTY);
        }

        private void generateDust()
        {
            if (random(0, 100) < DUST_SPAWN_CHANCE) generate(DUST);
        }

        private void generateJewel()
        {
            if (random(0, 100) < JEWEL_SPAWN_CHANCE) generate(JEWEL);
        }

        // Génère et affiche un objet dans un endroit aléatoire de la grille.
        private void generate(Cell.State state)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var randomPosition = new Position(
                    random(MIN_X, MAX_X),
                    random(MIN_Y, MAX_Y)
                );

                setState(state, randomPosition);
                display(getState(randomPosition), randomPosition);
            });
        }

        private void display(Cell.State state, Position position, bool displayRobot = false)
        {
            var sprite = getSprite(state, position, displayRobot);
            display(sprite, position);
        }

        private void display(UIElement sprite, Position position)
        {
            Grid.SetColumn(sprite, position.x);
            Grid.SetRow(sprite, position.y);
            gridDisplay.Children.Add(sprite);
        }

        /* ------------------------------------------------------------------------- */

        public void updateRobotAction(Agent.Action action, Position position)
        {
            switch (action)
            {
                case MOVE_UP:
                case MOVE_RIGHT:
                case MOVE_DOWN:
                case MOVE_LEFT:
                    doMove(position);
                    break;

                case CLEAN:
                    doClean(position);
                    break;

                case PICKUP:
                    doPickup(position);
                    break;

                case STAY:
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }
        }

        private void updatePerformance(Agent.Action action, Position position)
        {
            switch (getState(position))
            {
                case JEWEL when action == CLEAN:
                    performance -= 7;
                    break;
                case JEWEL when action == PICKUP:
                    performance += 7;
                    break;

                case DUST when action == CLEAN:
                    performance += 3;
                    break;
                case DUST when action == PICKUP:
                    performance += 0;
                    break;

                case DUST_AND_JEWEL when action == CLEAN:
                    performance -= 4;
                    break;
                case DUST_AND_JEWEL when action == PICKUP:
                    performance += 7;
                    break;

                case EMPTY:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            Debug.WriteLine($"Performance: {performance}");
        }

        private void doClean(Position position)
        {
            Debug.WriteLine("Action: Clean");

            updatePerformance(CLEAN, position);

            setState(EMPTY, position);
            display(EMPTY, position);
        }

        private void doPickup(Position position)
        {
            Debug.WriteLine("Action: Pickup");

            updatePerformance(PICKUP, position);

            if (getState(position) == JEWEL)
            {
                setState(EMPTY, position);
                display(EMPTY, position);
            }
            else if (getState(position) == DUST_AND_JEWEL)
            {
                setState(EMPTY, position);
                display(DUST, position);
            }
        }

        private void doMove(Position position)
        {
            Debug.WriteLine("Action: Moving");

            Application.Current.Dispatcher.Invoke(() =>
            {
                // Efface le sprite du robot dans l'ancienne case
                var previousCellState = getState(robotPosition);
                display(previousCellState, robotPosition, true);

                robotPosition.update(position);

                // Affiche le sprite du robot dans la nouvelle case
                var nextCellState = getState(robotPosition);
                display(nextCellState, robotPosition);
            });
        }

        /* ------------------------------------------------------------------------- */

        public Cell[,] getGrid()
        {
            return grid;
        }

        private Cell getCell(Position position)
        {
            return grid[position.x, position.y];
        }

        private Image getSprite(Cell.State state, Position position, bool displayRobot = false)
        {
            var image = new Image();
            var bitmapImage = new BitmapImage();
            var uri = getUri(state, position, displayRobot);

            bitmapImage.BeginInit();
            bitmapImage.UriSource = new Uri(uri, UriKind.Relative);
            bitmapImage.EndInit();

            image.Source = bitmapImage;
            return image;
        }

        public Cell.State getState(Position position)
        {
            return getCell(position).state;
        }

        private void setState(Cell.State state, Position position)
        {
            void Set(Cell.State s)
            {
                grid[position.x, position.y].state = s;
            }

            switch (state)
            {
                case DUST when getState(position) == EMPTY:
                    Set(state);
                    break;

                case DUST when getState(position) == JEWEL:
                    Set(DUST_AND_JEWEL);
                    break;

                case JEWEL when getState(position) == EMPTY:
                    Set(state);
                    break;

                case JEWEL when getState(position) == DUST:
                    Set(DUST_AND_JEWEL);
                    break;

                case EMPTY:
                    Set(EMPTY);
                    break;

                case DUST:
                    break;

                case JEWEL:
                    break;

                case DUST_AND_JEWEL:
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        public int getPerformance()
        {
            return performance;
        }

        private string getUri(Cell.State state, Position pos, bool displayRobot = false)
        {
            // Est-ce que l'on souhaite afficher le sprite du robot en plus de la cellule courante ?
            bool DisplayRobot()
            {
                return !displayRobot &&
                       pos.x == robotPosition.x &&
                       pos.y == robotPosition.y;
            }

            switch (state)
            {
                case DUST:
                    return DisplayRobot()
                        ? "images/wall-e-and-dust.jpg"
                        : "images/dust.jpg";
                case JEWEL:
                    return DisplayRobot()
                        ? "images/wall-e-and-jewels.jpg"
                        : "images/jewels.jpg";
                case DUST_AND_JEWEL:
                    return DisplayRobot()
                        ? "images/wall-e-and-dust-and-jewels.jpg"
                        : "images/dust-and-jewels.jpg";
                case EMPTY:
                    return DisplayRobot()
                        ? "images/wall-e.jpg"
                        : "images/empty.png";
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }
    }
}