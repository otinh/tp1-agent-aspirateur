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

        public static int getDistance(Environment.Position p1, Environment.Position p2)
        {
            return Math.Abs(p1.x - p2.x) + Math.Abs(p1.y - p2.y);
        }
    }
}