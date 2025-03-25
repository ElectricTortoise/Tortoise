using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Tortoise.Core
{
    public unsafe ref struct MoveOrderer
    {
        public Board board;
        public Span<ushort> moves;

        public MoveOrderer(Board board, ref MoveList moveList)
        {
            this.board = board;
            this.moves = MemoryMarshal.CreateSpan(ref moveList.Moves[0], moveList.Length);
        }
       
        public void OrderMoves()
        {
            moves.Sort(new CaptureOrderer());
            int numberOfCaptures;
            for (numberOfCaptures = 0; numberOfCaptures < this.moves.Length; numberOfCaptures++)
            {
                if (((this.moves[numberOfCaptures] >> 12) & (int)MoveFlag.Capture) == 0) { break; }
            }

            if (numberOfCaptures != 0)
            {
                var MVVLVA = MemoryMarshal.CreateSpan(ref this.moves[0], numberOfCaptures);
                MVVLVA.Sort(new MVVLVA_Orderer(this.board));
            }
        }
    }


    public class CaptureOrderer : IComparer<ushort>
    {
        public int Compare(ushort move1, ushort move2)
        {
            
            if ((((move1 >> 12) & (int)MoveFlag.Capture) == (int)MoveFlag.Capture) && (((move2 >> 12) & (int)MoveFlag.Capture) != (int)MoveFlag.Capture)) { return -1; }
            else if ((((move1 >> 12) & (int)MoveFlag.Capture) != (int)MoveFlag.Capture) && (((move2 >> 12) & (int)MoveFlag.Capture) == (int)MoveFlag.Capture)) { return 1; }
            else { return 0; }
        }
    }

    public unsafe class MVVLVA_Orderer : IComparer<ushort> {

        private Board board;

        public MVVLVA_Orderer(Board board) 
        { 
            this.board = board;
        }

        public int Compare(ushort move1, ushort move2)
        {
            int startSquare1 = (byte)((move1 >> 6) & 0b111111);
            int finalSquare1 = (byte)(move1 & 0b111111);

            int attacker1 = this.board.pieceTypesBitboard[startSquare1];
            int victim1 = this.board.pieceTypesBitboard[finalSquare1];

            int MVVLVAScore1 = victim1 - attacker1;

            int startSquare2 = (byte)((move2 >> 6) & 0b111111);
            int finalSquare2 = (byte)(move2 & 0b111111);

            int attacker2 = this.board.pieceTypesBitboard[startSquare2];
            int victim2 = this.board.pieceTypesBitboard[finalSquare2];

            int MVVLVAScore2 = victim2 - attacker2;

            if (MVVLVAScore1 > MVVLVAScore2) { return -1; }
            else if (MVVLVAScore1 < MVVLVAScore2) { return 1; }
            else { return 0; }
        }
    }
}
