using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tortoise.Core;
using Tortoise.UCI;

namespace Tortoise
{
    public unsafe static class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length != 0)
            {
                if (args[0] == "bench") 
                {
                    SearchBench.Go(6);
                    Environment.Exit(0);
                }
            }
            
            //Board board = new Board();
            //board.LoadPosition("r3k2r/p2pqpb1/bn2pBp1/2pPN3/1p2P3/2N2Q2/PPP1BPpP/2KR3R b kq - 0 3");
            //MoveList moveList = new MoveList();
            //MoveGen.GenAllMoves(board, ref moveList);

            DoInputLoop();
        }

        private static void DoInputLoop()
        {
            while (true)
            {
                string input = Console.ReadLine();
                if (input == null || input.Length == 0)
                {
                    continue;
                }
                string[] param = input.Split(' ');

                if (input.ToLower() == "uci")
                {
                    UCIClient client = new UCIClient();
                    client.Run();
                }
            }
        }
    }
}
