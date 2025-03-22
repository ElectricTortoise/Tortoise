﻿using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;


namespace Tortoise.Core
{
    public static unsafe class Search
    {
        public static int nodesCounter;
        public static int bestScore;
        public static Move BestMove;

        public static void StartSearch(Board board, ref SearchInformation info)
        {
            string move = "";
            info.SearchActive = true;
            nodesCounter = 0;
            bestScore = 0;

            TimeManager.TotalSearchTime.Start();
            for (int searchDepth = 1; searchDepth < info.DepthLimit + 1; searchDepth++)
            {
                NegaMax(board, ref info, searchDepth, 0, SearchConstants.AlphaStart, SearchConstants.BetaStart);
                long timeInMS = TimeManager.TotalSearchTime.ElapsedMilliseconds;
                int nps = (int)((nodesCounter / Math.Max(1, timeInMS)) * 1000);
                move = Utility.MoveToString(BestMove);

                if (info.TimeManager.CheckTime())
                {
                    break;
                }

                Console.WriteLine($"info depth {searchDepth} time {timeInMS} score cp {bestScore} nodes {nodesCounter} nps {nps} pv {move}");
            }

            TimeManager.TotalSearchTime.Reset();

            Console.WriteLine($"bestmove {move}");
            info.SearchActive = false;
        }
        
        public static int NegaMax(Board board, ref SearchInformation info, int depth, int ply, int alpha, int beta)
        {

            if (depth == 0)
            {
                return Evaluation.Evaluate(board);
                //return Quiesce(board, alpha, beta);
            }

            int bestSoFar = SearchConstants.AlphaStart;
            int legalMoves = 0;
            MoveList moveList = new MoveList();
            MoveGen.GenAllMoves(board, ref moveList);

            Board tempBoard = board;

            for (int i = 0; i < moveList.Length; i++)
            {
                Move move = new Move(moveList.Moves[i]);
                tempBoard = board;
                tempBoard.MakeMove(move); // flips whose turn it is to move

                int kingSquare = tempBoard.kingSquares[tempBoard.boardState.GetOpponentColour()]; // original player's king square
                if (MoveGenUtility.IsInCheck(tempBoard, kingSquare, tempBoard.boardState.GetColourToMove())) // ColourToMove() is actually opponent's colour
                {
                    continue;
                }
                nodesCounter++;
                legalMoves++;

                bestSoFar = Math.Max(bestSoFar, -NegaMax(tempBoard, ref info, depth - 1, ply + 1, -beta, -alpha));
                if (alpha < bestSoFar)
                {
                    alpha = bestSoFar;
                    if (ply == 0 && bestSoFar >= alpha)
                    {
                        bestScore = bestSoFar;
                        BestMove = move;
                    }
                }

                if (beta <= bestSoFar)
                {
                    return bestSoFar;
                }


                if (info.TimeManager.CheckTime())
                {
                    break;
                }
            }

            if (legalMoves == 0)
            {
                int kingSquare = board.kingSquares[board.boardState.GetColourToMove()]; 
                if (MoveGenUtility.IsInCheck(board, kingSquare, board.boardState.GetOpponentColour()))
                {
                    return -EvaluationConstants.ScoreMate + ply;
                }
                else
                {
                    return 0;
                }
            }

            return bestSoFar;
        }

        public static int Quiesce(Board board, int alpha, int beta)
        {
            //return if position is worse beta, else increase alpha to position value
            int standPat = Evaluation.Evaluate(board);
            int bestSoFar = standPat;
            alpha = Math.Max(alpha, standPat);
            if (beta <= standPat)
            {
                return standPat;
            }

            MoveList moveList = new MoveList();
            MoveGen.GenAllMoves(board, ref moveList); // a bit of time is wasted generating non-capture moves
            Board tempBoard = board;

            for (int i = 0; i < moveList.Length; i++)
            {
                Move move = new Move(moveList.Moves[i]);
                if ((move.flag & (MoveFlag.Capture | MoveFlag.EnPassant)) != 0)
                {
                    tempBoard = board;
                    tempBoard.MakeMove(move); // flips whose turn it is to move

                    int kingSquare = tempBoard.kingSquares[tempBoard.boardState.GetOpponentColour()]; // original player's king square
                    if (MoveGenUtility.IsInCheck(tempBoard, kingSquare, tempBoard.boardState.GetColourToMove())) // ColourToMove() is actually opponent's colour
                    {
                        continue;
                    }

                    Search.nodesCounter++;

                    bestSoFar = Math.Max(bestSoFar, -Quiesce(tempBoard, -beta, -alpha));
                    alpha = Math.Max(alpha, bestSoFar);
                    if (beta <= bestSoFar)
                    {
                        return bestSoFar;
                    }
                }
            }
            
            return bestSoFar;
        }
    }
}

