using System.Threading;

namespace tp1_agent_aspirateur
{
    public class Agent
    {
        private const int BATTERY_MAX = 100;
        
        private Thread thread;

        private int battery = BATTERY_MAX;
        private bool isAlive = true;
        
        public void start()
        {
            thread = new Thread(update);
            thread.Start();
        }

        private void update()
        {
            while (isAlive)
            {
                // Debug.WriteLine($"Agent's battery is at {battery--}%.");
                if (battery == 0)
                {
                    isAlive = false;
                    // Debug.WriteLine("Agent is dead :(");
                }
                Thread.Sleep(500);
            }
        }
    }
}