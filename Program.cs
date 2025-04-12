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
            //Yoinked from Lizard and Lynx
            //Console.ReadLine() has a buffer of 256 (UTF-16?) characters
            //This allows Tortoise to read UCI strings which are longer than that
            Console.SetIn(new StreamReader(Console.OpenStandardInput(), Encoding.UTF8, false, 4096 * 4));

            if (args.Length != 0)
            {
                if (args[0] == "bench") 
                {
                    SearchBench.Go(5);
                    Environment.Exit(0);
                }
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
