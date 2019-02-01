using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

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
        private Cell[,] grid = new Cell[MAX_X + 1, MAX_Y + 1];

        // Variables concernant le robot
        private int performance;
        private int rowRobot = 0;
        private int columnRobot = 0;

        // Variables pour gérer l'aléatoire
        private static readonly Random Rnd = new Random();
        private static readonly object SyncLock = new object();

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
            if (random(0, 100) < DUST_SPAWN_CHANCE)
            {
                generate(Cell.State.DUST);
                Debug.WriteLine("Dust has been generated!");
            }
        }

        private void generateJewel()
        {
            if (random(0, 100) < JEWEL_SPAWN_CHANCE)
            {
                generate(Cell.State.JEWEL);
                Debug.WriteLine("Jewel has been generated!");
            }
        }

        //TODO: nettoyer ce spaghetti code
        private void generate(Cell.State cell)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var x = random(MIN_X, MAX_X);
                var y = random(MIN_Y, MAX_Y);
                var sprite = getSprite(cell,x,y);

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
                                display(getSprite(Cell.State.DUST_AND_JEWEL,x,y), x, y);
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
                                display(getSprite(Cell.State.DUST_AND_JEWEL,x,y), x, y);
                                break;
                        }

                        break;
                }
            });
        }

        private Image getSprite(Cell.State state, int x, int y)
        {
            var image = new Image();
            var bitmapImage = new BitmapImage();
            var uri = "";
            switch (state)
            {
                case Cell.State.DUST:
                    uri = "images/dust.jpg";
                    if (x == rowRobot && y == columnRobot) uri = "images/wall-e-and-dust.jpg";
                    break;
                case Cell.State.JEWEL:
                    uri = "images/jewels.jpg";
                    if (x == rowRobot && y == columnRobot) uri = "images/wall-e-and-jewels.jpg";
                    break;
                case Cell.State.DUST_AND_JEWEL:
                    uri = "images/dust-and-jewels.jpg";
                    if (x == rowRobot && y == columnRobot) uri = "images/wall-e-and-dust-and-jewels.jpg";
                    break;
                case Cell.State.EMPTY:
                    uri = "images/empty.png";
                    if (x == rowRobot && y == columnRobot) uri = "images/wall-e.jpg";
                    break;
            }

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

        private static int random(int min, int max)
        {
            lock (SyncLock)
            {
                return Rnd.Next(min, max + 1);
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

        public void robotActionUpdate(string action, int row, int column)
        {
            switch (action)
            {
                case "clean":
                    //update performance
                    switch (grid[row, column].state)
                    {
                        case Cell.State.JEWEL:
                            performance -= 7;
                            break;
                        case Cell.State.DUST:
                            performance += 3;
                            break;
                        case Cell.State.DUST_AND_JEWEL:
                            performance -= 4;
                            break;
                    }

                    //clean the grid
                    grid[row, column].state = Cell.State.EMPTY;

                    //clean the display
                    var sprite = getSprite(Cell.State.EMPTY,row,column);
                    display(sprite, row, column);
                    break;

                case "catch":

                    switch (grid[row, column].state)
                    {
                        case Cell.State.JEWEL :
                            performance += 7; //update performance
                            grid[row, column].state = Cell.State.EMPTY; //clean the grid
                            var sprite_ = getSprite(Cell.State.EMPTY,row,column); //clean display
                            display(sprite_, row, column);
                            break;

                        case Cell.State.DUST_AND_JEWEL:
                            performance += 7;
                            grid[row, column].state = Cell.State.DUST;
                            var sprite__ = getSprite(Cell.State.DUST,row,column);
                            display(sprite__, row, column);
                            break;
                    }
                    break;

                case "move":
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        display(getSprite(grid[rowRobot, columnRobot].state, rowRobot, columnRobot), rowRobot, columnRobot);

                        rowRobot = row;
                        columnRobot = column;

                        display(getSprite(grid[row, column].state, row, column), row, column);
                    });
                    break;
            }
        }
    }
}