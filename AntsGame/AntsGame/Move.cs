using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntsGame
{
    public class Move
    {
        public Point LastPosition;
        public Point NextPosition;

        public bool IsCorrect(Map map, int playerId)
        {
            if (Point.Distance2(LastPosition, NextPosition) > 1)
                return false;

            bool isOurAnt = map.ObjectsMap[LastPosition].Type == Cell.CellType.Ant
                            && ((Ant)map.ObjectsMap[LastPosition]).CommandNumber == playerId;
            bool isMovable = map.OriginalMap[NextPosition].Type != Cell.CellType.Water
                            && map.ObjectsMap[NextPosition].Type != Cell.CellType.Food;

            return isOurAnt && isMovable;
        }
    }
}
