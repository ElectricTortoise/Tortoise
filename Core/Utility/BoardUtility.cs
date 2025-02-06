using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;


namespace TortoiseBot.Core.Utility
{
    public class BoardUtility
    {
        public static int GetSquareIndex(string square)
        {
            return ((square[0] - 'a') + (('8' - square[1]) * 8));
        }

        public static string GetSquareName(int square)
        {
            return (((char)(square % 8 + 'a')).ToString() + ((char)('8' - square / 8)).ToString());
        }

        public static bool IsBitOnBitboard(ulong bitboard, int index)
        {
            return (bitboard & (1UL << index)) != 0;
        }

        public static int[] GetBitIndices(ulong value)
        {
            Span<int> indices = stackalloc int[64];
            int count = 0;

            while (value != 0)
            {
                int index = BitOperations.TrailingZeroCount(value);
                indices[count++] = index;
                value &= value - 1;
            }

            return indices.Slice(0, count).ToArray();
        }
    }
}
