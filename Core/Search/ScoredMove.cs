using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace Tortoise.Core
{
    public unsafe struct ScoredMove
    {
        public int score;
        public Move move;

        public ScoredMove(ref Board board, Move move)
        {
            this.score = 0;
            this.move = move;

            if ((move.flag & MoveFlag.Capture) != 0)
            {
                this.score += board.pieceTypesBitboard[move.FinalSquare]/*victim*/ * 1000 - board.pieceTypesBitboard[move.StartSquare] /*attacker*/ + 100000;
            }

            if ((move.flag & MoveFlag.PromoteToKnight) != 0)
            {
                this.score += 100;
            }

            //if ((move.flag & MoveFlag.Castle) != 0)
            //{
            //    this.score += 50;
            //}
        }

        public ScoredMove(ref Board board, ushort move)
        {
            this = new ScoredMove(ref board, new Move(move));
        }
    }
}
