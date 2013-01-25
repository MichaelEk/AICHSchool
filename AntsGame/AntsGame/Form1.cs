using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AntsGame
{
    public partial class Form1 : Form
    {
        private Map _map;
        private AbstractPlayer[] _players;
        private Thread _gameThread;
        private const int SquareSize = 8;
        private int TimeOut = 10;
        Dictionary<string, string> _conf = new Dictionary<string, string>(); 

        private static readonly Color[] CommandsColors = new[]
            {
                Color.Red,
                Color.LightBlue,
                Color.Green,
                Color.Yellow,
                Color.LightGreen,
                Color.Orange,
                Color.Violet,
            };

        public Form1()
        {
            InitConfig();
            InitMap();
            _players = new AbstractPlayer[_map.Armies.Length];
            for (int i = 0; i < _map.Armies.Length; i++ )
            {
                _players[i] = InitPlayer(i);
            }
            InitializeComponent();

            for (int i = 0; i < 3; i++ )
            {
                _map.FoodGeneration(1);
            }
            _map.AntCreation();

            DrawMap();

            _gameThread = new Thread(PlayGame);
            _gameThread.Start();
            Closing += (sender, args) =>
                {
                    foreach (var player in _players)
                    {
                        player.EndGame(new int[0]);
                    }
                    _gameThread.Abort();
                };
        }

        private void InitConfig()
        {
            using(var reader = new StreamReader("config.txt"))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var split = line.Split('\t');
                    if(split.Length == 2)
                        _conf.Add(split[0], split[1]);
                }
            }
            if (_conf.ContainsKey("TimeOut"))
                TimeOut = int.Parse(_conf["TimeOut"]);
        }

        private void InitMap()
        {
            _map = new Map(File.ReadAllText(_conf["Map"]), _conf.ContainsKey("Symmetric") && bool.Parse(_conf["Symmetric"]));
        }

        private AbstractPlayer InitPlayer(int id)
        {
            var key = "Player" + id;
            if(!_conf.ContainsKey(key))
                return new LazyPlayer(id, _map.Armies.Length);
            var val = _conf[key];
            if (val.ToLower() == "stupid")
                return new StupidPlayer(id, _map.Armies.Length);
            if (val.ToLower() == "lazy")
                return new LazyPlayer(id, _map.Armies.Length);
            return new ProcessPlayer(val, _map, id, _map.Armies.Length);
        }

        private void PlayGame()
        {
            while (_map.TurnsLeft > 0)
            {
                _map.PlayMove(_players);
                _map.War();
                _map.HillsRaze();
                _map.AntCreation();
                _map.FoodHarvesting();
                _map.FoodGeneration(0.3);
                FormAccessDelegate dDelegate = DrawMap;
                DrawedMap.Invoke(dDelegate);
                Thread.Sleep(5);
            }
        }

        private delegate void FormAccessDelegate();

        private void DrawMap()
        {
            DrawedMap.Width = _map.Width*SquareSize;
            DrawedMap.Height = _map.Height*SquareSize;
            Height = DrawedMap.Height + 50;
            Width = DrawedMap.Width + 31;
            DrawedMap.Image = new Bitmap(DrawedMap.Width, DrawedMap.Height);
            var graphics = Graphics.FromImage(DrawedMap.Image);
            var land = new SolidBrush(Color.Brown);
            var water = new SolidBrush(Color.DarkBlue);
            var unseen = new SolidBrush(Color.Black);
            var emptyHillBrush = new SolidBrush(Color.Black);
            var hillBrush = new SolidBrush(Color.LightSalmon);
            var foodBrush = new SolidBrush(Color.Bisque);

            graphics.FillRectangle(new SolidBrush(Color.Brown), new Rectangle(0, 0, 100, 100));

            for(int j = 0; j < _map.Height; j++)
            {
                for (int i = 0; i < _map.Width; i++)
                {
                    if (_map.OriginalMap[i, j].Type == Cell.CellType.Water)
                    {
                        graphics.FillRectangle(water, new Rectangle(i*SquareSize, j*SquareSize, SquareSize, SquareSize));
                    }
                    else
                    {
                        graphics.FillRectangle(land, new Rectangle(i*SquareSize, j*SquareSize, SquareSize, SquareSize));
                    }
                }
            }

            for (int j = 0; j < _map.Height; j++)
            {
                for (int i = 0; i < _map.Width; i++)
                {
                    if (_map.OriginalMap[i, j].Type == Cell.CellType.Hill)
                    {
                        graphics.FillEllipse(hillBrush,
                                             new Rectangle(i * SquareSize - SquareSize / 2, j * SquareSize - SquareSize / 2,
                                                           2 * SquareSize, 2 * SquareSize));
                        var hill = (Hill)_map.OriginalMap[i, j];
                        if(!hill.IsDead)
                        {
                            graphics.FillEllipse(new SolidBrush(CommandsColors[hill.CommandNumber]),
                                                 new Rectangle(i * SquareSize, j * SquareSize, SquareSize, SquareSize));
                            graphics.FillEllipse(emptyHillBrush,
                                                 new Rectangle(i * SquareSize + 1, j * SquareSize + 1, SquareSize - 2, SquareSize - 2));
                        }
                    }
                }
            }


            for (int j = 0; j < _map.Height; j++)
            {
                for (int i = 0; i < _map.Width; i++)
                {
                    if (_map.ObjectsMap[i, j].Type == Cell.CellType.Food)
                    {
                        graphics.FillRectangle(foodBrush, new Rectangle(i * SquareSize, j * SquareSize, SquareSize, SquareSize));
                    }
                    else if (_map.OriginalMap[i, j].Type == Cell.CellType.Unseen)
                    {
                        graphics.FillRectangle(unseen, new Rectangle(i * SquareSize, j * SquareSize, SquareSize, SquareSize));
                    }
                    else if (_map.ObjectsMap[i, j].Type == Cell.CellType.Ant)
                    {
                        var ant = (Ant)_map.ObjectsMap[i, j];
                        if(!ant.IsDead)
                            graphics.FillEllipse(new SolidBrush(CommandsColors[ant.CommandNumber]),
                                                 new Rectangle(i * SquareSize, j * SquareSize, SquareSize, SquareSize));
                    }
                }
            }

            DrawedMap.Update();
        }
    }
}
