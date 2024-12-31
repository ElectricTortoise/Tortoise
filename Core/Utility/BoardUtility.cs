﻿using System;
using System.Collections.Generic;
using System.Linq;
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

        public static bool IsBitOnBitboard(ulong bitboard, int index)
        {
            return (bitboard & (1UL << index)) != 0;
        }
    }
}
