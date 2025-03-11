using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TortoiseBot.Core
{
    public static class SearchConstants
    {
        public const int AlphaStart = -EvaluationConstants.ScoreMate;
        public const int BetaStart = EvaluationConstants.ScoreMate;

        public const int MaxSearchTime = int.MaxValue - 1;
        public const ulong MaxSearchNodes = ulong.MaxValue - 1;
        public const int MaxDepth = 64;
        public const int DefaultMovesToGo = 20;
    }
}
