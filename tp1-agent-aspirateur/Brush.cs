using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tp1_agent_aspirateur
{
    public class Brush
    {

        private void catchStuff(Environment myEnv, int row, int column)
        {
            myEnv.robotActionUpdate("catch", row, column);
        }
    }
}
