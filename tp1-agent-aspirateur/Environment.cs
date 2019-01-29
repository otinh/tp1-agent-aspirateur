using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace tp1_agent_aspirateur
{
    public class Environment
    {
        private enum Object
        {
            DUST,
            JEWEL,
            DUST_AND_JEWEL
        }

        // Coordonnées de la grille
        private const int MIN_X = 0;
        private const int MAX_X = 9;
        private const int MIN_Y = 0;
        private const int MAX_Y = 9;

        // Chance d'apparaître chaque tour en %
        private const int DUST_SPAWN_CHANCE = 10;
        private const int JEWEL_SPAWN_CHANCE = 5;

        private Thread thread;
        private Grid grid;

        private static readonly Random rnd = new Random();
        private static readonly object syncLock = new object();

        private int performance { get; set; }

        public Environment(Grid grid)
        {
            this.grid = grid;
        }

        public void start()
        {
            thread = new Thread(update);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        private void update()
        {
            var i = 0;
            while (true)
            {
                generateDust();
                generateJewel();
                Debug.WriteLine($"Turn: {++i}");
                Thread.Sleep(500);
            }
        }

        private void generateDust()
        {
            if (random(0, 100) < DUST_SPAWN_CHANCE)
            {
                addToGrid(Object.DUST);
                Debug.WriteLine("Dust has been generated!");
            }
        }

        private void generateJewel()
        {
            if (random(0, 100) > 100 - JEWEL_SPAWN_CHANCE)
            {
                addToGrid(Object.JEWEL);
                Debug.WriteLine("Jewel has been generated!");
            }
        }

        private void addToGrid(Object o)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var x = random(MIN_X, MAX_X);
                var y = random(MIN_Y, MAX_Y);
                var image = getImage(o);
                setCoordinates(image, x, y);
                grid.Children.Add(image);
            });
        }

        private Image getImage(Object o)
        {
            var image = new Image();
            var bitmapImage = new BitmapImage();
            var uri = o == Object.DUST ? "images/dust.jpg" : "images/jewels.jpg";

            bitmapImage.BeginInit();
            bitmapImage.UriSource = new Uri(uri, UriKind.Relative);
            bitmapImage.EndInit();

            image.Source = bitmapImage;
            return image;
        }

        private void setCoordinates(UIElement image, int x, int y)
        {
            Grid.SetColumn(image, x);
            Grid.SetRow(image, y);
        }

        private static int random(int min, int max)
        {
            lock (syncLock)
            {
                return rnd.Next(min, max + 1);
            }
        }
    }
}