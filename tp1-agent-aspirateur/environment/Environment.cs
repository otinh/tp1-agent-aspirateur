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
        private const int UPDATE_TIME = 500;

        // Texte
        private TextBlock turnText;
        private TextBlock performanceText;
        
        // Variables concernant le robot
        private int performance;
        private Position robotPosition = new Position();

        private static int turn;

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
            while (true)
            {
                generateDust();
                generateJewel();
                turn++;
                updateText();
                Thread.Sleep(UPDATE_TIME);
            }

            // ReSharper disable once FunctionNeverReturns
        }

        /* ------------------------------------------------------------------------- */

        private void updateText()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                turnText.Text = $"- Tour (env) : -\n {turn}";
                performanceText.Text = $"- Performance : -\n {performance}";
            });
        }
        
        public void setTurnText(TextBlock text)
        {
            turnText = text;
        }
        
        public void setPerformanceText(TextBlock text)
        {
            performanceText = text;
        }
        
        /* ------------------------------------------------------------------------- */

        private void initGrid()
        {
            for (var x = 0; x < MAX_X + 1; ++x)
            for (var y = 0; y < MAX_Y + 1; ++y)
                grid[x, y] = new Cell(x, y);
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
                display(randomPosition);
            });
        }

        private void display(Position position)
        {
            display(getState(position), position);
        }

        private void display(Cell.State state, Position position, bool displayRobot = false)
        {
            var sprite = getSprite(state, position, displayRobot);
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

        private void doClean(Position position)
        {
            Debug.WriteLine("Action: Clean");

            updatePerformance(CLEAN, position);
            setState(EMPTY, position);
            Application.Current.Dispatcher.Invoke(() => { display(position); });
        }

        private void doPickup(Position position)
        {
            Debug.WriteLine("Action: Pickup");

            updatePerformance(PICKUP, position);

            var currentState = getState(position);
            switch (currentState)
            {
                case JEWEL:
                    setState(EMPTY, position);
                    Application.Current.Dispatcher.Invoke(() => { display(position); });
                    break;

                case DUST_AND_JEWEL:
                    setState(DUST, position);
                    Application.Current.Dispatcher.Invoke(() => { display(position); });
                    break;

                case EMPTY:
                case DUST:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void updatePerformance(Agent.Action action, Position position)
        {
            switch (getState(position))
            {
                case JEWEL when action == CLEAN:
                    performance -= 6;
                    break;
                case JEWEL when action == PICKUP:
                    performance += 10;
                    break;

                case DUST when action == CLEAN:
                    performance += 8;
                    break;
                case DUST when action == PICKUP:
                    performance += 0;
                    break;

                case DUST_AND_JEWEL when action == CLEAN:
                    performance -= 6;
                    break;
                case DUST_AND_JEWEL when action == PICKUP:
                    performance += 10;
                    break;

                case EMPTY:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            Debug.WriteLine($"Performance: {performance}");
        }

        /* ------------------------------------------------------------------------- */

        private Cell getCell(Position position)
        {
            return grid[position.x, position.y];
        }

        public Cell[,] getGrid()
        {
            return grid;
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

        private Cell.State getState(Position position)
        {
            return getCell(position).state;
        }

        private void setState(Cell.State state, Position position)
        {
            void Set(Cell.State s)
            {
                getCell(position).state = s;
            }

            var currentState = getState(position);

            switch (state)
            {
                case DUST when currentState == JEWEL:
                case JEWEL when currentState == DUST:
                    Set(DUST_AND_JEWEL);
                    break;

                case EMPTY:
                case DUST:
                case JEWEL:
                case DUST_AND_JEWEL:
                    Set(state);
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