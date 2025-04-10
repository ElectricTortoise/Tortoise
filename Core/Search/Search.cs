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

        public static TranspositionTable TranspositionTable;

        //for debugging
        public static int TThit;
        public static int TTsucceed;
        public static int SearchNodesCounter;
        public static int QSearchNodesCounter;
        public static int NodesCounter => SearchNodesCounter + QSearchNodesCounter;

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
            RootBestScore = 0;

            TimeManager.TotalSearchTime.Start();
            try
            {
                for (int searchDepth = 1; searchDepth < info.DepthLimit + 1; searchDepth++)
                {
                    TThit = 0;
                    TTsucceed = 0;
                    SearchNodesCounter = 0;
                    QSearchNodesCounter = 0;

                    int hashFullness = 0;

                    RootBestScore = NegaMax(board, ref info, searchDepth, 0, -EvaluationConstants.ScoreInfinite, EvaluationConstants.ScoreInfinite);
                    if (SearchCompleted)
                    {
                        RootBestMove = BestMove;
                    }

                    for (int i = 0; i < 1000; i++)
                    {
                        if (TranspositionTable.entries[i].zobristHash != 0)
                        {
                            hashFullness++;
                        }
                    }

                    long timeInMS = TimeManager.TotalSearchTime.ElapsedMilliseconds;
                    int nps = (int)((NodesCounter / Math.Max(1, timeInMS)) * 1000);
                    move = Utility.MoveToString(RootBestMove);

                    if (info.TimeManager.CheckTime())
                    {
                        break;
                    }

                    Console.WriteLine($"info depth {searchDepth} time {timeInMS} score cp {RootBestScore} nodes {NodesCounter} nps {nps} hashfull {hashFullness} pv {move}");
                }
            }
            catch (TimeoutException)
            {
                
            }

            TimeManager.TotalSearchTime.Reset();

            Console.WriteLine($"bestmove {move}");
            info.SearchActive = false;
        }

        public static int NegaMax(Board board, ref SearchInformation info, int depth, int ply, int alpha, int beta)
        {
            SearchCompleted = true;

            int repeatedMoves = 0;
            if (ply != 0) //implement twofold detection for post-root, threefold detection for pre-root and skip every other move (since I cannot play my opponent's moves)
            {
                foreach (ulong hash in RepetitionHistory)
                {
                    if (board.zobristHash == hash) { repeatedMoves++; }
                }
                if (repeatedMoves >= 3 || board.boardState.halfmoveClock >= 100)
                {
                    return EvaluationConstants.ScoreDraw;
                }
            }

            ulong TTIndex = board.zobristHash % (ulong)TranspositionTable.entries.Length;
            TTEntry entry = TranspositionTable.entries[TTIndex];
            if (ply != 0)
            {
                if (entry.zobristHash == board.zobristHash)
                {
                    TThit++;
                    if (entry.depth >= depth)
                    {
                        var entryBound = entry.type & TranspositionTable.TT_BOUND_MASK;
                        if ((entryBound == SearchConstants.NodeBoundExact) ||
                           ((entryBound == SearchConstants.NodeBoundUpper) && (entry.score <= alpha)) ||
                           ((entryBound == SearchConstants.NodeBoundLower) && (entry.score >= beta)))
                        {
                            TTsucceed++;
                            return entry.score;
                        }
                    }
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
            ushort bestMove = SearchConstants.NullMove;
            byte nodeBound = SearchConstants.NodeBoundUpper;

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

                SearchNodesCounter++;
                legalMoves++;

                RepetitionHistory.Push(tempBoard.zobristHash);
                bestSoFar = Math.Max(bestSoFar, -NegaMax(tempBoard, ref info, depth - 1, ply + 1, -beta, -alpha));
                RepetitionHistory.Pop();

                if (info.TimeManager.CheckTime())
                {
                    SearchCompleted = false;
                    throw new TimeoutException();
                }

                if (bestSoFar > alpha)
                {
                    bestMove = move;
                    alpha = bestSoFar;
                    nodeBound = SearchConstants.NodeBoundExact;
                    if (ply == 0 && bestSoFar >= alpha)
                    {
                        BestScore = bestSoFar;
                        BestMove = move;
                    }
                }

                if (bestSoFar >= beta)
                {
                    nodeBound = SearchConstants.NodeBoundLower;
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

            TranspositionTable.Add(board.zobristHash, new TTEntry(board.zobristHash, bestMove, (short)bestSoFar, (byte)depth, nodeBound));

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
            ushort bestMove = SearchConstants.NullMove;

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

                    QSearchNodesCounter++;


                    bestSoFar = Math.Max(bestSoFar, -Quiesce(tempBoard, -beta, -alpha));
                    alpha = Math.Max(alpha, bestSoFar);
                    if (bestSoFar > alpha)
                    {
                        bestMove = move;
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

