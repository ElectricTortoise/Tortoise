using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TortoiseBot.Core.Utility
{
    public class MoveGenUtility
    {
        public static bool DirectionOK(int square, int direction)
        {
            if (square + direction < 0 | square + direction > 63)
            {
                return false;
            }

            int rankDistance = Math.Abs((square / 8) - ((square + direction) / 8));
            int fileDistance = Math.Abs((square % 8) - ((square + direction) % 8));

            return Math.Max(rankDistance, fileDistance) <= 1;
        }
    

        public static ulong GenerateSliderAttackMask(int square, ulong blocker, bool isRook)
        {
            ulong attacks = 0;
            int rank = square / 8;
            int file = square % 8;
            int[] directions = isRook ? new int[] { 8, -8, 1, -1 } : new int[] { 9, 7, -9, -7 };

            foreach (int direction in directions)
            {
                int currentSquare = square;

                while (true)
                {

                    if (!DirectionOK(currentSquare, direction))
                    {
                        break;
                    }

                    currentSquare += direction;
                    attacks |= 1UL << currentSquare;

                    if ((blocker & (1UL << currentSquare)) != 0)
                    {
                        break;
                    }

                }
            }
            return attacks;
        }
    }
}
