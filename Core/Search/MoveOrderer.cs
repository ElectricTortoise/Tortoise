using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Tortoise.Core
{
    public static unsafe class MoveOrderer
    {
        public static void OrderMoves(ref Board board, ref MoveList moveList)
        {
            Span<ScoredMove> ScoredMoves = stackalloc ScoredMove[moveList.Length];

            for (int i = 0; i < moveList.Length; i++)
            {
                ScoredMove scoredMove = new ScoredMove(ref board, moveList.Moves[i]);
                ScoredMoves[i] = scoredMove;
            }

            ScoredMoves.Sort(new MoveSorter());

            //for (int i = 0; i < moveList.Length; i++)
            //{
            //    Console.WriteLine($"{Utility.MoveToString(ScoredMoves[i].move)}, {ScoredMoves[i].move.flag}");
            //}

            //Console.WriteLine();
            //Console.WriteLine();
            //Console.WriteLine();
            //Console.WriteLine();


            for (int i = 0; i < moveList.Length; i++)
            {
                moveList.Moves[i] = ScoredMoves[i].move.EncodeMove();
            }

            //for (int i = 0; i < moveList.Length; i++)
            //{
            //    Console.WriteLine($"{Utility.MoveToString(new Move(moveList.Moves[i]))}, {new Move(moveList.Moves[i]).flag}");
            //}
        }
    }

    public class MoveSorter : IComparer<ScoredMove>
    {
        public int Compare(ScoredMove move1, ScoredMove move2)
        {
            if (move1.score > move2.score) { return -1; }
            else if (move1.score < move2.score) { return 1; }
            else { return 0; }
        }
    }
}
