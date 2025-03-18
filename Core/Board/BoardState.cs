using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Tortoise.Core
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetColourToMove()
        {
            return whiteToMove ? Colour.White : Colour.Black;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetOpponentColour()
        {
            return whiteToMove ? Colour.Black : Colour.White;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ToggleWhiteCastling()
        {
            whiteKingCastle = false; whiteQueenCastle = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ToggleBlackCastling()
        {
            blackKingCastle = false; blackQueenCastle = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ToggleWhiteKingCastling()
        {
            whiteKingCastle = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ToggleWhiteQueenCastling()
        {
            whiteQueenCastle = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ToggleBlackKingCastling()
        {
            blackKingCastle = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ToggleBlackQueenCastling()
        {
            blackQueenCastle = false;
        }
    }
}
