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
                    SearchBench.Go(4);
                    Environment.Exit(0);
                }
            }

            //Board board = new Board();
            //MoveList moveList = new MoveList();

            //board.LoadPosition("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1");

            //MoveGen.GenAllMoves(board, ref moveList);
            //for (int i = 0; i < moveList.Length; i++)
            //{
            //    Console.WriteLine($"{new Move(moveList.Moves[i])}, {moveList.Moves[i]}");
            //}

            //Console.WriteLine();
            //Console.WriteLine();

            //MoveOrderer.OrderMoves(ref board, ref moveList, 0);
            //for (int i = 0; i < moveList.Length; i++)
            //{
            //    Console.WriteLine($"{new Move(moveList.Moves[i])}, {moveList.Moves[i]}");
            //}

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

                if (param[0].ToLower() == "bench")
                {
                    try
                    {
                        bool benchSuite = int.TryParse(param[1], out int depth);
                        if (benchSuite)
                        {
                            SearchBench.Go(depth);
                        }
                        else
                        {
                            if (param[1] == "fen")
                            {
                                Board board = new Board();

                                param = param.Skip(2).ToArray();
                                string fen = string.Join(" ", param);
                                board.LoadPosition(fen);

                                Console.Write("Depth: ");
                                int.TryParse(Console.ReadLine(), out depth);
                                SearchBench.SearchPosition(board, depth);
                            }
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
        }
    }
}
