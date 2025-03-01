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
        public static int NegaMax(Board board, int depth, int ply, int alpha, int beta)
        {
            if (depth == 0)
            {
                return Evaluation.Evaluate(board);
            }

            int bestSoFar = -1000000;
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
                legalMoves++;

                int moveScore = -NegaMax(tempBoard, depth - 1, ply + 1, alpha, beta);
                if (moveScore > bestSoFar)
                {
                    bestSoFar = moveScore;
                }
            }

            if (legalMoves == 0)
            {
                int kingSquare = board.kingSquares[board.boardState.GetColourToMove()]; // opponent's king square
                if (MoveGenUtility.IsInCheck(board, kingSquare, board.boardState.GetOpponentColour())) // get original colour
                {
                    return -999999 + ply;
                }
                else
                {
                    return 0;
                }
            }
            return bestSoFar;
        }
    }
}
