using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TortoiseBot.Core.Board
{
    [Flags]
    public enum Piece
    {
        None = 0,
        Pawn = 1,
        Rook = 2,
        Knight = 4,
        Bishop = 8,
        Queen = 16,
        King = 32,
        White = 64,
        Black = 128
    }

    public class Pieces
    {
        //White pieces
        public const Piece WhitePawn = Piece.Pawn | Piece.White;
        public const Piece WhiteRook = Piece.Rook | Piece.White;
        public const Piece WhiteKnight = Piece.Knight | Piece.White;
        public const Piece WhiteBishop = Piece.Bishop | Piece.White;
        public const Piece WhiteQueen = Piece.Queen | Piece.White;
        public const Piece WhiteKing = Piece.King | Piece.White;

        //Black pieces
        public const Piece BlackPawn = Piece.Pawn | Piece.Black;
        public const Piece BlackRook = Piece.Rook | Piece.Black;
        public const Piece BlackKnight = Piece.Knight | Piece.Black;
        public const Piece BlackBishop = Piece.Bishop | Piece.Black;
        public const Piece BlackQueen = Piece.Queen | Piece.Black;
        public const Piece BlackKing = Piece.King | Piece.Black;

    }
}
