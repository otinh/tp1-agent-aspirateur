namespace tp1_agent_aspirateur
{
    public class Brush
    {
        public void pickup(Environment environment, int x, int y)
        {
            environment.robotActionUpdate(Agent.Action.CATCH, x, y);
        }
    }
}