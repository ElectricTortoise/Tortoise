using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Tortoise.Core  
{

    public unsafe class FenUtility
    {
        public const string StartFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

        public static void ParseFenString(ref Board board, string fen)
        {
            string[] sections = fen.Split(" ");

            //Parse position
            string[] ranks = sections[0].Split("/");

            for (int row = 0; row < 8; row++)
            {
                int file = 0;
                foreach (char c in ranks[row])
                {
                    if (char.IsDigit(c))
                    {
                        file += (int)char.GetNumericValue(c);
                    }
                    else
                    {
                        int pieceType = char.ToLower(c) switch
                        {
                            'k' => PieceType.King,
                            'p' => PieceType.Pawn,
                            'n' => PieceType.Knight,
                            'b' => PieceType.Bishop,
                            'r' => PieceType.Rook,
                            'q' => PieceType.Queen,
                            _ => -1
                        };
                        int pieceColour = (char.IsUpper(c)) ? Colour.White : Colour.Black;
                        board.AddPiece(pieceType, pieceColour, row * 8 + file);
                        file += 1;
                    }
                }
            }

            //Side to move
            board.boardState.whiteToMove = (sections[1] == "w");

            //Castling rights
            board.boardState.whiteKingCastle = sections[2].Contains("K");
            board.boardState.whiteQueenCastle = sections[2].Contains("Q");
            board.boardState.blackKingCastle = sections[2].Contains("k");
            board.boardState.blackQueenCastle = sections[2].Contains("q");

            //En Passant
            if (sections[3] != "-")
            {
                board.boardState.epTargetSquare = Utility.GetSquareIndex(sections[3].ToString());
            }

            //Half move counter
            board.boardState.halfmoveClock = int.Parse(sections[4]);

            //Full move counter
            board.boardState.fullmoveClock = int.Parse(sections[5]);

            //King Square
            board.kingSquares[Colour.Black] = BitOperations.TrailingZeroCount(board.pieceBitboard[PieceType.King] & board.colourBitboard[Colour.Black]);
            board.kingSquares[Colour.White] = BitOperations.TrailingZeroCount(board.pieceBitboard[PieceType.King] & board.colourBitboard[Colour.White]);
        }
    }
}
