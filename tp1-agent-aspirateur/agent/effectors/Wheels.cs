namespace tp1_agent_aspirateur
{
    public class Wheels
    {
        public void move(Environment environment, Agent.Action action)
        {
            int x = 0, y = 0;
            switch (action)
            {
                    case Agent.Action.MOVE_UP:
                        y--;
                        break;
                    case Agent.Action.MOVE_RIGHT:
                        x++;
                        break;
                    case Agent.Action.MOVE_DOWN:
                        y++;
                        break;
                    case Agent.Action.MOVE_LEFT:
                        x--;
                        break;
            }
            environment.robotActionUpdate(Agent.Action.MOVE_RIGHT, x, y);
        }
    }
}