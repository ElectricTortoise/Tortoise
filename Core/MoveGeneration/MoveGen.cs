using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Tortoise.Core
{
    public static unsafe class MoveGen
    {
        public static readonly ulong[][] RookLookup;
        public static readonly ulong[][] BishopLookup;

        static MoveGen()
        {
            RookLookup = MagicGen.GenerateLookupTable(true);
            BishopLookup = MagicGen.GenerateLookupTable(false);
        }


        public static void GenAllMoves(Board board, ref MoveList movelist)
        {
            GenNonSliderMoves(board, PieceType.Knight, ref movelist);
            GenNonSliderMoves(board, PieceType.King, ref movelist);
            GenSliderMoves(board, PieceType.Rook, ref movelist);
            GenSliderMoves(board, PieceType.Bishop, ref movelist);
            GenSliderMoves(board, PieceType.Queen, ref movelist);
            GenPawnMoves(board, ref movelist);
            GenCastlingMoves(board, ref movelist);
        }

        public static void GenNonSliderMoves(Board board, int pieceType, ref MoveList movelist)
        {
            ulong myColourBitboard = board.colourBitboard[board.boardState.GetColourToMove()];
            ulong myPieceBitboard = board.pieceBitboard[pieceType] & myColourBitboard;
            ulong opponentColourBitboard = board.colourBitboard[board.boardState.GetOpponentColour()];

            while (myPieceBitboard != 0)
            {
                int startSquare = BitOperations.TrailingZeroCount(myPieceBitboard);
                ulong precomputedBitboard = pieceType switch
                {
                    PieceType.King => PrecomputedData.KingMask[startSquare],
                    PieceType.Knight => PrecomputedData.KnightMask[startSquare],
                    _ => 0
                };
                ulong targetSquareBitboard = precomputedBitboard & ~myColourBitboard;
                while (targetSquareBitboard != 0)
                {
                    int targetSquare = BitOperations.TrailingZeroCount(targetSquareBitboard);
                    MoveFlag flag = MoveFlag.Default;
                    if (Utility.IsBitOnBitboard(opponentColourBitboard, targetSquare))
                    {
                        flag = MoveFlag.Capture;
                    }
                    movelist.AddMove(startSquare, targetSquare, flag);
                    targetSquareBitboard &= targetSquareBitboard - 1;
                }
                myPieceBitboard &= myPieceBitboard - 1;
            }
        }

        public static void GenSliderMoves(Board board, int pieceType, ref MoveList movelist)
        {
            ulong allPieceBitboard = board.allPieceBitboard;
            ulong myColourBitboard = board.colourBitboard[board.boardState.GetColourToMove()];
            ulong myPieceBitboard = board.pieceBitboard[pieceType] & myColourBitboard;
            ulong opponentColourBitboard = board.colourBitboard[board.boardState.GetOpponentColour()];

            while (myPieceBitboard != 0)
            {
                int startSquare = BitOperations.TrailingZeroCount(myPieceBitboard);
                ulong moveBoard = pieceType switch
                {
                    PieceType.Queen => MoveGenUtility.GetSliderSquareMoves(allPieceBitboard, startSquare, true) | MoveGenUtility.GetSliderSquareMoves(allPieceBitboard, startSquare, false),
                    PieceType.Bishop => MoveGenUtility.GetSliderSquareMoves(allPieceBitboard, startSquare, false),
                    PieceType.Rook => MoveGenUtility.GetSliderSquareMoves(allPieceBitboard, startSquare, true),
                    _ => 0
                };

                ulong targetSquareBitboard = moveBoard & ~myColourBitboard;
                while (targetSquareBitboard != 0)
                {
                    int targetSquare = BitOperations.TrailingZeroCount(targetSquareBitboard);
                    MoveFlag flag = MoveFlag.Default;
                    if (Utility.IsBitOnBitboard(opponentColourBitboard, targetSquare))
                    {
                        flag = MoveFlag.Capture;
                    }
                    movelist.AddMove(startSquare, targetSquare, flag);
                    targetSquareBitboard &= targetSquareBitboard - 1;
                }

                myPieceBitboard &= myPieceBitboard - 1;
            }
        }

        public static void GenPawnMoves(Board board, ref MoveList movelist)
        {
            ulong allPieceBitboard = board.allPieceBitboard;
            ulong myColourBitboard = board.colourBitboard[board.boardState.GetColourToMove()];
            ulong myPieceBitboard = board.pieceBitboard[PieceType.Pawn] & myColourBitboard;
            ulong opponentColourBitboard = board.colourBitboard[board.boardState.GetOpponentColour()];

            //Single Move
            ulong singlePushMask = (board.boardState.whiteToMove ? (myPieceBitboard >> 8) : (myPieceBitboard << 8)) & ~allPieceBitboard;
            ulong singlePushMaskClone = singlePushMask & 0xffffffffffff00UL; //To prevent counting promotion moves as regular captures
            while (singlePushMaskClone != 0)
            {
                int targetSquare = BitOperations.TrailingZeroCount(singlePushMaskClone);
                MoveFlag flag = MoveFlag.Default;
                movelist.AddMove(board.boardState.whiteToMove ? (targetSquare + 8) : (targetSquare - 8), targetSquare, flag);
                singlePushMaskClone &= singlePushMaskClone - 1;
            }

            //Double Move
            ulong DoubleMoveTargetRank = board.boardState.whiteToMove ? BoardConstants.rank4 : BoardConstants.rank5;
            ulong doublePushMask = (board.boardState.whiteToMove ? (singlePushMask >> 8) : (singlePushMask << 8)) & ~allPieceBitboard & DoubleMoveTargetRank;
            while (doublePushMask != 0)
            {
                int targetSquare = BitOperations.TrailingZeroCount(doublePushMask);
                MoveFlag flag = MoveFlag.DoublePush;
                movelist.AddMove(board.boardState.whiteToMove ? (targetSquare + 16) : (targetSquare - 16), targetSquare, flag);
                doublePushMask &= doublePushMask - 1;
            }

            //Captures
            ulong captureMask = 0UL;
            ulong clonePieceBitboard = myPieceBitboard;
            while (clonePieceBitboard != 0)
            {
                int startSquare = BitOperations.TrailingZeroCount(clonePieceBitboard);
                ulong individualCaptureMask = (board.boardState.whiteToMove ? PrecomputedData.WhitePawnCaptureMask[startSquare] : PrecomputedData.BlackPawnCaptureMask[startSquare]) & opponentColourBitboard;
                captureMask |= individualCaptureMask;
                individualCaptureMask &= 0xffffffffffff00UL; //To prevent counting promotion captures as regular captures
                while (individualCaptureMask != 0)
                {
                    int targetSquare = BitOperations.TrailingZeroCount(individualCaptureMask);
                    MoveFlag flag = MoveFlag.Capture;
                    movelist.AddMove(startSquare, targetSquare, flag);
                    individualCaptureMask &= individualCaptureMask - 1;
                }
                clonePieceBitboard &= clonePieceBitboard - 1;
            }

            //En Passant
            if (board.boardState.epTargetSquare != BoardConstants.NONE_SQUARE)
            {
                int epTargetSquare = board.boardState.epTargetSquare;
                ulong epAttackSquares = board.boardState.whiteToMove ? PrecomputedData.BlackPawnCaptureMask[epTargetSquare] & BoardConstants.rank5 : PrecomputedData.WhitePawnCaptureMask[epTargetSquare] & BoardConstants.rank4;
                ulong epAttacker = myPieceBitboard & epAttackSquares;
                if (epAttacker != 0)
                {
                    while (epAttacker != 0)
                    {
                        int startSquare = BitOperations.TrailingZeroCount(epAttacker);
                        MoveFlag flag = MoveFlag.EnPassant | MoveFlag.Capture;
                        movelist.AddMove(startSquare, epTargetSquare, flag);
                        epAttacker &= epAttacker - 1;
                    }
                    captureMask |= (1UL << board.boardState.epTargetSquare);
                }
            }

            //Promotion
            ulong promotingPawnSquares = (board.boardState.whiteToMove ? BoardConstants.rank7 : BoardConstants.rank2) & myPieceBitboard;

            
            while (promotingPawnSquares != 0)
            {
                int startSquare = BitOperations.TrailingZeroCount(promotingPawnSquares);
                int targetSquare = startSquare + (board.boardState.whiteToMove ? -8 : 8);

                MoveFlag initialFlag = MoveFlag.PromoteToKnight;
                for (int i = 0; i < 4; i++)
                {
                    MoveFlag flag = (MoveFlag)((int)initialFlag + i);
                    if (!Utility.IsBitOnBitboard(allPieceBitboard, targetSquare))
                    {
                        movelist.AddMove(startSquare, targetSquare, flag);
                    }
                    if (((1UL << targetSquare + 1) & captureMask) != 0)
                    {
                        flag |= MoveFlag.Capture;
                        movelist.AddMove(startSquare, targetSquare + 1, flag);
                        flag ^= MoveFlag.Capture;
                    }
                    if (((1UL << targetSquare - 1) & captureMask) != 0)
                    {
                        flag |= MoveFlag.Capture;
                        movelist.AddMove(startSquare, targetSquare - 1, flag);
                        flag ^= MoveFlag.Capture;
                    }
                }
                promotingPawnSquares &= promotingPawnSquares - 1;
            }
        }

        public static void GenCastlingMoves(Board board, ref MoveList movelist)
        {
            MoveFlag flag = MoveFlag.Castle;

            int startSquare = board.boardState.whiteToMove ? BoardConstants.e1 : BoardConstants.e8;
            bool kingSide = board.boardState.whiteToMove ? board.boardState.whiteKingCastle : board.boardState.blackKingCastle;
            bool queenSide = board.boardState.whiteToMove ? board.boardState.whiteQueenCastle : board.boardState.blackQueenCastle;

            if (kingSide && IsCastlingValid(startSquare, startSquare + 2, board))
            {
                movelist.AddMove(startSquare, startSquare + 2, flag);
            }

            if (queenSide && IsCastlingValid(startSquare, startSquare - 2, board))
            {
                movelist.AddMove(startSquare, startSquare - 2, flag);
            }
        }

        private static bool IsCastlingValid(int startSquare, int targetSquare, Board board)
        {
            ulong allPieceBitboard = board.allPieceBitboard;
            int direction = targetSquare > startSquare ? 1 : -1;
            int annoyingSquareThatIHadToCheck = board.boardState.whiteToMove ? BoardConstants.b1 : BoardConstants.b8;
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

                if (MoveGenUtility.IsInCheck(board, castlingSquare, board.boardState.GetOpponentColour()))
                {
                    return false;
                }
            }
            return true;
        }


    }
}
