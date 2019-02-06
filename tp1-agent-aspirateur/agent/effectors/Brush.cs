namespace tp1_agent_aspirateur
{
    public class Brush
    {
        public void pickup(Environment environment, Environment.Position position)
        {
            environment.updateRobotAction(Agent.Action.PICKUP, position);
        }
    }
}