using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tortoise.Core;
using Tortoise.UCI;

namespace Tortoise
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length != 0)
            {
                if (args[0] == "bench") 
                {
                    SearchBench.Go(4);
                    Environment.Exit(0);
                }
            }

            //Board board = new Board();
            //MoveList moveList = new MoveList();

            //board.LoadPosition("3k4/7R/8/2pP4/8/7B/3R4/3KR3 w - c6 0 2");
            //SearchInformation searchInformation = new SearchInformation(board);

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
