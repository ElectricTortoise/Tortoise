using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tortoise.Core
{
    public static class SearchConstants
    {
        public const int AlphaStart = -EvaluationConstants.ScoreMate;
        public const int BetaStart = EvaluationConstants.ScoreMate;

        public const int MaxSearchTime = int.MaxValue - 1;
        public const ulong MaxSearchNodes = ulong.MaxValue - 1;
        public const int MaxDepth = 64;
        public const int DefaultMovesToGo = 20;

        public const ushort NullMove = 0;

        public const byte NodeTypeNull = 0;
        public const byte NodeBoundNone = 0b00;
        public const byte NodeBoundLower = 0b01;
        public const byte NodeBoundUpper = 0b10;
        public const byte NodeBoundExact = 0b11;
    }
}
