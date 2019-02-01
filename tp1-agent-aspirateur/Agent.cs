using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Input;

namespace tp1_agent_aspirateur
{
    public class Agent
    {
        private const int BATTERY_MAX = 100;
        public int myRow;
        public int myColumn;

        private Thread thread;
        private Environment myEnv;

        private Sensor lidar;
        private Wheels wheels;
        private Cleaner cleaner;
        private Brush brush;

        public enum Action
        {
            MOVEUP,
            MOVEDOWN,
            MOVELEFT,
            MOVERIGHT,
            CLEAN,
            CATCH,
            STAY
        }

        private Cell belief; //cellule la plus procheayant un intérêt pour nous
        private (Action,Cell) desire; //l'action que l'on désire faire
        private Action intention; //l'action que l'on va faire au prochain tour pour accomplir le desire

        private int battery = BATTERY_MAX;
        private bool isAlive = true;

        public Agent(Environment myEnv_)
        {
            myEnv = myEnv_;
            wheels = new Wheels();
            lidar = new Sensor();
            brush = new Brush();
            cleaner = new Cleaner();
        }
        public void start()
        {
            thread = new Thread(update);

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        private void update()
        {

            while (isAlive)
            {
                // Debug.WriteLine($"Agent's battery is at {battery--}%.");
                if (battery == 0)
                {
                    isAlive = false;
                    // Debug.WriteLine("Agent is dead :(");
                }
                Thread.Sleep(1500);
            }
        }

        public void move()
        {
            myRow += 1;
            myColumn += 1;
            wheels.move(myEnv, myRow, myColumn);
        }

        private List<Action> possibleAction()
        {
            List<Action> res = new List<Action>();

            if (myRow != 0) res.Add(Action.MOVELEFT);
            if (myRow != 9) res.Add(Action.MOVERIGHT);
            if (myColumn != 0) res.Add(Action.MOVEDOWN);
            if (myColumn != 9) res.Add(Action.MOVEUP);

            res.Add(Action.CATCH);
            res.Add(Action.CLEAN);
            res.Add(Action.STAY);

            return res;
        }

        private bool matchGoal(Action a)
        {
            if (a == desire.Item1) return true;

            //test pour voir si le move est bien.
            return true;
        }
        private void doAction(Cell[,] map, Environment myEnv)
        {
            switch (map[myRow,myColumn].state)
            {
                case Cell.State.DUST:
                    cleaner.clean(myEnv, myRow, myColumn);
                    break;
                case Cell.State.JEWEL:
                    brush.catchStuff(myEnv, myRow, myColumn);
                    break;
                case Cell.State.DUST_AND_JEWEL:
                    brush.catchStuff(myEnv, myRow, myColumn);
                    break;
                case Cell.State.EMPTY:
                    //wheels.move(myEnv, myRow + 1, myColumn);
                    break;
            }
        }

    }
}