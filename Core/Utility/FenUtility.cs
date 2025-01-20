using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TortoiseBot.Core.Board;

namespace TortoiseBot.Core.Utility
{

    public class FenUtility
    {
        public const string StartFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

        public static void ParseFenString(Bitboard bitboard, string fen)
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
                        bitboard.AddPiece(pieceType, pieceColour, row * 8 + file);
                        file += 1;
                    }
                }
            }

            //Side to move
            bitboard.boardState.whiteToMove = (sections[1] == "w");

            //Castling rights
            bitboard.boardState.whiteKingCastle = sections[2].Contains("K");
            bitboard.boardState.whiteQueenCastle = sections[2].Contains("Q");
            bitboard.boardState.blackKingCastle = sections[2].Contains("k");
            bitboard.boardState.blackQueenCastle = sections[2].Contains("q");

            //En Passant
            if (sections[3] != "-")
            {
                bitboard.boardState.epTargetSquare = BoardUtility.GetSquareIndex(sections[3].ToString());
            }

            //Half move counter
            bitboard.boardState.halfmoveClock = int.Parse(sections[4]);

            //Full move counter
            bitboard.boardState.fullmoveClock = int.Parse(sections[5]);
        }
    }
}
