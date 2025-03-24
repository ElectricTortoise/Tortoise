using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Tortoise.Core
{
    public class MoveOrderer : IComparer<ushort>
    {
        public int Compare(ushort move1, ushort move2)
        {
            if (((move1 >> 12) == (int)MoveFlag.Capture) && ((move2 >> 12) != (int)MoveFlag.Capture)) { return -1; }
            else if (((move1 >> 12) != (int)MoveFlag.Capture) && ((move2 >> 12) == (int)MoveFlag.Capture)) { return 1; }
            else { return 0; }
        }
    }
}
