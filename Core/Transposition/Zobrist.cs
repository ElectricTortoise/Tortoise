using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Tortoise.Core
{
    public static unsafe class Zobrist
    {
        private const int DefaultSeed = 69420;

        public static readonly ulong[,] ZobristPieces = new ulong[12, 64]; //PieceType then Square, first 6 are black, next 6 are white
        public static readonly ulong[] ZobristEnPassant = new ulong[8]; // a-h
        public static readonly ulong[] ZobristCastling = new ulong[4]; // wk, wq, bk, bq
        public static readonly ulong ZobristBlackToMove;

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

            while (black != 0)
            {
                int square = BitOperations.TrailingZeroCount(black);
                black ^= (1UL << square);
                int pieceType = board.pieceTypesBitboard[square];

                hash ^= ZobristPieces[pieceType, square];
            }

            while (white != 0)
            {
                int square = BitOperations.TrailingZeroCount(white);
                white ^= (1UL << square);
                int pieceType = board.pieceTypesBitboard[square] + 6;

                hash ^= ZobristPieces[pieceType, square];
            }

            if (board.boardState.epTargetSquare != BoardConstants.NONE_SQUARE)
            {
                hash ^= ZobristEnPassant[board.boardState.epTargetSquare % 8];
            }

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

        public static void PrintZobristKeys()
        {
            for (int row = 0; row < 8; row++)
            {
                int consoleColour = (int)ConsoleColor.Black;
                for (int colour = 0; colour < 2; colour++)
                {
                    for (int pt = 0; pt < 6; pt++)
                    {
                        consoleColour++;
                        Console.ForegroundColor = (ConsoleColor)consoleColour;
                        for (int col = 0; col < 8; col++)
                        {
                            Console.Write($"0x{ZobristPieces[pt + 6*colour, 8 * row + col].ToString("x").PadRight(16, ' ')} | ");
                        }

                        string pieceType = pt switch
                        {
                            PieceType.Pawn => "pawn",
                            PieceType.Knight => "knight",
                            PieceType.Bishop => "bishop",
                            PieceType.Rook => "rook",
                            PieceType.Queen => "queen",
                            PieceType.King => "king",
                            _ => ""
                        };
                        string pieceColour = colour switch
                        {
                            Colour.Black => "black",
                            Colour.White => "white",
                            _ => ""
                        };
                        Console.Write($"{pieceColour} {pieceType}");
                        Console.WriteLine();
                    }
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("-----------------------------------------------------------------------------------------------------------------------------------------------------------------------");
                }

                Console.ForegroundColor = ConsoleColor.White;
                for (int col = 0; col < 8; col++)
                {
                    Console.Write($"{(8*row+col).ToString().PadLeft(10, ' ').PadRight(18, ' ')} | ");
                }

                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("-----------------------------------------------------------------------------------------------------------------------------------------------------------------------");
            } //Pieces

            Console.WriteLine();
            Console.WriteLine("------------------------------------------------------------------------------En Passant-------------------------------------------------------------------------------");
            Console.ForegroundColor = ConsoleColor.White;
            for (int col = 0; col < 8; col++)
            {
                Console.Write($"0x{ZobristEnPassant[col].ToString("x").PadRight(16, ' ')} | ");
            }
            Console.WriteLine();
            Console.WriteLine("-----------------------------------------------------------------------------------------------------------------------------------------------------------------------");
            Console.WriteLine("         0         |          1         |          2         |          3         |          4         |          5         |          6         |          7         |");
            Console.WriteLine("-----------------------------------------------------------------------------------------------------------------------------------------------------------------------");

            Console.WriteLine();
            Console.WriteLine("-----------------------------------------------------------------Castling----------------------------------------------------------------");
            Console.Write($"White King 0x{ZobristCastling[0].ToString("x").PadLeft(9, ' ').PadRight(18, ' ')} | ");
            Console.Write($"White Queen 0x{ZobristCastling[1].ToString("x").PadLeft(9, ' ').PadRight(18, ' ')} | ");
            Console.Write($"Black King 0x{ZobristCastling[2].ToString("x").PadLeft(9, ' ').PadRight(18, ' ')} | ");
            Console.Write($"Black Queen 0x{ZobristCastling[3].ToString("x").PadLeft(9, ' ').PadRight(18, ' ')} | ");
            Console.WriteLine();
            Console.WriteLine("-----------------------------------------------------------------------------------------------------------------------------------------");

            Console.WriteLine($"Black to move {ZobristBlackToMove.ToString("x")}");
        }
    }
}
