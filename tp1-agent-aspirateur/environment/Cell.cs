namespace tp1_agent_aspirateur
{
    public class Cell
    {
        public Cell(int x = 0, int y = 0, State state = State.EMPTY)
        {
            position = new Environment.Position(x, y);
            this.state = state;
        }
        
        public Cell(Environment.Position position, State state = State.EMPTY)
        {
            this.position = position;
            this.state = state;
        }

        public enum State
        {
            EMPTY,
            DUST,
            JEWEL,
            DUST_AND_JEWEL
        }
        
        public Environment.Position position { get; }
        public State state { get; set; }
    }
}