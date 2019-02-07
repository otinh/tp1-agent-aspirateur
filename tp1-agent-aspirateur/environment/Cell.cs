using System.Collections.Generic;

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

        public static bool operator ==(Cell c1, Cell c2)
        {
            return c1.position.x == c2.position.x && c1.position.y == c2.position.y;
        }

        public static bool operator !=(Cell c1, Cell c2)
        {
            return !(c1 == c2);
        }

        public bool isUpFrom(Cell cell)
        {
            return position.y < cell.position.y;
        }
        
        public bool isRightFrom(Cell cell)
        {
            return position.x > cell.position.x;
        }

        public bool isDownFrom(Cell cell)
        {
            return position.y > cell.position.y;
        }

        public bool isLeftFrom(Cell cell)
        {
            return position.x < cell.position.x;
        }

        // Retourne les cellules voisines dans le sens horaire en commençant par le haut.
        public static IEnumerable<Cell> getNeighborCells(Cell cell, Cell[,] grid)
        {
            var list = new List<Cell>();

            if (cell.position.y > Environment.MIN_Y)
                list.Add(grid[cell.position.x, cell.position.y - 1]);

            if (cell.position.x < Environment.MAX_X)
                list.Add(grid[cell.position.x + 1, cell.position.y]);

            if (cell.position.y < Environment.MAX_Y)
                list.Add(grid[cell.position.x, cell.position.y + 1]);

            if (cell.position.x > Environment.MIN_X)
                list.Add(grid[cell.position.x - 1, cell.position.y]);

            return list;
        }

        // Retourne le chemin de `start` à `end` en se basant sur le path fourni.
        public static Stack<Cell> getCellPath(Cell start, Cell end, IReadOnlyDictionary<Cell, Cell> path)
        {
            var cellPath = new Stack<Cell>();

            var cell = end;
            while (cell != start)
            {
                cellPath.Push(cell);
                cell = path[cell];
            }

            cellPath.Push(start);

            return cellPath;
        }
    }
}