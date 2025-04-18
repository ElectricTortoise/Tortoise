﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Tortoise.Core
{
    public static unsafe class MoveOrderer
    {
        public static void OrderMoves(ref Board board, ref MoveList moveList, int[,,] HistoryTable, ushort TTmove)
        {
            Span<ScoredMove> ScoredMoves = stackalloc ScoredMove[moveList.Length];

            for (int i = 0; i < moveList.Length; i++)
            {
                if (moveList.Moves[i] == TTmove)
                {
                    ScoredMoves[i] = new ScoredMove(TTmove, 1000000);
                }
                else
                {
                    ScoredMove scoredMove = new ScoredMove(ref board, moveList.Moves[i]);
                    if ((scoredMove.move.flag & MoveFlag.Capture) == 0)
                    {
                        scoredMove.score += HistoryTable[board.boardState.GetColourToMove(), scoredMove.move.StartSquare, scoredMove.move.FinalSquare];
                    } 
                    ScoredMoves[i] = scoredMove;
                }
            }

            ScoredMoves.Sort(new MoveSorter());

            for (int i = 0; i < moveList.Length; i++)
            {
                moveList.Moves[i] = ScoredMoves[i].move.EncodeMove();
            }
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
