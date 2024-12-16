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
        public Piece[,] Square;

        public Board()
        {
            Square = new Piece[8,8];
        }
    }
}
    

