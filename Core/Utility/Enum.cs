using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Tortoise.Core
{
    public static class PieceType
    {
        public const byte Pawn = 0;
        public const byte Knight = 1;
        public const byte Bishop = 2;
        public const byte Rook = 3;
        public const byte Queen = 4;
        public const byte King = 5;
    }

    public static class Colour
    {
        public const byte Black = 0;
        public const byte White = 1;
    }

}
