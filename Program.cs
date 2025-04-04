﻿using System;
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

            //board.LoadPosition("r3k2r/p1ppqpb1/bnN1p1p1/3P4/1p2n1Q1/P1N5/1PPBBPpP/R3K2R b KQkq - 0 3");

            //MoveGen.GenAllMoves(board, ref moveList);
            //MoveOrderer.OrderMoves(ref board, ref moveList);
            //SearchInformation searchInformation = new SearchInformation(board, 5);

            //Search.StartSearch(board, ref searchInformation);

            //for (int i = 0; i < moveList.Length; i++)
            //{
            //    Console.WriteLine($"{Utility.MoveToString(new Move(moveList.Moves[i]))}, {new Move(moveList.Moves[i]).flag}");
            //}

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
