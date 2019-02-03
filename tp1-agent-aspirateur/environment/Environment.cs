using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using static tp1_agent_aspirateur.Agent.Action;
using static tp1_agent_aspirateur.Utils;

namespace tp1_agent_aspirateur
{
    public class Environment
    {
        // Coordonnées de la grille
        private const int MIN_X = 0;
        private const int MAX_X = 9;
        private const int MIN_Y = 0;
        private const int MAX_Y = 9;

        // Chance d'apparaître chaque tour en %
        private const int DUST_SPAWN_CHANCE = 15;
        private const int JEWEL_SPAWN_CHANCE = 5;

        // La grille et son affichage
        private Thread thread;
        private readonly Grid gridDisplay;
        private readonly Cell[,] grid = new Cell[MAX_X + 1, MAX_Y + 1];

        // Variables concernant le robot
        private int performance;
        private int xRobot;
        private int yRobot;

        // TODO: utiliser cette structure au lieu de rowRobot et columnRobot
        struct Robot
        {
            private int x, y;
        }

        public Environment(Grid gridDisplay)
        {
            this.gridDisplay = gridDisplay;
        }

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
                Debug.WriteLine($"Turn: {++turn}");
                Thread.Sleep(500);
            }
        }

        private void initGrid()
        {
            for (var i = 0; i < MAX_X + 1; ++i)
            for (var j = 0; j < MAX_Y + 1; ++j)
                grid[i, j] = new Cell(Cell.State.EMPTY);
        }

        private void generateDust()
        {
            if (random(0, 100) < DUST_SPAWN_CHANCE) generate(Cell.State.DUST);
        }

        private void generateJewel()
        {
            if (random(0, 100) < JEWEL_SPAWN_CHANCE) generate(Cell.State.JEWEL);
        }

        //TODO: nettoyer ce spaghetti code
        private void generate(Cell.State cell)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var x = random(MIN_X, MAX_X);
                var y = random(MIN_Y, MAX_Y);
                var sprite = getSprite(cell, x, y);

                switch (cell)
                {
                    case Cell.State.DUST:

                        switch (grid[x, y].state)
                        {
                            case Cell.State.EMPTY:
                                grid[x, y].state = Cell.State.DUST;
                                display(sprite, x, y);
                                break;
                            case Cell.State.JEWEL:
                                grid[x, y].state = Cell.State.DUST_AND_JEWEL;
                                sprite = getSprite(Cell.State.DUST_AND_JEWEL, x, y);
                                display(sprite, x, y);
                                break;
                        }

                        break;
                    case Cell.State.JEWEL:

                        switch (grid[x, y].state)
                        {
                            case Cell.State.EMPTY:
                                grid[x, y].state = Cell.State.JEWEL;
                                display(sprite, x, y);
                                break;
                            case Cell.State.DUST:
                                grid[x, y].state = Cell.State.DUST_AND_JEWEL;
                                sprite = getSprite(Cell.State.DUST_AND_JEWEL, x, y);
                                display(sprite, x, y);
                                break;
                        }

                        break;
                }
            });
        }

        private Image getSprite(Cell.State state, int x, int y, bool isRobotMoving = false)
        {
            var image = new Image();
            var bitmapImage = new BitmapImage();
            var uri = getUri(state, x, y, isRobotMoving);

            bitmapImage.BeginInit();
            bitmapImage.UriSource = new Uri(uri, UriKind.Relative);
            bitmapImage.EndInit();

            image.Source = bitmapImage;
            return image;
        }

        private void display(UIElement sprite, int x, int y)
        {
            Grid.SetColumn(sprite, x);
            Grid.SetRow(sprite, y);
            gridDisplay.Children.Add(sprite);
        }

        private string getUri(Cell.State state, int x, int y, bool isRobotMoving = false)
        {
            switch (state)
            {
                case Cell.State.DUST:
                    return !isRobotMoving && x == xRobot && y == yRobot
                        ? "images/wall-e-and-dust.jpg"
                        : "images/dust.jpg";
                case Cell.State.JEWEL:
                    return !isRobotMoving && x == xRobot && y == yRobot
                        ? "images/wall-e-and-jewels.jpg"
                        : "images/jewels.jpg";
                case Cell.State.DUST_AND_JEWEL:
                    return !isRobotMoving && x == xRobot && y == yRobot
                        ? "images/wall-e-and-dust-and-jewels.jpg"
                        : "images/dust-and-jewels.jpg";
                case Cell.State.EMPTY:
                    return !isRobotMoving && x == xRobot && y == yRobot ? "images/wall-e.jpg" : "images/empty.png";
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        public Cell[,] getGrid()
        {
            return grid;
        }

        public int getPerformance()
        {
            return performance;
        }

        private void updatePerformance(Cell cell, Agent.Action action)
        {
            switch (cell.state)
            {
                case Cell.State.JEWEL when action == CLEAN:
                    performance -= 7;
                    break;
                case Cell.State.JEWEL when action == CATCH:
                    performance += 7;
                    break;
                case Cell.State.DUST:
                    performance += 3;
                    break;
                case Cell.State.DUST_AND_JEWEL when action == CLEAN:
                    performance -= 4;
                    break;
                case Cell.State.DUST_AND_JEWEL when action == CATCH:
                    performance += 7;
                    break;
                case Cell.State.EMPTY:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(cell), cell, null);
            }
        }

        public void robotActionUpdate(Agent.Action action, int x, int y)
        {
            switch (action)
            {
                case CLEAN:
                    updatePerformance(grid[x, y], CLEAN);
                    grid[x, y].state = Cell.State.EMPTY;

                    // TODO: gérer le thread
                    //clean the display
                    var sprite = getSprite(Cell.State.EMPTY, x, y);
                    display(sprite, x, y);
                    break;

                case CATCH:
                    updatePerformance(grid[x, y], CATCH);
                    switch (grid[x, y].state)
                    {
                        case Cell.State.JEWEL:
                            grid[x, y].state = Cell.State.EMPTY;
                            var sprite_ = getSprite(Cell.State.EMPTY, x, y);
                            // TODO: gérer le thread
                            display(sprite_, x, y);
                            break;

                        case Cell.State.DUST_AND_JEWEL:
                            grid[x, y].state = Cell.State.DUST;
                            var sprite__ = getSprite(Cell.State.DUST, x, y);
                            // TODO: gérer le thread
                            display(sprite__, x, y);
                            break;
                    }

                    break;

                case MOVE_UP:
                case MOVE_DOWN:
                case MOVE_LEFT:
                case MOVE_RIGHT:
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var originSprite = getSprite(grid[xRobot, yRobot].state, xRobot, yRobot, true);
                        display(originSprite, xRobot, yRobot);

                        xRobot = x;
                        yRobot = y;

                        var destinationSprite = getSprite(grid[x, y].state, x, y);
                        display(destinationSprite, x, y);
                    });
                    break;
            }
        }
    }
}