using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TortoiseBot.Core.MoveGeneration
{
    public struct Move
    {
        public int StartSquare;
        public int FinalSquare;
        public MoveFlag flag;

        public Move(int startSquare, int finalSquare, MoveFlag moveFlag)
        {
            StartSquare = startSquare;
            FinalSquare = finalSquare;
            flag = moveFlag;
        }
    }

    public struct MoveList
    {
        private const int MAX_LENGTH = 256;
        public Move[] Moves;
        public int Length;

        public MoveList()
        {
            Moves = new Move[MAX_LENGTH];
            Length = 0;
        }
    }


    [Flags]
    public enum MoveFlag : byte
    {
        Default = 0,
        EnPassant = 1,
        DoublePush = 2,
        Capture = 4,
        PromoteToKnight = 8,
        PromoteToBishop = 9,
        PromoteToRook = 10,
        PromoteToQueen = 11,
        Castle = 16
    }
}
