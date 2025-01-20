using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TortoiseBot.Core.Board;
using TortoiseBot.Core.Utility;

namespace TortoiseBot.Core.MoveGeneration
{
    public class MoveGen
    {
        ulong[][] RookLookup;
        ulong[][] BishopLookup;

        public MoveGen()
        {
            RookLookup = MagicGen.GenerateLookupTable(true);
            BishopLookup = MagicGen.GenerateLookupTable(false);
        }


        public void GenNonSliderMoves(Bitboard bitboard, int pieceType, ref MoveList movelist)
        {
            ulong pieceBitboard = bitboard.pieceBitboard[pieceType];
            ulong myColourBitboard = bitboard.colourBitboard[bitboard.boardState.GetColourToMove()];
            ulong opponentColourBitboard = bitboard.colourBitboard[bitboard.boardState.GetOpponentColour()];

            ulong[] precomputedBitboard = pieceType switch
            {
                PieceType.King => PrecomputedData.KingMask,
                PieceType.Knight => PrecomputedData.KnightMask,
                _ => []
            };

            int[] startSquareArray = BoardUtility.GetBitIndices(pieceBitboard & myColourBitboard);
            foreach (int startSquare in startSquareArray)
            {
                ulong targetSquareBitboard = precomputedBitboard[startSquare] & ~myColourBitboard;
                int[] targetSquareArray = BoardUtility.GetBitIndices(targetSquareBitboard);
                foreach (int targetSquare in targetSquareArray)
                {
                    MoveFlag flag = MoveFlag.Default;
                    if (BoardUtility.IsBitOnBitboard(opponentColourBitboard, targetSquare))
                    {
                        flag = MoveFlag.Capture;
                    }
                    Move move = new Move(startSquare, targetSquare, flag);
                    movelist.Moves[movelist.Length] = move;
                    movelist.Length++;
                }
            }
        }

        public void GenSliderMoves(Bitboard bitboard, int pieceType, ref MoveList movelist)
        {
            ulong pieceBitboard = bitboard.pieceBitboard[pieceType];
            ulong myColourBitboard = bitboard.colourBitboard[bitboard.boardState.GetColourToMove()];
            ulong opponentColourBitboard = bitboard.colourBitboard[bitboard.boardState.GetOpponentColour()];

            int[] startSquares = BoardUtility.GetBitIndices(myColourBitboard & pieceBitboard);
            foreach (int startSquare in startSquares)
            {
                ulong moveBoard = pieceType switch
                {
                    PieceType.Queen => GetSquareMoves(pieceBitboard, startSquare, true) | GetSquareMoves(pieceBitboard, startSquare, false),
                    PieceType.Bishop => GetSquareMoves(pieceBitboard, startSquare, false),
                    PieceType.Rook => GetSquareMoves(pieceBitboard, startSquare, true),
                    _ => 0
                };

                ulong targetSquareBitboard = moveBoard & ~myColourBitboard;

                int[] targetSquareArray = BoardUtility.GetBitIndices(targetSquareBitboard);
                foreach (int targetSquare in targetSquareArray)
                {
                    MoveFlag flag = MoveFlag.Default;
                    if (BoardUtility.IsBitOnBitboard(opponentColourBitboard, targetSquare))
                    {
                        flag = MoveFlag.Capture;
                    }
                    Move move = new Move(startSquare, targetSquare, flag);
                    movelist.Moves[movelist.Length] = move;
                    movelist.Length++;
                }
            }
        }

        private ulong GetSquareMoves(ulong pieceBitboard, int square, bool isRook)
        {
            int pieceType = isRook ? PieceType.Rook : PieceType.Bishop;
            ulong precomputedBitboard = isRook ? PrecomputedData.OrthogonalMask[square] : PrecomputedData.DiagonalMask[square];
            ulong magic = isRook ? MagicGen.RookMagics[square] : MagicGen.BishopMagics[square];
            int shift = isRook ? MagicGen.RookShifts[square] : MagicGen.BishopShifts[square];
            ulong[][] lookup = isRook ? RookLookup : BishopLookup;

            ulong blockers = precomputedBitboard & pieceBitboard;
            ulong hash = (blockers * magic) >> shift;

            return lookup[square][hash];
        }

    }
}
