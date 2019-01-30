namespace tp1_agent_aspirateur
{
    public class Cell
    {
        public Cell(State state)
        {
            this.state = state;
        }

        public enum State
        {
            EMPTY,
            DUST,
            JEWEL,
            DUST_AND_JEWEL
        }

        public State state { get; set; }
    }
}