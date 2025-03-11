using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

//yoinked from lizard

namespace TortoiseBot.Core
{
    public class TimeManager
    {
        public int MaxSearchTime;
        public int PlayerIncrement;
        public int PlayerTime;
        public int MovesToGo = SearchConstants.DefaultMovesToGo;

        public double SoftTimeLimit = -1;
        public bool HasSoftTime => SoftTimeLimit > 0;

        /// <summary>
        /// Add this amount of milliseconds to the total search time when checking if the
        /// search should stop, in case the move overhead is very low and the UCI expects
        /// the search to stop very quickly after our time expires.
        /// </summary>
        public const int TimerBuffer = 50;

        /// <summary>
        /// If we got a "movetime" command, we use a smaller buffer to bring the time we actually search
        /// much closer to the requested time.
        /// </summary>
        private const int MoveTimeBuffer = 5;

        /// <summary>
        /// Set to true if the go command has the "movetime" parameter.
        /// </summary>
        public bool HasMoveTime = false;

        public TimeManager()
        {
            PlayerIncrement = 0;
            PlayerTime = SearchConstants.MaxSearchTime;
        }

        public static Stopwatch TotalSearchTime = new Stopwatch();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CheckTime()
        {
            double currentTime = TotalSearchTime.ElapsedMilliseconds;

            if (currentTime > (MaxSearchTime - (HasMoveTime ? MoveTimeBuffer : TimerBuffer)))
            {
                //  Stop if we are close to going over the max time
                return true;
            }

            return false;
        }

        /// <summary>
        /// Makes move time if it wasn't given in uci
        /// </summary>
        public void MakeMoveTime()
        {
            int newSearchTime = (PlayerIncrement / 2) + (PlayerTime / 20);

            newSearchTime = Math.Min(newSearchTime, PlayerTime);

            MaxSearchTime = newSearchTime;
        }
    }
}
