using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tortoise.Core;
using Tortoise.UCI;

namespace Tortoise
{
    public static unsafe class Program
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
            //MoveList moveList = new MoveList();

            //board.LoadPosition("4k2r/3np1pp/3n4/8/1Q6/B5P1/p3P1P1/2NQRKR1 b k - 0 1");

            //SearchInformation searchInformation = new SearchInformation(board, 5);

            //Search.StartSearch(board, ref searchInformation);

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
