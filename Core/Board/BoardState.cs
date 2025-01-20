using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TortoiseBot.Core.Utility;

namespace TortoiseBot.Core.Board
{
    public class BoardState
    {
        public bool whiteToMove;
        public bool whiteKingCastle;
        public bool whiteQueenCastle;
        public bool blackKingCastle;
        public bool blackQueenCastle;
        public int epTargetSquare;
        public int halfmoveClock;
        public int fullmoveClock;

        public byte GetColourToMove()
        {
            return whiteToMove ? Colour.White : Colour.Black;
        }

        public byte GetOpponentColour()
        {
            return whiteToMove ? Colour.Black : Colour.White;
        }
    }
}
