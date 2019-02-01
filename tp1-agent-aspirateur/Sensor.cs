using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tp1_agent_aspirateur
{
    public class Sensor
    {

        private Cell[,] perceptMap(Environment myEnv)
        {
            return myEnv.getGrid();
        }

        private void readPerformance(Environment myEnv)
        {
            myEnv.getPerformance();
        }
    }
}
