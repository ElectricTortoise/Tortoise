using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
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
                nodes += FullPerft(tempBoard, depth - 1);
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

        public void CheckStandardPositions()
        {
            Board.Board board = new Board.Board();
            StreamReader sr = new StreamReader("C:\\Users\\Heng Yi\\source\\repos\\TortoiseBot\\Core\\Perft\\standard.epd");
            string line = sr.ReadLine();
            bool flag = false;
            while (line != null)
            {
                string[] data = line.Split(';');
                string fen = data[0];
                board.LoadPosition(fen);
                Console.WriteLine(fen);

                for (int i = 1; i < data.Length; i++) 
                { 
                    int depth = data[i].Split(' ')[0][1] - '0';
                    int numberOfPositions = Convert.ToInt32((data[i].Split(' ')[1]));
                    if (FullPerft(board, depth) == numberOfPositions)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write($"D{depth} Passed; ");
                    }
                    else 
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write($"D{depth} Failed; ");
                        flag = true; 
                    }
                }

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("");
                board.Clear();
                line = sr.ReadLine();
            }
            if (flag)
            {
                Console.WriteLine("Error occured");
            }
            sr.Close();
        }

        public void PerftSpeed(Board.Board board, int depth)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            int nodes = FullPerft(board, depth);
            watch.Stop();
            Console.WriteLine($"{nodes}, {watch.ElapsedMilliseconds}ms");
            Console.WriteLine($"{Convert.ToInt32((((float)nodes / (float)watch.ElapsedMilliseconds) * 1000))}nps");
        }
    }

}
