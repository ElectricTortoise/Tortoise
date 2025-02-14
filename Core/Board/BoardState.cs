using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TortoiseBot.Core.MoveGeneration;
using TortoiseBot.Core.Utility;

namespace TortoiseBot.Core.Board
{
    public struct BoardState
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

        public void ToggleWhiteCastling()
        {
            whiteKingCastle = false; whiteQueenCastle = false;
        }

        public void ToggleBlackCastling()
        {
            blackKingCastle = false; blackQueenCastle = false;
        }
        public void ToggleWhiteKingCastling()
        {
            whiteKingCastle = false;
        }
        public void ToggleWhiteQueenCastling()
        {
            whiteQueenCastle = false;
        }

        public void ToggleBlackKingCastling()
        {
            blackKingCastle = false;
        }

        public void ToggleBlackQueenCastling()
        {
            blackQueenCastle = false;
        }
    }
}
