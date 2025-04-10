using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tortoise.Core
{
    public unsafe class TranspositionTable
    {
        public const int TT_BOUND_MASK = 0b11;

        public TTEntry[] entries;
        public ulong sizeInMiB;

        public TranspositionTable(ulong mb) 
        {
            this.sizeInMiB = mb;
            Initialize(mb);
        }

        public void Initialize(ulong mb)
        {
            this.entries = new TTEntry[(mb * 1024 * 1024) / (ulong)sizeof(TTEntry)];
        }

        ///<summary>
        ///Adds TTEntry at index (zobristHash % sizeofTT)
        ///</summary>
        public void Add(ulong zobristHash, TTEntry entry)
        {
            //Console.WriteLine(zobristHash % (uint)this.entries.Length);
            //Console.WriteLine(entry);
            this.entries[zobristHash % (ulong)this.entries.Length] = entry;
        }

        public void Clear()
        {
            Array.Clear(entries);
        }

    }
}
