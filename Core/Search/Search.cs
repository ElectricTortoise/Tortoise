using System;
using System.Collections;
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
        public static Stack<ulong> RepetitionHistory;

        public static int NodesCounter;

        private static bool SearchCompleted;
        private static int TempBestScore;
        private static Move TempBestMove;
        public static int BestScore;
        public static Move BestMove;

        static Search()
        {
            RepetitionHistory = new Stack<ulong>();
        }

        public static void StartSearch(Board board, ref SearchInformation info)
        {
            string move = "";
            info.SearchActive = true;
            NodesCounter = 0;
            BestScore = 0;

            TimeManager.TotalSearchTime.Start();
            for (int searchDepth = 1; searchDepth < info.DepthLimit + 1; searchDepth++)
            {
                BestScore = NegaMax(board, ref info, searchDepth, 0, SearchConstants.AlphaStart, SearchConstants.BetaStart);
                if (SearchCompleted)
                {
                    BestMove = TempBestMove;
                }

                //for (int i = 0; i < searchDepth - 1; i++)
                //{
                //    RepetitionHistory.Pop();
                //}
                long timeInMS = TimeManager.TotalSearchTime.ElapsedMilliseconds;
                int nps = (int)((NodesCounter / Math.Max(1, timeInMS)) * 1000);
                move = Utility.MoveToString(BestMove);

                if (info.TimeManager.CheckTime())
                {
                    break;
                }

                Console.WriteLine($"info depth {searchDepth} time {timeInMS} score cp {BestScore} nodes {NodesCounter} nps {nps} pv {move}");
            }

            TimeManager.TotalSearchTime.Reset();

            Console.WriteLine($"bestmove {move}");
            info.SearchActive = false;
        }


        public static void PrintValues(IEnumerable myCollection)
        {
            foreach (Object obj in myCollection)
                Console.Write("    {0}", obj);
            Console.WriteLine();
        }
        public static int NegaMax(Board board, ref SearchInformation info, int depth, int ply, int alpha, int beta)
        {
            SearchCompleted = true;
            

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
                int repeatedMoves = 0;
                Move move = new Move(moveList.Moves[i]);
                tempBoard = board;
                tempBoard.MakeMove(move); // flips whose turn it is to move

                int kingSquare = tempBoard.kingSquares[tempBoard.boardState.GetOpponentColour()]; // original player's king square
                if (MoveGenUtility.IsInCheck(tempBoard, kingSquare, tempBoard.boardState.GetColourToMove())) // ColourToMove() is actually opponent's colour
                {
                    continue;
                }
                RepetitionHistory.Push(tempBoard.zobristHash);
                NodesCounter++;
                legalMoves++;

                foreach (ulong hash in RepetitionHistory)
                {
                    if (tempBoard.zobristHash == hash) { repeatedMoves++; }
                }

                if (repeatedMoves >= 3)
                {
                    bestSoFar = Math.Max(bestSoFar, 0);
                }
                else
                {
                    bestSoFar = Math.Max(bestSoFar, -NegaMax(tempBoard, ref info, depth - 1, ply + 1, -beta, -alpha));
                }

                if (alpha < bestSoFar)
                {
                    alpha = bestSoFar;
                    if (ply == 0 && bestSoFar >= alpha)
                    {
                        TempBestScore = bestSoFar;
                        TempBestMove = move;
                    }
                }

                if (beta <= bestSoFar)
                {
                    RepetitionHistory.Pop();
                    return bestSoFar;
                }


                if (info.TimeManager.CheckTime())
                {
                    SearchCompleted = false;
                    break;
                }

                RepetitionHistory.Pop();
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

                    Search.NodesCounter++;

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

