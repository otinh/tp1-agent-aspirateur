namespace tp1_agent_aspirateur
{
    public class Vacuum
    {
        public void clean(Environment environment, Environment.Position position)
        {
            environment.updateRobotAction(Agent.Action.CLEAN, position);
        }
    }
}