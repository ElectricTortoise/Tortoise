using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;


namespace TortoiseBot.Core
{
    public static unsafe class Search
    {
        public static int counter;
        public static int bestScore;
        public static Move BestMove;

        public static int NegaMax(Board board, int depth, int ply, int alpha, int beta)
        {

            if (depth == 0)
            {
                //return Evaluation.Evaluate(board);
                return Quiesce(board, alpha, beta);
            }

            int bestSoFar = -SearchConstants.SCORE_INF;
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
                counter++;
                legalMoves++;

                bestSoFar = Math.Max(bestSoFar, -NegaMax(tempBoard, depth - 1, ply + 1, -beta, -alpha));
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
            }

            if (legalMoves == 0)
            {
                int kingSquare = board.kingSquares[board.boardState.GetColourToMove()]; 
                if (MoveGenUtility.IsInCheck(board, kingSquare, board.boardState.GetOpponentColour()))
                {
                    return -SearchConstants.SCORE_MATE + ply;
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

                    Search.counter++;

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

