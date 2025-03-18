﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tortoise.Core
{
    public static class PrecomputedData
    {

        public static readonly ulong[] KnightMask =
        {
            0x0000000000020400, 0x0000000000050800, 0x00000000000A1100, 0x0000000000142200,
            0x0000000000284400, 0x0000000000508800, 0x0000000000A01000, 0x0000000000402000,
            0x0000000002040004, 0x0000000005080008, 0x000000000A110011, 0x0000000014220022,
            0x0000000028440044, 0x0000000050880088, 0x00000000A0100010, 0x0000000040200020,
            0x0000000204000402, 0x0000000508000805, 0x0000000A1100110A, 0x0000001422002214,
            0x0000002844004428, 0x0000005088008850, 0x000000A0100010A0, 0x0000004020002040,
            0x0000020400040200, 0x0000050800080500, 0x00000A1100110A00, 0x0000142200221400,
            0x0000284400442800, 0x0000508800885000, 0x0000A0100010A000, 0x0000402000204000,
            0x0002040004020000, 0x0005080008050000, 0x000A1100110A0000, 0x0014220022140000,
            0x0028440044280000, 0x0050880088500000, 0x00A0100010A00000, 0x0040200020400000,
            0x0204000402000000, 0x0508000805000000, 0x0A1100110A000000, 0x1422002214000000,
            0x2844004428000000, 0x5088008850000000, 0xA0100010A0000000, 0x4020002040000000,
            0x0400040200000000, 0x0800080500000000, 0x1100110A00000000, 0x2200221400000000,
            0x4400442800000000, 0x8800885000000000, 0x100010A000000000, 0x2000204000000000,
            0x0004020000000000, 0x0008050000000000, 0x00110A0000000000, 0x0022140000000000,
            0x0044280000000000, 0x0088500000000000, 0x0010A00000000000, 0x0020400000000000
        };

        public static readonly ulong[] DiagonalMask =
        {
            0x0040201008040200, 0x0000402010080400, 0x0000004020100a00, 0x0000000040221400, 
            0x0000000002442800, 0x0000000204085000, 0x0000020408102000, 0x0002040810204000, 
            0x0020100804020000, 0x0040201008040000, 0x00004020100a0000, 0x0000004022140000, 
            0x0000000244280000, 0x0000020408500000, 0x0002040810200000, 0x0004081020400000, 
            0x0010080402000200, 0x0020100804000400, 0x004020100a000a00, 0x0000402214001400, 
            0x0000024428002800, 0x0002040850005000, 0x0004081020002000, 0x0008102040004000, 
            0x0008040200020400, 0x0010080400040800, 0x0020100a000a1000, 0x0040221400142200, 
            0x0002442800284400, 0x0004085000500800, 0x0008102000201000, 0x0010204000402000, 
            0x0004020002040800, 0x0008040004081000, 0x00100a000a102000, 0x0022140014224000, 
            0x0044280028440200, 0x0008500050080400, 0x0010200020100800, 0x0020400040201000, 
            0x0002000204081000, 0x0004000408102000, 0x000a000a10204000, 0x0014001422400000, 
            0x0028002844020000, 0x0050005008040200, 0x0020002010080400, 0x0040004020100800, 
            0x0000020408102000, 0x0000040810204000, 0x00000a1020400000, 0x0000142240000000, 
            0x0000284402000000, 0x0000500804020000, 0x0000201008040200, 0x0000402010080400, 
            0x0002040810204000, 0x0004081020400000, 0x000a102040000000, 0x0014224000000000, 
            0x0028440200000000, 0x0050080402000000, 0x0020100804020000, 0x0040201008040200
        };

        public static readonly ulong[] OrthogonalMask =
        {
            0x000101010101017e, 0x000202020202027c, 0x000404040404047a, 0x0008080808080876, 
            0x001010101010106e, 0x002020202020205e, 0x004040404040403e, 0x008080808080807e, 
            0x0001010101017e00, 0x0002020202027c00, 0x0004040404047a00, 0x0008080808087600, 
            0x0010101010106e00, 0x0020202020205e00, 0x0040404040403e00, 0x0080808080807e00, 
            0x00010101017e0100, 0x00020202027c0200, 0x00040404047a0400, 0x0008080808760800, 
            0x00101010106e1000, 0x00202020205e2000, 0x00404040403e4000, 0x00808080807e8000, 
            0x000101017e010100, 0x000202027c020200, 0x000404047a040400, 0x0008080876080800, 
            0x001010106e101000, 0x002020205e202000, 0x004040403e404000, 0x008080807e808000, 
            0x0001017e01010100, 0x0002027c02020200, 0x0004047a04040400, 0x0008087608080800, 
            0x0010106e10101000, 0x0020205e20202000, 0x0040403e40404000, 0x0080807e80808000, 
            0x00017e0101010100, 0x00027c0202020200, 0x00047a0404040400, 0x0008760808080800, 
            0x00106e1010101000, 0x00205e2020202000, 0x00403e4040404000, 0x00807e8080808000, 
            0x007e010101010100, 0x007c020202020200, 0x007a040404040400, 0x0076080808080800,
            0x006e101010101000, 0x005e202020202000, 0x003e404040404000, 0x007e808080808000, 
            0x7e01010101010100, 0x7c02020202020200, 0x7a04040404040400, 0x7608080808080800, 
            0x6e10101010101000, 0x5e20202020202000, 0x3e40404040404000, 0x7e80808080808000
        };

        public static readonly ulong[] KingMask =
{
            0x0000000000000302, 0x0000000000000705, 0x0000000000000E0A, 0x0000000000001C14,
            0x0000000000003828, 0x0000000000007050, 0x000000000000E0A0, 0x000000000000C040,
            0x0000000000030203, 0x0000000000070507, 0x00000000000E0A0E, 0x00000000001C141C,
            0x0000000000382838, 0x0000000000705070, 0x0000000000E0A0E0, 0x0000000000C040C0,
            0x0000000003020300, 0x0000000007050700, 0x000000000E0A0E00, 0x000000001C141C00,
            0x0000000038283800, 0x0000000070507000, 0x00000000E0A0E000, 0x00000000C040C000,
            0x0000000302030000, 0x0000000705070000, 0x0000000E0A0E0000, 0x0000001C141C0000,
            0x0000003828380000, 0x0000007050700000, 0x000000E0A0E00000, 0x000000C040C00000,
            0x0000030203000000, 0x0000070507000000, 0x00000E0A0E000000, 0x00001C141C000000,
            0x0000382838000000, 0x0000705070000000, 0x0000E0A0E0000000, 0x0000C040C0000000,
            0x0003020300000000, 0x0007050700000000, 0x000E0A0E00000000, 0x001C141C00000000,
            0x0038283800000000, 0x0070507000000000, 0x00E0A0E000000000, 0x00C040C000000000,
            0x0302030000000000, 0x0705070000000000, 0x0E0A0E0000000000, 0x1C141C0000000000,
            0x3828380000000000, 0x7050700000000000, 0xE0A0E00000000000, 0xC040C00000000000,
            0x0203000000000000, 0x0507000000000000, 0x0A0E000000000000, 0x141C000000000000,
            0x2838000000000000, 0x5070000000000000, 0xA0E0000000000000, 0x40C0000000000000
        };

        public static readonly ulong[] BlackPawnCaptureMask =
        {
            0x0000000000000200, 0x0000000000000500, 0x0000000000000a00, 0x0000000000001400,
            0x0000000000002800, 0x0000000000005000, 0x000000000000a000, 0x0000000000004000,
            0x0000000000020000, 0x0000000000050000, 0x00000000000a0000, 0x0000000000140000,
            0x0000000000280000, 0x0000000000500000, 0x0000000000a00000, 0x0000000000400000,
            0x0000000002000000, 0x0000000005000000, 0x000000000a000000, 0x0000000014000000,
            0x0000000028000000, 0x0000000050000000, 0x00000000a0000000, 0x0000000040000000,
            0x0000000200000000, 0x0000000500000000, 0x0000000a00000000, 0x0000001400000000,
            0x0000002800000000, 0x0000005000000000, 0x000000a000000000, 0x0000004000000000,
            0x0000020000000000, 0x0000050000000000, 0x00000a0000000000, 0x0000140000000000,
            0x0000280000000000, 0x0000500000000000, 0x0000a00000000000, 0x0000400000000000,
            0x0002000000000000, 0x0005000000000000, 0x000a000000000000, 0x0014000000000000,
            0x0028000000000000, 0x0050000000000000, 0x00a0000000000000, 0x0040000000000000,
            0x0200000000000000, 0x0500000000000000, 0x0a00000000000000, 0x1400000000000000,
            0x2800000000000000, 0x5000000000000000, 0xa000000000000000, 0x4000000000000000,
            0x0000000000000000, 0x0000000000000000, 0x0000000000000000, 0x0000000000000000,
            0x0000000000000000, 0x0000000000000000, 0x0000000000000000, 0x0000000000000000
        };

        public static readonly ulong[] WhitePawnCaptureMask =
        {
            0x0000000000000000, 0x0000000000000000, 0x0000000000000000, 0x0000000000000000, 
            0x0000000000000000, 0x0000000000000000, 0x0000000000000000, 0x0000000000000000, 
            0x0000000000000002, 0x0000000000000005, 0x000000000000000a, 0x0000000000000014, 
            0x0000000000000028, 0x0000000000000050, 0x00000000000000a0, 0x0000000000000040, 
            0x0000000000000200, 0x0000000000000500, 0x0000000000000a00, 0x0000000000001400,
            0x0000000000002800, 0x0000000000005000, 0x000000000000a000, 0x0000000000004000, 
            0x0000000000020000, 0x0000000000050000, 0x00000000000a0000, 0x0000000000140000, 
            0x0000000000280000, 0x0000000000500000, 0x0000000000a00000, 0x0000000000400000, 
            0x0000000002000000, 0x0000000005000000, 0x000000000a000000, 0x0000000014000000, 
            0x0000000028000000, 0x0000000050000000, 0x00000000a0000000, 0x0000000040000000, 
            0x0000000200000000, 0x0000000500000000, 0x0000000a00000000, 0x0000001400000000, 
            0x0000002800000000, 0x0000005000000000, 0x000000a000000000, 0x0000004000000000, 
            0x0000020000000000, 0x0000050000000000, 0x00000a0000000000, 0x0000140000000000, 
            0x0000280000000000, 0x0000500000000000, 0x0000a00000000000, 0x0000400000000000, 
            0x0002000000000000, 0x0005000000000000, 0x000a000000000000, 0x0014000000000000, 
            0x0028000000000000, 0x0050000000000000, 0x00a0000000000000, 0x0040000000000000
        };
    }
}
