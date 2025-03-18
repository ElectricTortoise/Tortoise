using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Tortoise.Core
{
    public readonly struct Move
    {
        public readonly byte StartSquare;
        public readonly byte FinalSquare;
        public readonly MoveFlag flag;

        public Move(byte startSquare, byte finalSquare, MoveFlag moveFlag)
        {
            StartSquare = startSquare;
            FinalSquare = finalSquare;
            flag = moveFlag;
        }

        public Move(ushort move)
        {
            flag = (MoveFlag)(move >> 12);
            StartSquare = (byte)((move >> 6) & 0b111111);
            FinalSquare = (byte)(move & 0b111111);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort EncodeMove() 
        {
            return (ushort)((ushort)((int)this.flag << 12) | this.StartSquare << 6 | this.FinalSquare);
        }
    }

    public unsafe struct MoveList
    {
        public fixed ushort Moves[256];
        public int Length;

        public MoveList()
        {
            Length = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddMove(int startSquare, int targetSquare, MoveFlag flag)
        {
            Move move = new Move((byte)startSquare, (byte)targetSquare, flag);
            this.Moves[this.Length] = move.EncodeMove();
            this.Length++;
        }
    }

    [Flags]
    public enum MoveFlag : byte
    {
        Default = 0,
        EnPassant = 1,
        DoublePush = 2,
        Castle = 3,
        Capture = 4,
        PromoteToKnight = 8,
        PromoteToBishop = 9,
        PromoteToRook = 10,
        PromoteToQueen = 11
    }
}
