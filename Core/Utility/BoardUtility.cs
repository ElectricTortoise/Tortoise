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
    }
}
