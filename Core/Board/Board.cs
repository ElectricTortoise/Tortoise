using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TortoiseBot.Core.Utility;

namespace TortoiseBot.Core.Board
{
    public class Board
    {
        public PositionInfo currentPosition;

        public void LoadStartingPosition()
        {
            LoadPosition(FenUtility.StartFen);
        }

        public void LoadPosition(string fen)
        {
            this.currentPosition = FenUtility.FenStringParser(fen);
        }

    }
}
    

