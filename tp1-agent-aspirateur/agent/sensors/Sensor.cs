namespace tp1_agent_aspirateur
{
    public class Sensor
    {

        public Cell[,] perceptMap(Environment environment)
        {
            return environment.getGrid();
        }

        public void readPerformance(Environment environment)
        {
            environment.getPerformance();
        }
    }
}
