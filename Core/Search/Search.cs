using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;


namespace Tortoise.Core
{
    public static unsafe class Search
    {
        public static Stack<ulong> RepetitionHistory;

        public static int NodesCounter;

        private static bool SearchCompleted;
        private static int BestScore;
        private static ushort BestMove;
        public static int RootBestScore;
        public static ushort RootBestMove;

        static Search()
        {
            RepetitionHistory = new Stack<ulong>();
        }

        public static void StartSearch(Board board, ref SearchInformation info)
        {
            string move = "";
            info.SearchActive = true;
            NodesCounter = 0;
            RootBestScore = 0;

            TimeManager.TotalSearchTime.Start();
            for (int searchDepth = 1; searchDepth < info.DepthLimit + 1; searchDepth++)
            {
                RootBestScore = NegaMax(board, ref info, searchDepth, 0, -EvaluationConstants.ScoreInfinite, EvaluationConstants.ScoreInfinite);
                if (SearchCompleted)
                {
                    RootBestMove = BestMove;
                }

                long timeInMS = TimeManager.TotalSearchTime.ElapsedMilliseconds;
                int nps = (int)((NodesCounter / Math.Max(1, timeInMS)) * 1000);
                move = Utility.MoveToString(RootBestMove);

                if (info.TimeManager.CheckTime())
                {
                    break;
                }

                Console.WriteLine($"info depth {searchDepth} time {timeInMS} score cp {RootBestScore} nodes {NodesCounter} nps {nps} pv {move}");
            }

            TimeManager.TotalSearchTime.Reset();

            Console.WriteLine($"bestmove {move}");
            info.SearchActive = false;
        }

        public static int NegaMax(Board board, ref SearchInformation info, int depth, int ply, int alpha, int beta)
        {
            SearchCompleted = true;

            int repeatedMoves = 0;
            if (ply != 0) //To-do: implement twofold detection for post-root, threefold detection for pre-root and skip every other move (since I cannot play my opponent's moves)
            {
                foreach (ulong hash in RepetitionHistory)
                {
                    if (board.zobristHash == hash) { repeatedMoves++; }
                }
                if (repeatedMoves >= 3) //board.boardState.halfmoveClock >= 100
                {
                    return EvaluationConstants.ScoreDraw;
                }
            }

            if (depth <= 0)
            {
                return Quiesce(board, alpha, beta);
            }

            int bestSoFar = -EvaluationConstants.ScoreInfinite;
            int legalMoves = 0;
            MoveList moveList = new MoveList();
            MoveGen.GenAllMoves(board, ref moveList);
            MoveOrderer.OrderMoves(ref board, ref moveList);

            Board tempBoard;

            for (int i = 0; i < moveList.Length; i++)
            {
                ushort move = moveList.Moves[i];
                tempBoard = board;
                tempBoard.MakeMove(move); // flips whose turn it is to move

                int kingSquare = tempBoard.kingSquares[tempBoard.boardState.GetOpponentColour()]; // original player's king square
                if (MoveGenUtility.IsInCheck(tempBoard, kingSquare, tempBoard.boardState.GetColourToMove())) // ColourToMove() is actually opponent's colour
                {
                    continue;
                }

                NodesCounter++;
                legalMoves++;

                RepetitionHistory.Push(tempBoard.zobristHash);
                bestSoFar = Math.Max(bestSoFar, -NegaMax(tempBoard, ref info, depth - 1, ply + 1, -beta, -alpha));
                RepetitionHistory.Pop();

                if (bestSoFar > alpha)
                {
                    alpha = bestSoFar;
                    if (ply == 0 && bestSoFar >= alpha)
                    {
                        BestScore = bestSoFar;
                        BestMove = move;
                    }
                }

                if (bestSoFar >= beta)
                {
                    break;
                }

                if (info.TimeManager.CheckTime())
                {
                    SearchCompleted = false;
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
                    return EvaluationConstants.ScoreDraw;
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
            if (standPat >= beta)
            {
                return standPat;
            }

            MoveList moveList = new MoveList();
            MoveGen.GenAllMoves(board, ref moveList); // a bit of time is wasted generating non-capture moves
            MoveOrderer.OrderMoves(ref board, ref moveList);

            Board tempBoard;

            for (int i = 0; i < moveList.Length; i++)
            {
                ushort move = moveList.Moves[i];
                if ((new Move(move).flag & MoveFlag.Capture) != 0)
                {
                    tempBoard = board;
                    tempBoard.MakeMove(move); // flips whose turn it is to move

                    int kingSquare = tempBoard.kingSquares[tempBoard.boardState.GetOpponentColour()]; // original player's king square
                    if (MoveGenUtility.IsInCheck(tempBoard, kingSquare, tempBoard.boardState.GetColourToMove())) // ColourToMove() is actually opponent's colour
                    {
                        continue;
                    }

                    Search.NodesCounter++;


                    bestSoFar = Math.Max(bestSoFar, -Quiesce(tempBoard, -beta, -alpha));
                    alpha = Math.Max(alpha, bestSoFar);
                    if (bestSoFar > alpha)
                    {
                        alpha = bestSoFar;
                    }
                    if (bestSoFar >= beta)
                    {
                        return bestSoFar;
                    }
                }
                else 
                {
                    return bestSoFar;
                }
            }

            return bestSoFar;
        }
    }
}

