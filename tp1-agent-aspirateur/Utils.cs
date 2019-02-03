using System;

namespace tp1_agent_aspirateur
{
    public static class Utils
    {
        private static readonly Random Rnd = new Random();
        private static readonly object SyncLock = new object();

        public static int random(int min, int max)
        {
            lock (SyncLock)
            {
                return Rnd.Next(min, max + 1);
            }
        }
    }
}