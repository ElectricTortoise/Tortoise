using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;


namespace Tortoise.Core
{
    public static class Utility
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetSquareIndex(string square)
        {
            return ((square[0] - 'a') + (('8' - square[1]) * 8));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetSquareName(int square)
        {
            return (((char)(square % 8 + 'a')).ToString() + ((char)('8' - square / 8)).ToString());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBitOnBitboard(ulong bitboard, int index)
        {
            return (bitboard & (1UL << index)) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string MoveToString(Move move)
        {
            string startSquare = GetSquareName(move.StartSquare);
            string finalSquare = GetSquareName(move.FinalSquare);
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

            return $"{startSquare}{finalSquare}{flag}";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string MoveToString(ushort move)
        {
            return MoveToString(new Move(move));
        }
    }
}
