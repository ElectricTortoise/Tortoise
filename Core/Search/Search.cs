using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
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

        public static ButterflyHistory History;

        //for debugging
        public static int TThit;
        public static int TTsucceed;
        public static int SearchNodesCounter;
        public static int QSearchNodesCounter;
        public static int NodesCounter => SearchNodesCounter + QSearchNodesCounter;
        public static int AlphaRaises;
        public static int BetaCutoffs;

        private static bool SearchCompleted;
        private static int BestScore;
        private static ushort BestMove;
        public static int RootBestScore;
        public static ushort RootBestMove;
        public static ushort[,] PVMoves;

        static Search()
        {
            RepetitionHistory = new Stack<ulong>();
            History = new ButterflyHistory();
            PVMoves = new ushort[256, 256];
        }

        public static void StartSearch(Board board, ref SearchInformation info)
        {
            info.SearchActive = true;
            RootBestScore = 0;

            TimeManager.TotalSearchTime.Start();
            try
            {
                TThit = 0;
                TTsucceed = 0;
                AlphaRaises = 0;
                BetaCutoffs = 0;
                SearchNodesCounter = 0;
                QSearchNodesCounter = 0;
                for (int searchDepth = 1; searchDepth < info.DepthLimit + 1; searchDepth++)
                {
                    Array.Clear(PVMoves);
                    int hashFullness = 0;
                    string move = "";

                    int alpha = -EvaluationConstants.ScoreInfinite;
                    int beta = EvaluationConstants.ScoreInfinite;
                    int delta = SearchConstants.DefaultAspirationWindow;

                    if (searchDepth >= 5)
                    {
                        alpha = Math.Max(BestScore - delta, -EvaluationConstants.ScoreInfinite);
                        beta = Math.Min(BestScore + delta, EvaluationConstants.ScoreInfinite);
                    }

                    while (true)
                    {
                        BestScore = NegaMax(board, ref info, searchDepth, 0, alpha, beta);

                        if (BestScore <= alpha)
                        {
                            alpha -= delta;
                        }
                        else if (BestScore >= beta)
                        {
                            beta += delta;
                        }
                        else
                        {
                            break;
                        }

                        delta += delta;
                    }

                    if (SearchCompleted)
                    {
                        RootBestMove = BestMove;
                        RootBestScore = BestScore;
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

                    for (int i = 0; i <= searchDepth - 1; i++)
                    {
                        move += ($"{Utility.MoveToString(PVMoves[0, i])} ");
                    }

                    Console.WriteLine($"info depth {searchDepth} time {timeInMS} score cp {RootBestScore} nodes {NodesCounter} nps {nps} hashfull {hashFullness} pv {move}");
                }
            }
            catch (TimeoutException)
            {

            }

            TimeManager.TotalSearchTime.Reset();

            Console.WriteLine($"bestmove {Utility.MoveToString(RootBestMove)}");
            info.SearchActive = false;
        }

        public static int NegaMax(Board board, ref SearchInformation info, int depth, int ply, int alpha, int beta)
        {
            //initialize some values
            SearchCompleted = true;
            byte nodeType = SearchConstants.NodeTypeNull;
            bool isPV = (beta - alpha > 1);

            //threefold
            int repeatedMoves = 0;
            if (ply != 0) //To-do: implement twofold detection for post-root, threefold detection for pre-root and skip every other move (since I cannot play my opponent's moves)
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

            //TT cutoff
            ulong TTIndex = board.zobristHash % (ulong)TranspositionTable.entries.Length;
            TTEntry entry = TranspositionTable.entries[TTIndex];
            if (ply != 0 && !isPV)
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

            //QS
            if (depth <= 0)
            {
                return Quiesce(board, alpha, beta);
            }

            //RFP
            if (depth <= 3 && !isPV && !MoveGenUtility.IsInCheck(board, board.kingSquares[board.boardState.GetColourToMove()], board.boardState.GetOpponentColour()))
            {
                int staticEval = Evaluation.Evaluate(board);
                if ((staticEval - depth * SearchConstants.RFPMargin) >= beta)
                {
                    return staticEval;
                }
            }

            //generate moves
            int bestSoFar = -EvaluationConstants.ScoreInfinite;
            int legalMoves = 0;
            MoveList moveList = new MoveList();
            MoveGen.GenAllMoves(board, ref moveList);
            MoveOrderer.OrderMoves(ref board, ref moveList, History.ButterflyTable, entry.move);

            //initialise remaining variables
            Board tempBoard;
            ushort bestMove = SearchConstants.NullMove;
            byte nodeBound = SearchConstants.NodeBoundUpper;

            //moveloop
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

                SearchNodesCounter++;
                legalMoves++;

                RepetitionHistory.Push(tempBoard.zobristHash);
                if (i == 0)
                {
                    bestSoFar = Math.Max(bestSoFar, -NegaMax(tempBoard, ref info, depth - 1, ply + 1, -beta, -alpha));
                }
                else
                {
                    bestSoFar = Math.Max(bestSoFar, -NegaMax(tempBoard, ref info, depth - 1, ply + 1, -alpha - 1, -alpha)); //zws
                    if (alpha < bestSoFar && bestSoFar < beta)
                    {
                        bestSoFar = Math.Max(bestSoFar, -NegaMax(tempBoard, ref info, depth - 1, ply + 1, -beta, -alpha)); //full window search if zws fails-high
                    }
                }
                RepetitionHistory.Pop();

                if (info.TimeManager.CheckTime())
                {
                    SearchCompleted = false;
                    throw new TimeoutException();
                }

                if (bestSoFar > alpha)
                {
                    AlphaRaises++;
                    bestMove = move.EncodeMove();
                    alpha = bestSoFar;
                    nodeBound = SearchConstants.NodeBoundExact;
                    if (ply == 0)
                    {
                        BestScore = bestSoFar;
                        BestMove = bestMove;
                    }
                    if (isPV)
                    {
                        for (int j = 0; j < 256; j++)
                        {
                            ushort pv = PVMoves[ply + 1, j];

                            if (pv == SearchConstants.NullMove)
                            {
                                PVMoves[ply, j + 1] = pv;
                                break;
                            }
                            PVMoves[ply, j + 1] = pv;
                        }
                        PVMoves[ply, 0] = bestMove;
                    }
                }

                if (bestSoFar >= beta)
                {
                    BetaCutoffs++;
                    if ((move.flag & MoveFlag.Capture) == 0)
                    {
                        History.Add(5, board.boardState.GetColourToMove(), move.StartSquare, move.FinalSquare);
                    }
                    nodeBound = SearchConstants.NodeBoundLower;
                    break;
                }

                if ((move.flag & MoveFlag.Capture) == 0)
                {
                    History.Add(-5, board.boardState.GetColourToMove(), move.StartSquare, move.FinalSquare);
                }
            }

            //legality check
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

            nodeType |= nodeBound;

            TranspositionTable.Add(board.zobristHash, new TTEntry(board.zobristHash, bestMove, (short)bestSoFar, (byte)depth, nodeType));

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
            MoveOrderer.OrderMoves(ref board, ref moveList, History.ButterflyTable, SearchConstants.NullMove);

            Board tempBoard;

            for (int i = 0; i < moveList.Length; i++)
            {
                Move move = new Move(moveList.Moves[i]);
                if ((move.flag & MoveFlag.Capture) != 0)
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
    

        private static void PrintPVTable(int searchDepth)
        {
            for (int i = 0; i <= searchDepth - 1; i++)
            {
                for (int j = 0; j <= searchDepth - 1; j++)
                {
                    Console.Write($"{Utility.MoveToString(PVMoves[i, j])} ");
                }
                Console.WriteLine();
            }
            Console.WriteLine();
            Console.WriteLine();
        }
    }
}
