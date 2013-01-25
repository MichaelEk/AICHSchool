using System.Collections.Generic;
using System.Diagnostics;

namespace AntsGame
{
    public class ProcessPlayer : AbstractPlayer
    {
        private readonly Process _process = new Process();

        public ProcessPlayer(string fileName, Map map, int id, int playerCount)
            : base(id, playerCount)
        {
            _process.StartInfo.UseShellExecute = false;
            // You can start any process, HelloWorld is a do-nothing example.
            _process.StartInfo.FileName = fileName;
            _process.StartInfo.CreateNoWindow = true;
            _process.StartInfo.RedirectStandardInput = true;
            _process.StartInfo.RedirectStandardOutput = true;
            _process.Start();
            _process.StandardInput.WriteLine(GetZeroTurnInfo(map));
            _process.StandardInput.Flush(); 
            GetMoves(_process.StandardOutput, map);
        }

        public override List<Move> PlayMove(List<Cell> visibleCells, int turn, Map map)
        {
            _process.StandardInput.WriteLine(GetVisibleMapDesc(visibleCells, turn));
            _process.StandardInput.Flush();
            return GetMoves(_process.StandardOutput, map);
        }

        public override void EndGame(int[] score)
        {
            _process.Kill();
        }
    }
}