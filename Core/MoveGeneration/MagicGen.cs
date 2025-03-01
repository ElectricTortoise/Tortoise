using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TortoiseBot.Core
{
    public unsafe class MagicGen
    {
        public MagicGen()
        {
            ulong[] RookMagics = GenerateMagics(true);
            ulong[] BishopMagics = GenerateMagics(false);


            foreach (var rookMagic in RookMagics)
                Console.Write("0x" + Convert.ToString((long)rookMagic, 16) + ", ");
            Console.WriteLine();
            Console.WriteLine();
            foreach (var bishopMagic in BishopMagics)
                Console.Write("0x" + Convert.ToString((long)bishopMagic, 16) + ", ");
        }

        private static List<ulong> GenerateBlockers(int square, bool isRook)
        {
            ulong bitboard = isRook ? PrecomputedData.OrthogonalMask[square] : PrecomputedData.DiagonalMask[square];

            List<ulong> blockers = new List<ulong>();
            ulong subset = 0;
            do
            {
                blockers.Add(subset);
                subset = subset - bitboard & bitboard;
            } while (subset != 0);

            return blockers;
        }

        // GenerateMoveBoards() produces attack masks in an order which corresponds to the blockers in GenerateBlockers()
        private static List<ulong> GenerateMoveBoards(int square, bool isRook)
        {
            ulong bitboard = isRook ? PrecomputedData.OrthogonalMask[square] : PrecomputedData.DiagonalMask[square];
            List<ulong> blockerArray = GenerateBlockers(square, isRook);
            List<ulong> attackMasks = new List<ulong>();

            for (int i = 0; i < blockerArray.Count; i++)
            {
                ulong blockerArrangement = blockerArray[i];
                attackMasks.Add(MoveGenUtility.IterativeGenerateSliderAttackMask(square, blockerArrangement, isRook));
            }
            return attackMasks;
        }

        public static ulong[] GenerateMagics(bool isRook)
        {
            ulong[] magics = new ulong[64];

            for (int square = 0; square < 64; square++)
            {
                List<ulong> blockers = GenerateBlockers(square, isRook);

                bool finishedGenerating = false;

                do
                {

                    Random random = new Random();

                    ulong magicCandidate = (ulong)random.NextInt64() & (ulong)random.NextInt64() & (ulong)random.NextInt64();

                    int tableLength = blockers.Count;
                    ulong[] seenIndices = new ulong[tableLength];

                    int shift = 64 - (int)Math.Log2(tableLength);

                    for (int i = 0; i < tableLength; i++)
                    {
                        ulong blocker = blockers[i];

                        ulong hash = (magicCandidate * blocker) >> shift;

                        if (seenIndices[hash] == 0)
                        {
                            finishedGenerating = true;
                            seenIndices[hash] = 1;
                        }
                        else
                        {
                            finishedGenerating = false;
                            break;
                        }
                    }

                    magics[square] = magicCandidate;
                } while (!finishedGenerating);
            }

            return magics;
        }

        public static ulong[][] GenerateLookupTable(bool isRook)
        {
            ulong[][] lookupTable = new ulong[64][];

            for (int square = 0; square < 64; square++)
            {
                List<ulong> blockers = GenerateBlockers(square, isRook);
                List<ulong> attacks = GenerateMoveBoards(square, isRook);
                ulong magic = isRook ? RookMagics[square] : BishopMagics[square];
                int shift = isRook ? RookShifts[square] : BishopShifts[square];

                int tableLength = blockers.Count;
                ulong[] squareTable = new ulong[tableLength];

                for (int i = 0; i < tableLength; i++)
                {
                    ulong blocker = blockers[i];
                    ulong hash = (magic * blocker) >> shift;

                    squareTable[hash] = attacks[i];
                }

                lookupTable[square] = squareTable;
            }

            return lookupTable;
        }

        public static readonly int[] RookShifts =
        {
            52, 53, 53, 53, 53, 53, 53, 52, 
            53, 54, 54, 54, 54, 54, 54, 53,
            53, 54, 54, 54, 54, 54, 54, 53, 
            53, 54, 54, 54, 54, 54, 54, 53, 
            53, 54, 54, 54, 54, 54, 54, 53,
            53, 54, 54, 54, 54, 54, 54, 53, 
            53, 54, 54, 54, 54, 54, 54, 53,
            52, 53, 53, 53, 53, 53, 53, 52
        };

        public static readonly int[] BishopShifts =
        {
            58, 59, 59, 59, 59, 59, 59, 58, 
            59, 59, 59, 59, 59, 59, 59, 59,
            59, 59, 57, 57, 57, 57, 59, 59, 
            59, 59, 57, 55, 55, 57, 59, 59, 
            59, 59, 57, 55, 55, 57, 59, 59,
            59, 59, 57, 57, 57, 57, 59, 59, 
            59, 59, 59, 59, 59, 59, 59, 59, 
            58, 59, 59, 59, 59, 59, 59, 58
        };

        public static readonly ulong[] RookMagics =
        {
            0x280004000221080, 0x104000100c422000, 0x200082111820040, 0x80080080045000,
            0xa001060040e0028, 0x30011002a140008, 0x400048804020150, 0x2a00084080240102,
            0x911002105800040, 0x400150082000, 0x211003300406000, 0x1801000080280,
            0x80800802240080, 0x1003000802040100, 0x100808002003100, 0x880030001c080,
            0x5000208000c00280, 0x8404000201000, 0x1100888020015000, 0x10010020481100,
            0x3400050008010010, 0x402080800c000a00, 0x20040010030802, 0x80002000100d284,
            0x4440084080042080, 0x31200480400880, 0x3000200080300084, 0x1100080480080,
            0x40040080280080, 0x2424040080800200, 0x20400050810, 0x810200008044,
            0x44426c000800080, 0x800402000c01000, 0x1010080020200400, 0x80080080801002,
            0x391008803000430, 0x4913000209001400, 0x440820824001011, 0x10000044020002a1,
            0x1884001a08000, 0x1510831040010020, 0x220220048820011, 0x12042002112000a,
            0x400080080800c, 0x2108840002008080, 0x4021000200310004, 0x4000320488a0004,
            0x40c18011610100, 0x5008804001200080, 0x1e0090020401100, 0x148000884100280,
            0x11204c0118008080, 0x49001294004900, 0xc1000402000100, 0x4009044110600,
            0x1473402015028003, 0x4c00100208b4091, 0x20014090604901, 0x2205001000212c89,
            0x11000800040211, 0x100040022c801, 0x2500811020820804, 0x2000004508840022
        };

        public static readonly ulong[] BishopMagics =
        {
            0x2002880113060202, 0x5080a04002401, 0x2030008202440000, 0x3051040080004080,
            0x13114004490000, 0x350a121220014048, 0x10108080a2902a1, 0x2016008084332042,
            0x8200204480080, 0x4000204800810040, 0x108100410020, 0xc424040088010c,
            0x8040410a8000404, 0x9206501c2104, 0x220104424000, 0x1002408400880440,
            0x1240009222020408, 0x40100018120b8400, 0x808001000851310, 0x10200104018014,
            0x104000202110050, 0x900808080801, 0x40002a2011084, 0x5000a00100825000,
            0x582408c0040802, 0x2200018080080, 0x80300222108200, 0x808008020002,
            0x1201011011004000, 0x33020400480400, 0x12a20041011002, 0x4002011410410,
            0x2103421112007, 0x6c50882000a40408, 0x18020a840400, 0xa404800008200,
            0x1220002080040084, 0x21830602230080, 0x21040c00050120, 0x2004040080002289,
            0x88084002681a, 0x22082148014401, 0x109090000800, 0x2141084200801800,
            0x10081900421400, 0x212010810a040040, 0x2114100081090220, 0x12040842000088,
            0x40441018090020, 0x4221110282000, 0x4c84004404310480, 0xc40000020a81002,
            0xa1184005050300, 0x20200a12020010, 0x40502c0124040044, 0x1120826181010202,
            0x200440208014402, 0x5400520200820800, 0x220084208d000, 0x482420600,
            0x20400000400d0500, 0x210b240b0128080, 0x20028202400822c, 0x404021180100d282
        };
    }
}
