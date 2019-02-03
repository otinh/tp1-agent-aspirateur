namespace tp1_agent_aspirateur
{
    public class Cleaner
    {
        public void clean(Environment environment, int x, int y)
        {
            environment.robotActionUpdate(Agent.Action.CLEAN, x, y);
        }
    }
}