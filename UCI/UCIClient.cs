using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tortoise.Core;

//yoinked from lizard
namespace Tortoise.UCI
{
    public unsafe class UCIClient
    {
        private Board board;
        private SearchInformation info;

        public UCIClient()
        {
            board = new Board();
            info = new SearchInformation(board);
        }

        public void Run()
        {
            Console.WriteLine("id name CarrotzDestroyer");
            Console.WriteLine("id author ElectricTortoise");

            Console.WriteLine("option name Threads type spin default 1 min 1 max 1");
            Console.WriteLine("option name Hash type spin default 1 min 1 max 1");

            Console.WriteLine("uciok");
            InputLoop();
        }

        private static string[] ReceiveString(out string cmd)
        {
            string input = Console.ReadLine();
            if (input == null || input.Length == 0)
            {
                cmd = ":(";
                return Array.Empty<string>();
            }

            string[] splits = input.Split(" ");
            cmd = splits[0].ToLower();
            string[] param = splits.ToList().GetRange(1, splits.Length - 1).ToArray();

            return param;
        }

        private void InputLoop()
        {
            while (true)
            {
                string[] param = ReceiveString(out string cmd);

                if (cmd == "quit")
                {
                    Environment.Exit(0);
                }
                else if (cmd == "isready")
                {
                    Console.WriteLine("readyok");
                }
                else if (cmd == "ucinewgame")
                {
                    Search.RepetitionHistory.Clear(); 
                }
                else if (cmd == "position")
                {
                    ParsePositionCommand(param);
                }
                else if (cmd == "go")
                {
                    HandleGo(param);
                }
            }
        }

        //input ::= [ "fen" <fenstring> | "startpos" ] "moves" <move1> ... <movei>
        private void ParsePositionCommand(string[] input)
        {
            board.Clear();
            if (input[0] == "fen")
            {
                input = input.Skip(1).ToArray();
                string fen = string.Join(" ", input.TakeWhile(x => x != "moves"));
                board.LoadPosition(fen);
            }

            else if (input[0] == "startpos")
            {
                board.LoadStartingPosition();
            }

            Search.RepetitionHistory.Clear();
            Search.RepetitionHistory.Push(board.zobristHash);

            var moves = input.SkipWhile(x => x != "moves").Skip(1).ToArray();

            for (int i = 0; i < moves.Length; i++)
            {
                int startSquare = Utility.GetSquareIndex(moves[i].Substring(0, 2));
                int finalSquare = Utility.GetSquareIndex(moves[i].Substring(2, 2));
                string promotingPiece = moves[i].Substring(4);

                MoveFlag flag = new MoveFlag();

                flag = MoveFlag.Default;

                if (Utility.IsBitOnBitboard(board.allPieceBitboard, finalSquare)) 
                {
                    flag = MoveFlag.Capture; 
                } //capture


                if (promotingPiece != "")
                {
                    flag |= promotingPiece switch
                    {
                        "q" => MoveFlag.PromoteToQueen,
                        "r" => MoveFlag.PromoteToRook,
                        "b" => MoveFlag.PromoteToBishop,
                        "n" => MoveFlag.PromoteToKnight,
                        _ => MoveFlag.Default
                    };
                } // promotion

                if ((startSquare == 60 && board.kingSquares[Colour.White] == 60) || (startSquare == 4 && board.kingSquares[Colour.Black] == 4)) 
                {
                    bool kingSide = board.boardState.whiteToMove ? board.boardState.whiteKingCastle : board.boardState.blackKingCastle;
                    bool queenSide = board.boardState.whiteToMove ? board.boardState.whiteQueenCastle : board.boardState.blackQueenCastle;

                    if (kingSide && (finalSquare == startSquare + 2))
                    {
                        flag = MoveFlag.Castle;
                    }

                    if (queenSide && (finalSquare == startSquare - 2))
                    {
                        flag = MoveFlag.Castle;
                    }
                } //castling


                if (board.pieceTypesBitboard[startSquare] == PieceType.Pawn)
                {
                    if ((moves[i].Substring(1, 1) == "2" && moves[i].Substring(3, 1) == "4") || (moves[i].Substring(1, 1) == "7" && moves[i].Substring(3, 1) == "5"))
                    {
                        flag = MoveFlag.DoublePush;
                    }

                    if ((moves[i].Substring(0, 1) != moves[i].Substring(2, 1)) && !Utility.IsBitOnBitboard(board.allPieceBitboard, finalSquare))
                    {
                        flag = MoveFlag.EnPassant | MoveFlag.Capture;
                    }
                } //funny pawn moves (EP and double push)


                Move move = new Move((byte)startSquare, (byte)finalSquare, flag);
                board.MakeMove(move); // flips whose turn it is to move

                int kingSquare = board.kingSquares[board.boardState.GetOpponentColour()]; // original player's king square
                if (MoveGenUtility.IsInCheck(board, kingSquare, board.boardState.GetColourToMove())) // ColourToMove() is actually opponent's colour
                {
                    continue;
                }

                Search.RepetitionHistory.Push(board.zobristHash);
            }

            info = new SearchInformation(board);
        }

