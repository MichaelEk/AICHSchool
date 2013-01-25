using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntsGame
{
    public class Map
    {
        public MapTable<UnMovableCell> OriginalMap;
        public MapTable<MovableCell> ObjectsMap;
        private MapTable<List<Cell>> KilledAntsMap;
        private bool YSymmetrical = false;

        public int Width
        {
            get { return OriginalMap.Width; }
        }
        public int Height
        {
            get { return OriginalMap.Height; }
        }

        public int TurnsLeft = int.MaxValue;
        public int Turn = 0;
        public int ViewRadius2 = 55;
        public int AttackRadius2 = 5;
        public int SpawnRadius2 = 1;

        public int[] GetPointStatistic(Point point, int radius2)
        {
            var antsCounts = new int[Armies.Length];

            var maxDist = (int) Math.Sqrt(radius2);

            for (int i = -maxDist; i <= maxDist; i++)
            {
                for (int j = -maxDist; j <= maxDist; j++)
                {
                    var vector = new Point(i, j);
                    if (vector.Norm2() > radius2)
                        continue;
                    var newPoint = point + vector;
                    if (ObjectsMap[newPoint].Type != Cell.CellType.Ant)
                        continue;
                    antsCounts[((Ant)ObjectsMap[newPoint]).CommandNumber]++;
                }
            }

            return antsCounts;
        }

        public void PlayMove(AbstractPlayer[] players)
        {
            Turn++;
            var newCells = new Dictionary<Point, Ant>();

            for(int playerId = 0; playerId < players.Length; playerId++)
            {
                foreach (var move in players[playerId].PlayMove(GetVisibleCells(playerId), Turn, this))
                {
                    if(move.IsCorrect(this, playerId))
                    {
                        var ant = (Ant)ObjectsMap[move.LastPosition];
                        ObjectsMap[move.LastPosition] = new Empty(move.LastPosition, this);
                        ant.Coords = move.NextPosition;
                        if(newCells.ContainsKey(ant.Coords))
                        {
                            var oldAnt = newCells[ant.Coords];
                            KilledAntsMap[ant.Coords].Add(ant.GetDead());
                            if (oldAnt != null)
                                KilledAntsMap[oldAnt.Coords].Add(oldAnt.GetDead());
                            newCells[ant.Coords] = null;
                        }
                        else
                            newCells[ant.Coords] = ant;
                    }
                }
            }
            foreach (var newCell in newCells)
            {
                if (ObjectsMap[newCell.Key].Type == Cell.CellType.Ant)
                {
                    KilledAntsMap[newCell.Key].Add(((Ant)ObjectsMap[newCell.Key]).GetDead());
                    if(newCell.Value != null)
                        KilledAntsMap[newCell.Key].Add(newCell.Value.GetDead());
                    ObjectsMap[newCell.Key] = new Empty(newCell.Key, this);
                }
                else
                {
                    if (newCell.Value != null)
                        ObjectsMap[newCell.Key] = newCell.Value;
                }
            }
        }

        private IEnumerable<Cell> GetNearCells(Point point, int radius2)
        {
            var maxDist = (int)Math.Sqrt(radius2);

            for (int i = -maxDist; i <= maxDist; i++)
            {
                for (int j = -maxDist; j <= maxDist; j++)
                {
                    var vector = new Point(i, j);
                    if (vector.Norm2() > radius2)
                        continue;
                    var visiblePoint = point + vector;
                    if (OriginalMap[visiblePoint].Type != Cell.CellType.Land)
                        yield return OriginalMap[visiblePoint];
                    if (ObjectsMap[visiblePoint].Type != Cell.CellType.Empty)
                        yield return ObjectsMap[visiblePoint];

                    var dead = KilledAntsMap[visiblePoint];
                    foreach (var cell in dead)
                    {
                        yield return cell;
                    }
                }
            }
        }

        private List<Cell> GetVisibleCells(int armyId)
        {
            return Armies[armyId].Ants.SelectMany(ant => GetNearCells(ant.Coords, ViewRadius2)).ToList();
        }

        public void War()
        {
            var ants = Armies.SelectMany(army => army.Ants).ToList();
            var enemiesCount = GetEnemiesCountForAnts(ants);
            var deadAnts = new HashSet<Ant>(
                ants.Where(ant => IsAntDead(ant, enemiesCount))
                );
            foreach (var antArmy in Armies)
            {
                antArmy.Ants = antArmy.Ants.Where(ant => !deadAnts.Contains(ant)).ToList();
            }
            foreach (var deadAnt in deadAnts)
            {
                ObjectsMap[deadAnt.Coords] = new Empty(deadAnt.Coords, this);
                KilledAntsMap[deadAnt.Coords].Add(deadAnt.GetDead());
            }
        }

        public Dictionary<Ant, int> GetEnemiesCountForAnts(List<Ant> ants)
        {
            var result = new Dictionary<Ant, int>();
            ants.ForEach(ant => result.Add(ant, GetEnemiesCount(ant)));
            return result;
        }

        public bool IsAntDead(Ant ant, Dictionary<Ant, int> enemiesCount)
        {
            var antEnemiesCount = enemiesCount[ant];

            return GetNearCells(ant.Coords, AttackRadius2)
                        .Where(cell => cell is Ant && ((Ant)cell).CommandNumber != ant.CommandNumber && !((Ant)cell).IsDead)
                        .Select(cell => (Ant) cell)
                        .Any(enemy => enemiesCount[enemy] <= antEnemiesCount);
        }

        public void HillsRaze()
        {
            foreach (var antArmy in Armies)
            {
                var newHills = new List<Hill>();
                foreach (var hill in antArmy.Hills)
                {
                    if(ObjectsMap[hill.Coords].Type == Cell.CellType.Ant && ((Ant)ObjectsMap[hill.Coords]).CommandNumber != hill.CommandNumber)
                    {
                        hill.GetDead();
                    }
                    else
                    {
                        newHills.Add(hill);
                    }
                }
                antArmy.Hills = newHills;
            }
        }

        public int GetEnemiesCount(Ant ant)
        {
            var commandId = ant.CommandNumber;

            var statistic = GetPointStatistic(ant.Coords, AttackRadius2);
            int sum = 0;
            for (int i = 0; i < statistic.Length; i++ )
            {
                if (i != commandId)
                    sum += statistic[i];
            }
            return sum;
        }

        public void FoodHarvesting()
        {
            var rest = new List<Food>();

            foreach (var food in Food)
            {
                var stat = GetPointStatistic(food.Coords, SpawnRadius2);
                var nearArmiesCount = stat.Where(st => st > 0).Count();

                if (nearArmiesCount > 1)
                    continue;
                var armyIdx = GetNotZeroIdx(stat);
                if (armyIdx < 0)
                    rest.Add(food);
                else
                {
                    ObjectsMap[food.Coords] = new Empty(food.Coords, this);
                    Armies[armyIdx].HiveSize++;
                }
            }

            Food = rest;
        }

        public void AntCreation()
        {
            foreach (var army in Armies)
            {
                var emptyHills = army.Hills.Where(hill => ObjectsMap[hill.Coords].Type == Cell.CellType.Empty).ToList();
                while (emptyHills.Count != 0 && army.HiveSize != 0)
                {
                    var idx = Rndom.Next(emptyHills.Count);
                    var ant = new Ant(emptyHills[idx].Coords, army.Idx, this);
                    ObjectsMap[emptyHills[idx].Coords] = ant;
                    army.Ants.Add(ant);
                    emptyHills.RemoveAt(idx);
                    army.HiveSize--;
                }
            }
        }

        static readonly Random Rndom = new Random();

        public void FoodGeneration(double prob)
        {
            prob -= Rndom.NextDouble();
            while (prob >= 0)
            {
                bool foodPlaced = false;
                while (!foodPlaced)
                {
                    int dx = Rndom.Next(Width);
                    int dy = Rndom.Next(Height);

                    var dPoint = new Point(dx, dy);
                    var d2Point = YSymmetrical ? new Point(dx, -dy, Width, Height) : new Point(dx, dy, Width, Height);

                    if (OriginalMap[Hills[0].Coords + dPoint].Type != Cell.CellType.Land)
                        continue;
                    foreach (var hill in Hills)
                    {
                        var newPoint = hill.Coords.Y < Height / 2 ? hill.Coords + dPoint : hill.Coords + d2Point;
                        if (ObjectsMap[newPoint].Type == Cell.CellType.Empty)
                        {
                            var food = new Food(newPoint, this);
                            ObjectsMap[newPoint] = food;
                            Food.Add(food);
                        }
                    }

                    foodPlaced = true;
                }
                prob -= Rndom.NextDouble();
            }
        }

        private int GetNotZeroIdx(int[] counts)
        {
            for(int i = 0; i < counts.Length; i++)
            {
                if (counts[i] > 0)
                    return i;
            }
            return -1;
        }

        public Map(string seializedMap, bool isYSymmetrical)
        {
            var lines = seializedMap.Split('\n');
            YSymmetrical = isYSymmetrical;
            int jdx = 0;
            int rows = 0;
            int cols = 0;
            foreach (var line in lines)
            {
                if(line.StartsWith("players"))
                {
                    var playersCount = int.Parse(line.Substring(7).Trim());
                    Armies = new AntArmy[playersCount];
                    for(int i = 0; i < playersCount; i++)
                        Armies[i] = new AntArmy{Idx = i};
                }
                else if (line.StartsWith("rows"))
                {
                    rows = int.Parse(line.Substring(4).Trim());
                }
                else if (line.StartsWith("cols"))
                {
                    cols = int.Parse(line.Substring(4).Trim());
                }
                else if (line.StartsWith("m"))
                {
                    if (OriginalMap == null)
                        OriginalMap = new MapTable<UnMovableCell>(cols, rows);
                    if (ObjectsMap == null)
                        ObjectsMap = new MapTable<MovableCell>(cols, rows);
                    if (KilledAntsMap == null)
                        KilledAntsMap = new MapTable<List<Cell>>(cols, rows);

                    var mapPart = line.Substring(1).Trim();

                    for (int idx = 0; idx < mapPart.Length; idx++)
                    {
                        var point = new Point(idx, jdx, Width, Height);
                        InitCell(point, mapPart[idx]);

                        if (OriginalMap[point].Type == Cell.CellType.Hill)
                            Hills.Add((Hill)OriginalMap[point]);
                    }

                    jdx++;
                }
            }
        }

        public string Serialize()
        {
            var str = "";
            str += "rows " + Height + "\n";
            str += "cols " + Width + "\n";
            str += "players " + Armies.Length + "\n";

            for(int j = 0; j < Height; j++)
            {
                str += "m ";
                for (int i = 0; i < Width; i++ )
                {
                    str += Cell.GetChar(OriginalMap[i, j], ObjectsMap[i, j]);
                }
                str += "\n";
            }

            return str;
        }

        private void InitCell(Point point, char c)
        {
            KilledAntsMap[point] = new List<Cell>(); 
            if (c >= '0' && c <= '9')
            {
                int command = c - '0';
                var hill = new Hill(point, command, this);
                Armies[command].Hills.Add(hill);
                OriginalMap[point] = hill;
                ObjectsMap[point] = new Empty(point, this);

            }
            else if (c >= 'a' && c <= 'j')
            {
                int command = c - 'a';
                var ant = new Ant(point, command, this);
                Armies[command].Ants.Add(ant);
                OriginalMap[point] = new UnMovableCell(point, Cell.CellType.Land, this);
                ObjectsMap[point] = ant;
            }
            else if (c >= 'A' && c <= 'J')
            {
                int command = c - 'A';
                var hill = new Hill(point, command, this);
                Armies[command].Hills.Add(hill);
                var ant = new Ant(point, command, this);
                Armies[command].Ants.Add(ant);
                OriginalMap[point] = hill;
                ObjectsMap[point] = ant;
            }
            else if (c == '*')
            {
                var food = new Food(point, this);
                Food.Add(food);
                OriginalMap[point] = new UnMovableCell(point, Cell.CellType.Land, this);
                ObjectsMap[point] = food;
            }
            else if (c == '!')
            {
                OriginalMap[point] = new UnMovableCell(point, Cell.CellType.Land, this);
                ObjectsMap[point] = new Empty(point, this);
                KilledAntsMap[point].Add(new Ant(point, -1, this).GetDead());
            }
            else if (c == '%')
            {
                OriginalMap[point] = new UnMovableCell(point, Cell.CellType.Water, this);
                ObjectsMap[point] = new Empty(point, this);
            }
            else if (c == '.')
            {
                OriginalMap[point] = new UnMovableCell(point, Cell.CellType.Land, this);
                ObjectsMap[point] = new Empty(point, this);
            }
            else
            {
                OriginalMap[point] = new UnMovableCell(point, Cell.CellType.Unseen, this);
                ObjectsMap[point] = new Empty(point, this);
            }
        }

        public AntArmy[] Armies;
        public List<Food> Food = new List<Food>(); 
        public List<Hill> Hills = new List<Hill>();  
    }

    public class AntArmy
    {
        public int Idx;
        public List<Ant> Ants = new List<Ant>();
        public List<Hill> Hills = new List<Hill>();
        public int HiveSize = 1;
        public int Points = 0;
    }
}
