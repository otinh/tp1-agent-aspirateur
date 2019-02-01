using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tp1_agent_aspirateur
{
    public class Cleaner
    {

        private void clean(Environment myEnv, int row, int column)
        {
            myEnv.robotActionUpdate("clean", row, column);
        }
    }
}
