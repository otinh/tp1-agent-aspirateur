using System;

namespace tp1_agent_aspirateur
{
    public class Wheels
    {
        public void move(Environment environment, Agent.Action action)
        {
            var position = new Environment.Position();
            switch (action)
            {
                case Agent.Action.MOVE_UP:
                    position.y--;
                    environment.updateRobotAction(Agent.Action.MOVE_UP, position);
                    break;
                case Agent.Action.MOVE_RIGHT:
                    position.x++;
                    environment.updateRobotAction(Agent.Action.MOVE_RIGHT, position);
                    break;
                case Agent.Action.MOVE_DOWN:
                    position.y++;
                    environment.updateRobotAction(Agent.Action.MOVE_DOWN, position);
                    break;
                case Agent.Action.MOVE_LEFT:
                    position.x--;
                    environment.updateRobotAction(Agent.Action.MOVE_LEFT, position);
                    break;
                case Agent.Action.CLEAN:
                    break;
                case Agent.Action.PICKUP:
                    break;
                case Agent.Action.STAY:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }
        }
    }
}