        //param :=  [("searchmoves" <move_list> | "ponder" | "wtime" <int> "btime" <int> | "winc" <int> "binc" <int> | "movestogo" <int> | "depth" <int> | "nodes" <int> | "mate" <int> | "movetime" <int> | "infinite")]
        private void HandleGo(string[] param)
        {
            if (info.SearchActive)
            {
                return;
            }

            bool makeTime = ParseGo(param, ref info);

            //  If we weren't told to search for a specific time (no "movetime" and not "infinite"),
            //  then we make one ourselves
            if (makeTime)
            {
                info.TimeManager.MakeMoveTime();
            }

            Search.StartSearch(this.board, ref this.info);
        }

        private bool ParseGo(string[] param, ref SearchInformation info)
        {
            TimeManager tm = info.TimeManager;
            bool makeTime = false;

            //  Assume that we can search infinitely, and let the UCI's "go" parameters constrain us accordingly.
            info.NodeLimit = SearchConstants.MaxSearchNodes;
            tm.MaxSearchTime = SearchConstants.MaxSearchTime;
            info.DepthLimit = SearchConstants.MaxDepth;

            int stm = info.board.boardState.GetColourToMove();

            for (int i = 0; i < param.Length - 1; i++)
            {
                if (param[i] == "movetime" && int.TryParse(param[i + 1], out int reqMovetime))
                {
                    info.SetMoveTime(reqMovetime);
                }
                else if (param[i] == "depth" && int.TryParse(param[i + 1], out int reqDepth))
                {
                    info.DepthLimit = reqDepth;
                }
                else if (param[i] == "nodes" && ulong.TryParse(param[i + 1], out ulong reqNodes))
                {
                    info.NodeLimit = reqNodes;
                }
                else if (param[i] == "movestogo" && int.TryParse(param[i + 1], out int reqMovestogo))
                {
                    tm.MovesToGo = reqMovestogo;
                }
                else if (((param[i] == "wtime" && stm == Colour.White) || (param[i] == "btime" && stm == Colour.Black)) && int.TryParse(param[i + 1], out int reqPlayerTime))
                {
                    tm.PlayerTime = reqPlayerTime;
                    makeTime = true;
                }
                else if (((param[i] == "winc" && stm == Colour.White) || (param[i] == "binc" && stm == Colour.Black)) && int.TryParse(param[i + 1], out int reqPlayerIncrement))
                {
                    tm.PlayerIncrement = reqPlayerIncrement;
                    makeTime = true;
                }
                else if (param[i] == "stop")
                {
                    info.NodeLimit = SearchConstants.MaxSearchNodes;
                    tm.MaxSearchTime = SearchConstants.MaxSearchTime;
                    info.DepthLimit = SearchConstants.MaxDepth;
                }
                else if (param[i] == "infinite")
                {
                    info.NodeLimit = SearchConstants.MaxSearchNodes;
                    tm.MaxSearchTime = SearchConstants.MaxSearchTime;
                    info.DepthLimit = SearchConstants.MaxDepth;
                }
            }
            return makeTime;
        }
    }
}
