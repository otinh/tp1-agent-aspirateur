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
        private const int DUST_SPAWN_CHANCE = 50;
        private const int JEWEL_SPAWN_CHANCE = 5;

        // La grille et son affichage
        private Thread thread;
        private readonly Grid gridDisplay;
        private Cell[,] grid = new Cell[MAX_X + 1, MAX_Y + 1];

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
            for (var i = 0; i < MAX_X; ++i)
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
                var sprite = getSprite(cell);

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
                                display(getSprite(Cell.State.DUST_AND_JEWEL), x, y);
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
                                display(getSprite(Cell.State.DUST_AND_JEWEL), x, y);
                                break;
                        }

                        break;
                }
            });
        }

        private Image getSprite(Cell.State state)
        {
            var image = new Image();
            var bitmapImage = new BitmapImage();
            var uri = state == Cell.State.DUST ? "images/dust.jpg" : "images/jewels.jpg";

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
    }
}