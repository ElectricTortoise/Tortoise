using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TortoiseBot.Core
{
    public unsafe struct SearchInformation
    {
        public Board board;
        public TimeManager TimeManager;

        public int DepthLimit = SearchConstants.MaxDepth;
        public ulong NodeLimit = SearchConstants.MaxSearchNodes;
        public ulong SoftNodeLimit = SearchConstants.MaxSearchNodes;


        public bool SearchActive = false;

        public SearchInformation(Board board, int depth = SearchConstants.MaxDepth, int searchTime = SearchConstants.MaxSearchTime)
        {
            this.board = board;
            this.DepthLimit = depth;

            this.TimeManager = new TimeManager();
            this.TimeManager.MaxSearchTime = searchTime;
        }

        public void SetMoveTime(int moveTime)
        {
            TimeManager.MaxSearchTime = moveTime;
            TimeManager.HasMoveTime = true;
        }
    }
}
