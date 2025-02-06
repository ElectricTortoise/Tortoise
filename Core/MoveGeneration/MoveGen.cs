using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TortoiseBot.Core.Board;
using TortoiseBot.Core.Utility;

namespace TortoiseBot.Core.MoveGeneration
{
    public static class MoveGen
    {
        public static readonly ulong[][] RookLookup;
        public static readonly ulong[][] BishopLookup;

        static MoveGen()
        {
            RookLookup = MagicGen.GenerateLookupTable(true);
            BishopLookup = MagicGen.GenerateLookupTable(false);
        }


        public static void GenAllMoves(Board.Board board, ref MoveList movelist)
        {
            GenNonSliderMoves(board, PieceType.Knight, ref movelist);
            GenNonSliderMoves(board, PieceType.King, ref movelist);
            GenSliderMoves(board, PieceType.Rook, ref movelist);
            GenSliderMoves(board, PieceType.Bishop, ref movelist);
            GenSliderMoves(board, PieceType.Queen, ref movelist);
            GenPawnMoves(board, ref movelist);
            GenCastlingMoves(board, ref movelist);
        }

        public static void GenNonSliderMoves(Board.Board board, int pieceType, ref MoveList movelist)
        {
            ulong myColourBitboard = board.colourBitboard[board.boardState.GetColourToMove()];
            ulong myPieceBitboard = board.pieceBitboard[pieceType] & myColourBitboard;
            ulong opponentColourBitboard = board.colourBitboard[board.boardState.GetOpponentColour()];

            ulong[] precomputedBitboard = pieceType switch
            {
                PieceType.King => PrecomputedData.KingMask,
                PieceType.Knight => PrecomputedData.KnightMask,
                _ => []
            };

            int[] startSquareArray = BoardUtility.GetBitIndices(myPieceBitboard);
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
                    AddMove(ref movelist, startSquare, targetSquare, flag);
                }
            }
        }

        public static void GenSliderMoves(Board.Board board, int pieceType, ref MoveList movelist)
        {
            ulong allPieceBitboard = board.allPieceBitboard;
            ulong myColourBitboard = board.colourBitboard[board.boardState.GetColourToMove()];
            ulong myPieceBitboard = board.pieceBitboard[pieceType] & myColourBitboard;
            ulong opponentColourBitboard = board.colourBitboard[board.boardState.GetOpponentColour()];

            int[] startSquares = BoardUtility.GetBitIndices(myPieceBitboard);
            foreach (int startSquare in startSquares)
            {
                ulong moveBoard = pieceType switch
                {
                    PieceType.Queen => GetSquareMoves(allPieceBitboard, startSquare, true) | GetSquareMoves(allPieceBitboard, startSquare, false),
                    PieceType.Bishop => GetSquareMoves(allPieceBitboard, startSquare, false),
                    PieceType.Rook => GetSquareMoves(allPieceBitboard, startSquare, true),
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
                    AddMove(ref movelist, startSquare, targetSquare, flag);
                }
            }
        }

        public static void GenPawnMoves(Board.Board board, ref MoveList movelist)
        {
            ulong allPieceBitboard = board.allPieceBitboard;
            ulong myColourBitboard = board.colourBitboard[board.boardState.GetColourToMove()];
            ulong myPieceBitboard = board.pieceBitboard[PieceType.Pawn] & myColourBitboard;
            ulong opponentColourBitboard = board.colourBitboard[board.boardState.GetOpponentColour()];

            int[] pawnSquares = BoardUtility.GetBitIndices(myPieceBitboard);

            //Single Move
            ulong singlePushMask = board.boardState.whiteToMove ? (myPieceBitboard >> 8) & ~allPieceBitboard : (myPieceBitboard << 8) & ~allPieceBitboard;
            int[] defaultTargetSquareArray = BoardUtility.GetBitIndices(singlePushMask & 0xffffffffffff00UL);
            foreach (int targetSquare in defaultTargetSquareArray)
            {
                MoveFlag flag = MoveFlag.Default;
                AddMove(ref movelist, board.boardState.whiteToMove ? (targetSquare + 8) : (targetSquare - 8), targetSquare, flag);
            }

            //Double Move
            ulong DoubleMoveTargetRank = board.boardState.whiteToMove ? 0xff00000000UL : 0xff000000UL;
            ulong doublePushMask = board.boardState.whiteToMove ? (singlePushMask >> 8) & ~allPieceBitboard & DoubleMoveTargetRank : (singlePushMask << 8) & ~allPieceBitboard & DoubleMoveTargetRank;
            int[] doubleMoveTargetSquareArray = BoardUtility.GetBitIndices(doublePushMask);
            foreach (int targetSquare in doubleMoveTargetSquareArray)
            {
                MoveFlag flag = MoveFlag.DoublePush;
                AddMove(ref movelist, board.boardState.whiteToMove ? (targetSquare + 16) : (targetSquare - 16), targetSquare, flag);
            }

            //Captures
            ulong captureMask = 0UL;
            foreach (int pawnSquare in pawnSquares)
            {
                ulong individualCaptureMask = board.boardState.whiteToMove ? PrecomputedData.WhitePawnCaptureMask[pawnSquare] : PrecomputedData.BlackPawnCaptureMask[pawnSquare];
                individualCaptureMask &= opponentColourBitboard;
                int[] captureTargetSquareArray = BoardUtility.GetBitIndices(individualCaptureMask & 0xffffffffffff00UL);
                foreach (int targetSquare in captureTargetSquareArray)
                {
                    MoveFlag flag = MoveFlag.Capture;
                    AddMove(ref movelist, pawnSquare, targetSquare, flag);
                }
                captureMask |= individualCaptureMask;
            }

            //En Passant
            if (board.boardState.epTargetSquare != 0)
            {
                int epTargetSquare = board.boardState.epTargetSquare;
                ulong epRank = board.boardState.whiteToMove ? PrecomputedData.BlackPawnCaptureMask[epTargetSquare] & 0xff000000UL : PrecomputedData.WhitePawnCaptureMask[epTargetSquare] & 0xff00000000UL;
                ulong epAttacker = myPieceBitboard & epRank;
                if (epAttacker != 0)
                {
                    int[] startSquares = BoardUtility.GetBitIndices(epAttacker);
                    foreach (int startSquare in startSquares)
                    {
                        MoveFlag flag = MoveFlag.EnPassant | MoveFlag.Capture;
                        AddMove(ref movelist, startSquare, epTargetSquare, flag);
                    }
                    captureMask |= (1UL << board.boardState.epTargetSquare);
                }
            }
            

            //Promotion
            ulong allTargetSquareBitboard = singlePushMask | doublePushMask | captureMask;
            ulong promotingPawnMask = (board.boardState.whiteToMove ? 0xff00UL : 0xff000000000000UL) & myPieceBitboard;
            ulong promotionMask = (board.boardState.whiteToMove ? 0xffUL : 0xff00000000000000UL) & allTargetSquareBitboard;
            int[] promotionTargetSquareArray = BoardUtility.GetBitIndices(promotionMask);
            int[] promotingPawnArray = BoardUtility.GetBitIndices(promotingPawnMask);

            foreach (int startSquare in promotingPawnArray)
            {
                foreach (int targetSquare in promotionTargetSquareArray)
                {
                    MoveFlag initialFlag = MoveFlag.PromoteToKnight;
                    for (int i = 0; i < 4; i++) 
                    {
                        MoveFlag flag = (MoveFlag)((int)initialFlag + i);
                        if (((1UL << targetSquare) & captureMask) != 0)
                        {
                            flag |= MoveFlag.Capture;
                            AddMove(ref movelist, startSquare, targetSquare, flag);
                            flag ^= MoveFlag.Capture;
                        }
                        else
                        {
                            AddMove(ref movelist, startSquare, targetSquare, flag);
                        }
                    }
                }
            } 
        }

        public static void GenCastlingMoves(Board.Board board, ref MoveList movelist)
        {
            MoveFlag flag = MoveFlag.Castle;

            int startSquare = board.boardState.whiteToMove ? 60 : 4;
            bool kingSide = board.boardState.whiteToMove ? board.boardState.whiteKingCastle : board.boardState.blackKingCastle;
            bool queenSide = board.boardState.whiteToMove ? board.boardState.whiteQueenCastle : board.boardState.blackQueenCastle;

            if (kingSide && IsCastlingValid(startSquare, startSquare + 2, board))
            {
                AddMove(ref movelist, startSquare, startSquare + 2, flag);
            }

            if (queenSide && IsCastlingValid(startSquare, startSquare - 2, board))
            {
                AddMove(ref movelist, startSquare, startSquare - 2, flag);
            }
        }

        private static ulong GetSquareMoves(ulong allPieceBitboard, int square, bool isRook)
        {
            int pieceType = isRook ? PieceType.Rook : PieceType.Bishop;
            ulong precomputedBitboard = isRook ? PrecomputedData.OrthogonalMask[square] : PrecomputedData.DiagonalMask[square];
            ulong magic = isRook ? MagicGen.RookMagics[square] : MagicGen.BishopMagics[square];
            int shift = isRook ? MagicGen.RookShifts[square] : MagicGen.BishopShifts[square];
            ulong[][] lookup = isRook ? RookLookup : BishopLookup;

            ulong blockers = precomputedBitboard & allPieceBitboard;
            ulong hash = (blockers * magic) >> shift;

            return lookup[square][hash];
        }

        private static bool IsCastlingValid(int startSquare, int targetSquare, Board.Board board)
        {
            ulong allPieceBitboard = board.allPieceBitboard;
            int direction = targetSquare > startSquare ? 1 : -1;
            int annoyingSquareThatIHadToCheck = board.boardState.whiteToMove ? 57 : 1;
            for (int castlingSquare = startSquare; castlingSquare != targetSquare; castlingSquare += direction)
            {
                if ((allPieceBitboard & (1UL << (castlingSquare + direction))) != 0)
                {
                    return false;
                }

                if ((direction == -1) && ((allPieceBitboard & (1UL << annoyingSquareThatIHadToCheck)) != 0))
                {
                    return false;
                }

                if (MoveGenUtility.IsInCheck(board, castlingSquare))
                {
                    return false;
                }
            }
            return true;
        }

        private static void AddMove(ref MoveList movelist, int startSquare, int targetSquare, MoveFlag flag)
        {
            Move move = new Move(startSquare, targetSquare, flag);
            movelist.Moves[movelist.Length] = move;
            movelist.Length++;
        }
    }
}
