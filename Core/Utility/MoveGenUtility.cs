using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;


namespace Tortoise.Core
{
    public unsafe class MoveGenUtility
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

        public static ulong IterativeGenerateSliderAttackMask(int square, ulong blocker, bool isRook)
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

        public static ulong GetSliderSquareMoves(ulong allPieceBitboard, int square, bool isRook)
        {
            ulong precomputedBitboard = isRook ? PrecomputedData.OrthogonalMask[square] : PrecomputedData.DiagonalMask[square];
            ulong magic = isRook ? MagicGen.RookMagics[square] : MagicGen.BishopMagics[square];
            int shift = isRook ? MagicGen.RookShifts[square] : MagicGen.BishopShifts[square];

            ulong blockers = precomputedBitboard & allPieceBitboard;
            ulong hash = (blockers * magic) >> shift;

            return isRook ? MoveGen.RookLookup[square][hash] : MoveGen.BishopLookup[square][hash]; ;
        }

        public static bool IsInCheck(in Board board, int kingSquare, int opponentColour)
        {
            ulong opponentColourBitboard = board.colourBitboard[opponentColour];
            ulong allPieceBitboard = board.allPieceBitboard;


            if ((PrecomputedData.KnightMask[kingSquare] & opponentColourBitboard & board.pieceBitboard[PieceType.Knight]) != 0) { return true; };
            if ((PrecomputedData.KingMask[kingSquare] & opponentColourBitboard & board.pieceBitboard[PieceType.King]) != 0) { return true; };

            //Only get the pawn mask if Knight and King doesn't return
            ulong pawnMask = opponentColour switch
            {
                Colour.White => PrecomputedData.BlackPawnCaptureMask[kingSquare],
                Colour.Black => PrecomputedData.WhitePawnCaptureMask[kingSquare],
                _ => 0
            };
            if ((pawnMask & opponentColourBitboard & board.pieceBitboard[PieceType.Pawn]) != 0) { return true; };

            //Only get the rook/bishop mask if Pawn doesn't return
            ulong rookMask = GetSliderSquareMoves(allPieceBitboard, kingSquare, true);
            ulong bishopMask = GetSliderSquareMoves(allPieceBitboard, kingSquare, false);
            if ((rookMask & opponentColourBitboard & (board.pieceBitboard[PieceType.Rook] | board.pieceBitboard[PieceType.Queen])) != 0) { return true; };
            if ((bishopMask & opponentColourBitboard & (board.pieceBitboard[PieceType.Bishop] | board.pieceBitboard[PieceType.Queen])) != 0) { return true; };

            return false;
        }
    }
}
