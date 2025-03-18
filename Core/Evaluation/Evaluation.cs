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
            int materialValue = 0;
            int colourToMove = board.boardState.GetColourToMove();
            int opponentColour = board.boardState.GetOpponentColour();

            for (int pieceType = 0; pieceType < 5; pieceType++) 
            {
                ulong myPieceBitboard = board.pieceBitboard[pieceType] & board.colourBitboard[colourToMove];
                ulong opponentPieceBitboard = board.pieceBitboard[pieceType] & board.colourBitboard[opponentColour];

                while (myPieceBitboard != 0)
                {
                    materialValue += GetPieceValue(pieceType);
                    myPieceBitboard &= myPieceBitboard - 1;
                }

                while (opponentPieceBitboard != 0)
                {
                    materialValue -= GetPieceValue(pieceType);
                    opponentPieceBitboard &= opponentPieceBitboard - 1;
                }
            }

            return materialValue;
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

    }
}
