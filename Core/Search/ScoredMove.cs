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

        public ScoredMove(ref Board board, ref Move move)
        {
            this.score = 0;
            this.move = move;

            if ((move.flag | MoveFlag.PromoteToKnight) != 0)
            {
                this.score += 2;
            }

            if ((move.flag | MoveFlag.Capture) != 0)
            {
                this.score += board.pieceTypesBitboard[move.FinalSquare] - board.pieceTypesBitboard[move.StartSquare];
            }
        }

        public ScoredMove(ref Board board, ushort move)
        {
            int score = 0;
            this.move = new Move(move);

            MoveFlag flag = (MoveFlag)(move >> 12);
            int startSquare = (byte)(move >> 6 & 0b111111);
            int finalSquare = (byte)(move & 0b111111);

            if ((flag & MoveFlag.PromoteToKnight) != 0)
            {
                score += 2;
            }

            if ((flag & MoveFlag.Capture) != 0)
            {
                score += board.pieceTypesBitboard[finalSquare] - board.pieceTypesBitboard[startSquare];
            }
        }
    }
}
