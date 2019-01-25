using System;
using System.Diagnostics;
using System.Threading;

namespace tp1_agent_aspirateur
{
    public class Environment
    {
        private const int DUST_SPAWN_CHANCE = 10;
        private const int JEWEL_SPAWN_CHANCE = 5;

        private Thread thread;

        private int performance { get; set; }

        public void start()
        {
            thread = new Thread(update);
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

        private int random()
        {
            return new Random().Next(0, 101);
        }
    }
}