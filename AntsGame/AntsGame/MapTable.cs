using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntsGame
{
    public class MapTable<T>
    {
        private T[,] _table;

        public T this[Point point]
        {
            get { return _table[point.Y, point.X]; }
            set { _table[point.Y, point.X] = value; }
        }

        public T this[int x, int y]
        {
            get { return _table[y, x]; }
            set { _table[y, x] = value; }
        }

        public MapTable(int width, int height)
        {
            _table = new T[height, width];
        }

        public int Width
        {
            get { return _table.GetLength(1); }
        }
        public int Height
        {
            get { return _table.GetLength(0); }
        }
    }
}
