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
                    SearchBench.Go(4);
                    Environment.Exit(0);
                }
            }
            
            Board board = new Board();
            board.LoadPosition("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1");
            MoveList moveList = new MoveList();
            MoveGen.GenAllMoves(board, ref moveList);

            for (int i = 0; i < moveList.Length; i++) 
            {
                Console.WriteLine($"{Utility.MoveToString(new Move(moveList.Moves[i]))}, {new Move(moveList.Moves[i]).flag}");
            }
            Console.WriteLine();
            Console.WriteLine();

            moveList.OrderMoves();

            for (int i = 0; i < moveList.Length; i++)
            {
                Console.WriteLine($"{Utility.MoveToString(new Move(moveList.Moves[i]))}, {new Move(moveList.Moves[i]).flag}");
            }

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
