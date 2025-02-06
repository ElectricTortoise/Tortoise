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
            ulong myColourBitboard = board.colourBitboard[board.boardState.GetColourToMove()];
            ulong opponentColourBitboard = board.colourBitboard[board.boardState.GetOpponentColour()];
            ulong value = opponentColourBitboard;
            ulong allPieceBitboard = board.allPieceBitboard;

            while (value != 0)
            {
                int square = BitOperations.TrailingZeroCount(value);

                int pieceType = board.pieceTypesBitboard[square];
                switch (pieceType)
                {
                    case PieceType.Knight:
                        if ((PrecomputedData.KnightMask[kingSquare] & (1UL << square)) != 0)
                        {
                            return true;
                        }
                        break;
                    case PieceType.King:
                        if ((PrecomputedData.KingMask[kingSquare] & (1UL << square)) != 0)
                        {
                            return true;
                        }
                        break;
                    case PieceType.Pawn:
                        ulong pawnMask = board.boardState.whiteToMove ? PrecomputedData.WhitePawnCaptureMask[kingSquare] : PrecomputedData.BlackPawnCaptureMask[kingSquare];
                        if ((pawnMask & (1UL << square)) != 0)
                        {
                            return true;
                        }
                        break;
                    case PieceType.Rook:
                        ulong rookBlocker = PrecomputedData.OrthogonalMask[kingSquare] & allPieceBitboard;
                        ulong rookMask = GenerateSliderAttackMask(kingSquare, rookBlocker, true);
                        if ((rookMask & (1UL << square)) != 0)
                        {
                            return true;
                        }
                        break;
                    case PieceType.Bishop:
                        ulong bishopBlocker = PrecomputedData.DiagonalMask[kingSquare] & allPieceBitboard;
                        ulong bishopMask = GenerateSliderAttackMask(kingSquare, bishopBlocker, false);
                        if ((bishopMask & (1UL << square)) != 0)
                        {
                            return true;
                        }
                        break;
                    case PieceType.Queen:
                        rookBlocker = PrecomputedData.OrthogonalMask[kingSquare] & allPieceBitboard;
                        rookMask = GenerateSliderAttackMask(kingSquare, rookBlocker, true);
                        bishopBlocker = PrecomputedData.DiagonalMask[kingSquare] & allPieceBitboard;
                        bishopMask = GenerateSliderAttackMask(kingSquare, bishopBlocker, false);
                        ulong queenMask = rookMask | bishopMask;
                        if ((queenMask & (1UL << square)) != 0)
                        {
                            return true;
                        }
                        break;
                }
                value &= value - 1;
            }

            return false;
        }
    }
}
