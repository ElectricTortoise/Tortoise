using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TortoiseBot.Core.Board;
using TortoiseBot.Core.MoveGeneration;
using TortoiseBot.Core.Utility;

namespace TortoiseBot.Core.Perft
{
    public unsafe class Perft
    {
        public int FullPerft(Board.Board board, int depth)
        {
            int nodes = 0;
            MoveList moveList = new MoveList();

            if (depth == 0)
            {
                return 1;
            }

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
                nodes += FullPerft(tempBoard, depth-1);
            }

            return nodes;
        }

        public void DividedPerft(Board.Board board, int depth)
        {
            int total = 0;
            MoveList moveList = new MoveList();
            MoveGen.GenAllMoves(board, ref moveList);

            for (int node = 0; node < moveList.Length; node++) 
            {
                Board.Board tempBoard = board;
                Move move = new Move(moveList.Moves[node]);
                tempBoard.MakeMove(move);
                tempBoard.boardState.whiteToMove = !tempBoard.boardState.whiteToMove;
                int kingSquare = tempBoard.kingSquares[tempBoard.boardState.whiteToMove ? Colour.White : Colour.Black];

                if (MoveGenUtility.IsInCheck(tempBoard, kingSquare))
                {
                    continue;
                }

                tempBoard.boardState.whiteToMove = !tempBoard.boardState.whiteToMove;
                string startSquare = BoardUtility.GetSquareName(move.StartSquare);
                string finalSquare = BoardUtility.GetSquareName(move.FinalSquare);
                int count = FullPerft(tempBoard, depth - 1);
                total += count;
                char flag = move.flag switch
                {
                    MoveFlag.PromoteToKnight => 'n',
                    MoveFlag.PromoteToKnight | MoveFlag.Capture => 'n',
                    MoveFlag.PromoteToBishop => 'b',
                    MoveFlag.PromoteToBishop | MoveFlag.Capture => 'b',
                    MoveFlag.PromoteToRook => 'r',
                    MoveFlag.PromoteToRook | MoveFlag.Capture => 'r',
                    MoveFlag.PromoteToQueen => 'q',
                    MoveFlag.PromoteToQueen | MoveFlag.Capture => 'q',
                    _ => ' '
                };
                Console.WriteLine($"{startSquare}{finalSquare}{flag}: {count}");
            }

            Console.WriteLine($"Total positions: {total}");
        }
    }

}
