using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tortoise.Core
{
    public struct TTEntry
    {
        public ulong zobristHash;
        public ushort move;
        public short score;
        public byte depth;
        public byte type; 

        public TTEntry(ulong zobristHash, ushort move, short score, byte depth, byte type)
        {
            this.zobristHash = zobristHash;
            this.move = move;
            this.score = score;
            this.depth = depth;
            this.type = type;
        }

        public override string ToString()
        {
            string typeName = (type & TranspositionTable.TT_BOUND_MASK) switch
            {
                SearchConstants.NodeBoundNone => "N",
                SearchConstants.NodeBoundLower => "L",
                SearchConstants.NodeBoundUpper => "U",
                SearchConstants.NodeBoundExact => "E",
                _ => ""
            };
            return $"hash {zobristHash} \nmove {move} \nscore {score} \ndepth {depth} \ntype {typeName} \n";
        }
    }
}
