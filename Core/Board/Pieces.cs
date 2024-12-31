using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TortoiseBot.Core.Board
{
    public class Pieces
    {
        public static int? GetPieceColour(int piece)
        {
            if (piece == 0)
            {
                return null;
            }
            else
            {
                return piece & 1; // Black = 0, White = 1
            }
        }

        public static int GetPieceType(int piece)
        {
            return piece >> 1; // P=0, R=1, N=2, B=3, Q=4, K=5
        }
    }
}
