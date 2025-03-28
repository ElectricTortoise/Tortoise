using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Tortoise.Core
{
    public static unsafe class Zobrist
    {
        private const int DefaultSeed = 69420;

        private static readonly ulong[,] ZobristPieces = new ulong[12, 64]; //PieceType then Square, first 6 are white, next 6 are black
        private static readonly ulong[] ZobristEnPassant = new ulong[8]; // a-h
        private static readonly ulong[] ZobristCastling = new ulong[4]; // wk, wq, bk, bq
        private static readonly ulong ZobristBlackToMove;

        private static readonly Random ZobristRandom = new Random(DefaultSeed);

        static Zobrist()
        {
            for (int pieceType = 0; pieceType < 12; pieceType++)
            {
                for (int square = 0; square < 64; square++)
                {
                    ZobristPieces[pieceType, square] = unchecked((ulong)ZobristRandom.NextInt64(long.MinValue, long.MaxValue));
                }
            }

            for (int i = 0; i < 8; i++)
            {
                ZobristEnPassant[i] = unchecked((ulong)ZobristRandom.NextInt64(long.MinValue, long.MaxValue));
            }

            for (int i = 0; i < 4; i++)
            {
                ZobristCastling[i] = unchecked((ulong)ZobristRandom.NextInt64(long.MinValue, long.MaxValue));
            }

            ZobristBlackToMove = unchecked((ulong)ZobristRandom.NextInt64(long.MinValue, long.MaxValue));
        }

        public static ulong GetZobristHash(Board board)
        {
            ulong white = board.colourBitboard[Colour.White];
            ulong black = board.colourBitboard[Colour.Black];
            ulong hash = 0;

            while (white != 0)
            {
                int square = BitOperations.TrailingZeroCount(white);
                white ^= (1UL << square);
                int pieceType = board.pieceTypesBitboard[square];

                hash ^= ZobristPieces[pieceType, square];
            }

            while (black != 0)
            {
                int square = BitOperations.TrailingZeroCount(black);
                black ^= (1UL << square);
                int pieceType = board.pieceTypesBitboard[square] + 6;

                hash ^= ZobristPieces[pieceType, square];
            }

            hash ^= ZobristEnPassant[board.boardState.epTargetSquare % 8];

            if (board.boardState.whiteKingCastle)
            {
                hash ^= ZobristCastling[0];
            }
            if (board.boardState.whiteQueenCastle)
            {
                hash ^= ZobristCastling[1];
            }
            if (board.boardState.blackKingCastle)
            {
                hash ^= ZobristCastling[2];
            }
            if (board.boardState.blackQueenCastle)
            {
                hash ^= ZobristCastling[3];
            }

            if (!board.boardState.whiteToMove)
            {
                hash ^= ZobristBlackToMove;
            }

            return hash;
        }
    }
}
