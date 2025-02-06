using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TortoiseBot.Core.Board;
using TortoiseBot.Core.MoveGeneration;

namespace TortoiseBot.Core.Utility
{
    public class MoveGenUtility
    {
        public static bool DirectionOK(int square, int direction)
        {
            if (square + direction < 0 | square + direction > 63)
            {
                return false;
            }

            int rankDistance = Math.Abs((square / 8) - ((square + direction) / 8));
            int fileDistance = Math.Abs((square % 8) - ((square + direction) % 8));

            return Math.Max(rankDistance, fileDistance) <= 1;
        }

        public static ulong GenerateSliderAttackMask(int square, ulong blocker, bool isRook)
        {
            ulong attacks = 0;
            int rank = square / 8;
            int file = square % 8;
            int[] directions = isRook ? new int[] { 8, -8, 1, -1 } : new int[] { 9, 7, -9, -7 };

            foreach (int direction in directions)
            {
                int currentSquare = square;

                while (true)
                {

                    if (!DirectionOK(currentSquare, direction))
                    {
                        break;
                    }

                    currentSquare += direction;
                    attacks |= 1UL << currentSquare;

                    if ((blocker & (1UL << currentSquare)) != 0)
                    {
                        break;
                    }

                }
            }
            return attacks;
        }

        public static bool IsInCheck(Board.Board board, int kingSquare)
        {
            ulong opponentColourBitboard = board.colourBitboard[board.boardState.GetOpponentColour()];
            ulong allPieceBitboard = board.allPieceBitboard;


            if ((PrecomputedData.KnightMask[kingSquare] & opponentColourBitboard & board.pieceBitboard[PieceType.Knight]) != 0) { return true; };
            if ((PrecomputedData.KingMask[kingSquare] & opponentColourBitboard & board.pieceBitboard[PieceType.King]) != 0) { return true; };

            //Only get the pawn mask if Knight and King doesn't return
            ulong pawnMask = board.boardState.whiteToMove ? PrecomputedData.WhitePawnCaptureMask[kingSquare] : PrecomputedData.BlackPawnCaptureMask[kingSquare];
            if ((pawnMask & opponentColourBitboard & board.pieceBitboard[PieceType.Pawn]) != 0) { return true; };

            //Only get the rook/bishop mask if Pawn doesn't return
            ulong rookMask = MoveGen.GetSquareMoves(allPieceBitboard, kingSquare, true);
            ulong bishopMask = MoveGen.GetSquareMoves(allPieceBitboard, kingSquare, false);
            if ((rookMask & opponentColourBitboard & (board.pieceBitboard[PieceType.Rook] | board.pieceBitboard[PieceType.Queen])) != 0) { return true; };
            if ((bishopMask & opponentColourBitboard & (board.pieceBitboard[PieceType.Bishop] | board.pieceBitboard[PieceType.Queen])) != 0) { return true; };

            return false;
        }
    }
}
