using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntsGame
{
    public class Cell
    {
        public enum CellType
        {
            Land = '.',
            Water = '%',
            Food = '*',
            Ant,
            Unseen = '?',
            Hill,
            Empty
        }

        public readonly CellType Type;
        public readonly Map OriginalMap;
        public Point Coords;

        public Cell(Point point, Map originalMap) : this(point, CellType.Unseen, originalMap)
        {
        }

        public Cell(Point point, CellType type, Map originalMap)
        {
            Coords = point;
            Type = type;
            OriginalMap = originalMap;
        }

        public override string ToString()
        {
            return (char)Type + "";
        }

        public string GetCellDesc(int id, int playerCount)
        {
            if (Type == CellType.Ant)
                return ((Ant)this).GetCellDesc(id, playerCount);
            if (Type == CellType.Hill)
                return ((Hill)this).GetCellDesc(id, playerCount);
            var cellSymbol = Type == CellType.Water 
                                ? 'w' 
                                : Type == CellType.Food 
                                    ? 'f'
                                    : '!';
            return cellSymbol + " " + Coords.Y + " " + Coords.X;
        }

        public static char GetChar(UnMovableCell landcell, MovableCell moveCell)
        {
            if (moveCell.Type == Cell.CellType.Ant && landcell.Type == Cell.CellType.Hill)
                return (char)(((Ant)moveCell).CommandNumber + 'A');
            if (moveCell.Type == Cell.CellType.Ant)
                return (char)(((Ant)moveCell).CommandNumber + 'a');
            if (landcell.Type == Cell.CellType.Hill)
            {
                var hill = (Hill)landcell;
                return (char)(hill.CommandNumber + '0');
            }
            return moveCell.Type == CellType.Empty 
                    ? (char) landcell.Type
                    : (char) moveCell.Type;
        }
    }

    public class UnMovableCell : Cell
    {
        public UnMovableCell(Point point, Map originalMap)
            : base(point, originalMap)
        {
        }

        public UnMovableCell(Point point, CellType type, Map originalMap)
            : base(point, type, originalMap)
        {
        }
    }

    public class MovableCell : Cell
    {
        public MovableCell(Point point, Map originalMap)
            : base(point, originalMap)
        {
        }

        public MovableCell(Point point, CellType type, Map originalMap)
            : base(point, type, originalMap)
        {
        }
    }

    public class Hill : UnMovableCell
    {
        public int CommandNumber;
        public bool IsDead = false;

        public Hill(Point point, int command, Map originalMap)
            : base(point, CellType.Hill, originalMap)
        {
            CommandNumber = command;
        }

        public override string ToString()
        {
            return '0' + CommandNumber + "";
        }

        public new string GetCellDesc(int id, int playerCount)
        {
            const char cellSymbol = 'h';
            return cellSymbol + " " + Coords.Y + " " + Coords.X + " " + (CommandNumber - id + playerCount) % playerCount;
        }

        public Cell GetDead()
        {
            IsDead = true;
            return this;
        }
    }

    public class Ant : MovableCell
    {
        public int CommandNumber;
        public bool IsDead = false;

        public Ant(Point point, int command, Map originalMap)
            : base(point, CellType.Ant, originalMap)
        {
            CommandNumber = command;
        }

        public void MoveLeft()
        {
            Coords = Coords + new Point(-1, 0, OriginalMap.Width, OriginalMap.Height);
        }

        public void MoveRight()
        {
            Coords = Coords + new Point(1, 0, OriginalMap.Width, OriginalMap.Height);
        }

        public void MoveDown()
        {
            Coords = Coords + new Point(0, 1, OriginalMap.Width, OriginalMap.Height);
        }

        public void MoveUp()
        {
            Coords = Coords + new Point(0, -1, OriginalMap.Width, OriginalMap.Height);
        }

        public Cell GetDead()
        {
            IsDead = true;
            return this;
        }

        public override string ToString()
        {
            return IsDead ? "!" : CommandNumber+'a'+"";
        }

        public new string GetCellDesc(int id, int playerCount)
        {
            var cellSymbol = IsDead ? 'd' : 'a';
            return cellSymbol + " " + Coords.Y + " " + Coords.X + " " + (CommandNumber - id + playerCount)%playerCount; 
        }
    }

    public class Food : MovableCell
    {
        public Food(Point point, Map originalMap)
            : base(point, CellType.Food, originalMap)
        {
        }
    }

    public class Empty : MovableCell
    {
        public Empty(Point point, Map originalMap)
            : base(point, CellType.Empty, originalMap)
        {
        }
    }
}
