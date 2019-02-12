namespace tp1_agent_aspirateur
{
    public class Sensor
    {

        public Cell[,] observe(Environment environment)
        {
            return environment.getGrid();
        }

        public int readPerformance(Environment environment)
        {
            return environment.getPerformance();
        }

        public void resetPerformance(Environment environment)
        {
            environment.resetPerformance();
        }
    }
}
