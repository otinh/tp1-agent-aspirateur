namespace tp1_agent_aspirateur
{
    public class Wheels
    {
        public void move(Environment environment, int x, int y)
        {
            // TODO: gérer les quatre directions
            environment.robotActionUpdate(Agent.Action.MOVE_RIGHT, x, y);
        }
    }
}