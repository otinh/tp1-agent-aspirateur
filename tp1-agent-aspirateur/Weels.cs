using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tp1_agent_aspirateur
{
    public class Weels
    {

        private void move(Environment myEnv, int row, int column)
        {
            myEnv.robotActionUpdate("move", row, column);
        }
    }
}