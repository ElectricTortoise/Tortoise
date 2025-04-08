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

            TimeManager.TotalSearchTime.Reset();

            Console.WriteLine($"bestmove {move}");
            info.SearchActive = false;
        }

        public static int NegaMax(Board board, ref SearchInformation info, int depth, int ply, int alpha, int beta)
        {
            SearchCompleted = true;

            ulong TTIndex = board.zobristHash % (uint)TranspositionTable.entries.Length;
            TTEntry entry = TranspositionTable.entries[TTIndex];
            if (ply != 0)
            {
                if (entry.zobristHash == board.zobristHash)
                {
                    if (entry.depth >= depth)
                    {
                        var entryBound = entry.type & TranspositionTable.TT_BOUND_MASK;
                        if ((entryBound == SearchConstants.NodeBoundExact) ||
                           ((entryBound == SearchConstants.NodeBoundUpper) && (entry.score <= alpha)) ||
                           ((entryBound == SearchConstants.NodeBoundLower) && (entry.score >= beta)))
                        {
                            return entry.score;
                        }
                    }
                }
            }

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
            byte nodeBound = SearchConstants.NodeBoundExact;
            byte nodeType = SearchConstants.NodeTypeNull;

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
                    bestMove = move;
                    alpha = bestSoFar;
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

            if (bestMove == SearchConstants.NullMove)
            {
                nodeBound = SearchConstants.NodeBoundUpper;
            }

            nodeType |= nodeBound;

            TranspositionTable.Add(board.zobristHash, new TTEntry(board.zobristHash, bestMove, (short)bestSoFar, (byte)depth, nodeType));

            return bestSoFar;
        }

        public static int Quiesce(Board board, int alpha, int beta)
        {
            ulong TTIndex = board.zobristHash % (uint)TranspositionTable.entries.Length;
            TTEntry entry = TranspositionTable.entries[TTIndex];
            if (entry.zobristHash == board.zobristHash)
            {
                if (entry.depth >= 0)
                {
                    if (((entry.type & TranspositionTable.TT_BOUND_MASK) == SearchConstants.NodeBoundExact) ||
                        (((entry.type & TranspositionTable.TT_BOUND_MASK) == SearchConstants.NodeBoundUpper) && (entry.score <= alpha)) ||
                        (((entry.type & TranspositionTable.TT_BOUND_MASK) == SearchConstants.NodeBoundLower) && (entry.score >= beta)))
                    {
                        return entry.score;
                    }
                }
            }

            //return if position is worse beta, else increase alpha to position value
            int standPat = Evaluation.Evaluate(board);
            int bestSoFar = standPat;
            alpha = Math.Max(alpha, standPat);
            if (standPat >= beta)
            {
                TranspositionTable.Add(board.zobristHash, new TTEntry(board.zobristHash, SearchConstants.NullMove, (short)bestSoFar, 0, SearchConstants.NodeBoundNone));
                return standPat;
            }

            MoveList moveList = new MoveList();
            MoveGen.GenAllMoves(board, ref moveList); // a bit of time is wasted generating non-capture moves
            MoveOrderer.OrderMoves(ref board, ref moveList);

            Board tempBoard;
            ushort bestMove = SearchConstants.NullMove;
            byte nodeBound = SearchConstants.NodeBoundExact;
            byte nodeType = SearchConstants.NodeTypeNull;

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
                        bestMove = move;
                        alpha = bestSoFar;
                    }
                    if (bestSoFar >= beta)
                    {
                        nodeBound = SearchConstants.NodeBoundLower;
                        return bestSoFar;
                    }
                }
                else 
                {
                    return bestSoFar;
                }
            }

            if (bestMove == SearchConstants.NullMove)
            {
                nodeBound = SearchConstants.NodeBoundUpper;
            }

            nodeType |= nodeBound;

            TranspositionTable.Add(board.zobristHash, new TTEntry(board.zobristHash, bestMove, (short)bestSoFar, 0, nodeBound));

            return bestSoFar;
        }
    }
}

