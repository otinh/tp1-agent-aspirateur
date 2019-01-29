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
        private const int DUST_SPAWN_CHANCE = 10;
        private const int JEWEL_SPAWN_CHANCE = 5;

        private Thread thread;
        private Grid grid;

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
            if (random() < DUST_SPAWN_CHANCE)
            {
                // TODO: accéder à MainWindow.xaml pour générer de la poussière

                // Accès à l'image
                var image = new Image();
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.UriSource = new Uri("images/wall-e.jpg", UriKind.Relative);
                bitmapImage.EndInit();
                image.Source = bitmapImage;

                // Coordonnées (x,y)
                Grid.SetColumn(image, 0);
                Grid.SetRow(image, 5);

                // TODO: Problème InvalidOperationException !
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
                    new ThreadStart(delegate { grid.Children.Add(image); }));

                Debug.WriteLine("Dust has been generated!");
            }
        }

        private void generateJewel()
        {
            if (random() > 100 - JEWEL_SPAWN_CHANCE)
            {
                // TODO: accéder à MainWindow.xaml pour générer des bijoux
                Debug.WriteLine("Jewel has been generated!");
            }
        }

        private static int random()
        {
            return new Random().Next(0, 101);
        }
    }
}