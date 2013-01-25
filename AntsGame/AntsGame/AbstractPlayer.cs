using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace AntsGame
{
    public abstract class AbstractPlayer
    {
        private int _playerCount;
        private HashSet<Cell> _visibleWater = new HashSet<Cell>();
        public int PlayerId;

        public AbstractPlayer(int id, int playerCount)
        {
            PlayerId = id;
            _playerCount = playerCount;
        }

        protected string GetZeroTurnInfo(Map map)
        {
            return String.Format(@"turn 0
loadtime 3000  
turntime 1000  
rows {0}  
cols {1}  
turns {2}  
viewradius2 {3}  
attackradius2 {4}  
spawnradius2 {5}
player_seed 42
ready", map.Height, map.Width, 1000000, map.ViewRadius2, map.AttackRadius2, map.SpawnRadius2);
        }

        protected string GetVisibleMapDesc(List<Cell> visibleCells, int turn)
        {
            var cells = visibleCells.Where(cell =>
                {
                    if (cell.Type != Cell.CellType.Water)
                        return true;
                    if (!_visibleWater.Contains(cell))
                    {
                        _visibleWater.Add(cell);
                        return true;
                    }
                    return false;
                });
            return String.Format(@"turn {0}
{1}go", turn, cells.Aggregate("", (a, b) => a + b.GetCellDesc(PlayerId, _playerCount) + "\n"));
        }

        static readonly Dictionary<char, Point> Moves = new Dictionary<char, Point>(); 
        static AbstractPlayer()
        {
            Moves.Add('w', new Point(-1, 0));
            Moves.Add('n', new Point(0, -1));
            Moves.Add('e', new Point(1, 0));
            Moves.Add('s', new Point(0, 1));
        }

        protected List<Move> GetMoves(StreamReader input, Map map)
        {
            var moves = new List<Move>();

            string line;

            while ((line = input.ReadLine()) == null || !line.StartsWith("go"))
            {
                var move = GetMove(line, map);
                if(move != null)
                    moves.Add(move);
            }

            return moves;
        }


        protected Move GetMove(string line, Map map)
        {
            if (line == null)
            {
                Thread.Sleep(10);
                return null;
            }
            line = line.ToLower();

            if (!line.StartsWith("o"))
                return null;

            var array = line.Split(' ');
            if (array.Length != 4)
                return null;
            int y;
            int x;
            if (!int.TryParse(array[1], out y))
                return null;
            if (!int.TryParse(array[2], out x))
                return null;
            if (array[3].Length == 0 || !Moves.ContainsKey(array[3][0]))
                return null;
            var move = new Move { LastPosition = new Point(x, y, map.Width, map.Height), NextPosition = new Point(x, y, map.Width, map.Height) + Moves[array[3][0]] };
            if (move.IsCorrect(map, PlayerId))
                return move;

            return null;
        }

        public abstract List<Move> PlayMove(List<Cell> visibleCells, int turn, Map map);

        public abstract void EndGame(int[] score);
    }

    public class LazyPlayer : AbstractPlayer
    {
        Random _rand = new Random();

        public LazyPlayer(int id, int playerCount)
            : base(id, playerCount)
        {
        }

        public override List<Move> PlayMove(List<Cell> visibleCells, int turn, Map map)
        {
            return new List<Move>();
        }

        public override void EndGame(int[] score)
        {
        }
    }

    public class StupidPlayer : AbstractPlayer
    {
        Random _rand = new Random();
        char[] _sides = new[]{'W', 'E', 'N', 'S'};

        public StupidPlayer(int id, int playerCount)
            : base(id, playerCount)
        {
            Thread.Sleep(100);
        }

        public override List<Move> PlayMove(List<Cell> visibleCells, int turn, Map map)
        {
            var used = new HashSet<Point>();
            var moves = new List<Move>();
            foreach (var ant in map.Armies[PlayerId].Ants)
            {
                int trys = 4;
                Move move = null;
                while ((move == null || used.Contains(move.NextPosition)) && trys > 0)
                {
                    move = GetMove("o " + ant.Coords.Y + " " + ant.Coords.X + " " + _sides[_rand.Next(4)], map);
                    trys--;
                }
                if (move != null && !used.Contains(move.NextPosition))
                {
                    moves.Add(move);
                    used.Add(move.NextPosition);
                }
            }
            return moves;
        }

        public override void EndGame(int[] score)
        {
        }
    }
}
