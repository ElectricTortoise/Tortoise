using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TortoiseBot.Core.MoveGeneration;
using TortoiseBot.Core.Utility;

namespace TortoiseBot.Core.Board
{
    public class Board
    {
        public PositionInfo currentPosition;
        public Bitboard bitboard = new();

        public void LoadStartingPosition()
        {
            LoadPosition(FenUtility.StartFen);
        }

        public void LoadPosition(string fen)
        {
            currentPosition = FenUtility.FenStringParser(fen);
            bitboard = currentPosition.bitboard;
        }

    }
}
    

