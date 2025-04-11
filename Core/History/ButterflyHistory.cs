using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tortoise.Core
{

    public class ButterflyHistory
    {
        public int[,,] ButterflyTable;

        public ButterflyHistory() 
        {
            this.ButterflyTable = new int[2, 64, 64];
        }

        public void Add(int score, int colour, int startSquare, int targetSquare)
        {
            this.ButterflyTable[colour, startSquare, targetSquare] += score;
        }
    }
}