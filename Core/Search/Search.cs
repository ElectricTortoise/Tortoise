using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TortoiseBot.Core.MoveGeneration;
using TortoiseBot.Core.Utility;

namespace TortoiseBot.Core.Search
{
    public static unsafe class Search
    {
        public static int NegaMax(Board.Board board, int depth, int alpha, int beta)
        {
            if (depth == 0)
            {
                return Evaluation.Evaluation.Evaluate(board);
            }

            int bestSoFar = int.MinValue;
            MoveList moveList = new MoveList();
            MoveGen.GenAllMoves(board, ref moveList);

            for (int i = 0; i < moveList.Length; i++)
            {
                Move move = new Move(moveList.Moves[i]);
                Board.Board tempBoard = board;
                tempBoard.MakeMove(move);

                tempBoard.boardState.whiteToMove = !tempBoard.boardState.whiteToMove;
                int kingSquare = tempBoard.kingSquares[tempBoard.boardState.whiteToMove ? Colour.White : Colour.Black];
                if (MoveGenUtility.IsInCheck(tempBoard, kingSquare))
                {
                    continue;
                }
                tempBoard.boardState.whiteToMove = !tempBoard.boardState.whiteToMove;

                int moveScore = -NegaMax(tempBoard, depth - 1, alpha, beta);
                if (moveScore > bestSoFar)
                {
                    bestSoFar = moveScore;
                }
            }

            return bestSoFar;
        }
    }
}
