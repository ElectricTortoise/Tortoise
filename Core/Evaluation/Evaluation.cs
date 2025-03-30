using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Tortoise.Core
{
    public static unsafe class Evaluation
    {
        public static int Evaluate(Board board)
        {
            int positionValue = 0;
            int colourToMove = board.boardState.GetColourToMove();
            int opponentColour = board.boardState.GetOpponentColour();
            ulong allPieceBitboard = board.allPieceBitboard;
            int piecesLeft = 0;

            while (allPieceBitboard != 0)
            {
                allPieceBitboard &= allPieceBitboard - 1;
                piecesLeft++;
            }

            bool endgame = ((piecesLeft <= 10) && ((board.pieceBitboard[PieceType.Queen] & board.colourBitboard[opponentColour]) == 0UL));

            for (int pieceType = 0; pieceType < 5; pieceType++) 
            {
                ulong myPieceBitboard = board.pieceBitboard[pieceType] & board.colourBitboard[colourToMove];
                ulong opponentPieceBitboard = board.pieceBitboard[pieceType] & board.colourBitboard[opponentColour];

                while (myPieceBitboard != 0)
                {
                    int square = BitOperations.TrailingZeroCount(myPieceBitboard);
                    positionValue += GetPieceValue(pieceType) + GetPositionValue(pieceType, square, endgame);
                    myPieceBitboard &= myPieceBitboard - 1;
                }

                while (opponentPieceBitboard != 0)
                {
                    int square = BitOperations.TrailingZeroCount(opponentPieceBitboard) ^ 0b111000;
                    positionValue -= GetPieceValue(pieceType) + GetPositionValue(pieceType, square, endgame); ;
                    opponentPieceBitboard &= opponentPieceBitboard - 1;
                }
            }

            return positionValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetPieceValue(int pieceType)
        {
            return pieceType switch
            {
                PieceType.Pawn => SearchOptions.PawnValue,
                PieceType.Knight => SearchOptions.KnightValue,
                PieceType.Bishop => SearchOptions.BishopValue,
                PieceType.Rook => SearchOptions.RookValue,
                PieceType.Queen => SearchOptions.QueenValue,
                _ => 0
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetPositionValue(int pieceType, int square, bool endgame)
        {
            if (endgame)
            {
                return pieceType switch
                {
                    PieceType.Pawn => PieceSquareTable.endgamePawnTable[square],
                    PieceType.Knight => PieceSquareTable.endgameKnightTable[square],
                    PieceType.Bishop => PieceSquareTable.endgameBishopTable[square],
                    PieceType.Rook => PieceSquareTable.endgameRookTable[square],
                    PieceType.Queen => PieceSquareTable.endgameQueenTable[square],
                    _ => 0
                };
            }
            else
            {
                return pieceType switch
                {
                    PieceType.Pawn => PieceSquareTable.midgamePawnTable[square],
                    PieceType.Knight => PieceSquareTable.midgameKnightTable[square],
                    PieceType.Bishop => PieceSquareTable.midgameBishopTable[square],
                    PieceType.Rook => PieceSquareTable.midgameRookTable[square],
                    PieceType.Queen => PieceSquareTable.midgameQueenTable[square],
                    _ => 0
                };
            }
        }

    }
}
