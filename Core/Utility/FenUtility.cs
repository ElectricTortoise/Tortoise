using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TortoiseBot.Core.MoveGeneration;

namespace TortoiseBot.Core.Utility
{

    public class FenUtility
    {
        public const string StartFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

        public static PositionInfo FenStringParser(string fen)
        {
            PositionInfo loadedPositionInfo = new PositionInfo(fen);
            return loadedPositionInfo;
        } 
    }

    public readonly struct PositionInfo
    {
        public readonly string fen;
        public readonly bool whiteToMove;
        public readonly bool whiteKingCastle;
        public readonly bool whiteQueenCastle;
        public readonly bool blackKingCastle;
        public readonly bool blackQueenCastle;
        public readonly int epTargetSquare;
        public readonly int halfmoveClock;
        public readonly int fullmoveClock;
        public readonly Bitboard bitboard;


        private static Dictionary<char, int> pieceTypeFromSymbol = new Dictionary<char, int>()
        {
            ['k'] = PieceType.King,
            ['p'] = PieceType.Pawn,
            ['n'] = PieceType.Knight,
            ['b'] = PieceType.Bishop,
            ['r'] = PieceType.Rook,
            ['q'] = PieceType.Queen
        };

        public PositionInfo(string fen)
        {
            this.fen = fen;
            bitboard = new Bitboard();

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
                        int pieceType = pieceTypeFromSymbol[char.ToLower(c)];
                        int pieceColour = (char.IsUpper(c)) ? Colour.White : Colour.Black;
                        bitboard.AddPiece(pieceType, pieceColour, row * 8 + file);
                        file += 1;
                    }
                }
            }

            //Side to move
            whiteToMove = (sections[1] == "w");

            //Castling rights
            whiteKingCastle = sections[2].Contains("K");
            whiteQueenCastle = sections[2].Contains("Q");
            blackKingCastle = sections[2].Contains("k");
            blackQueenCastle = sections[2].Contains("q");

            //En Passant
            if (sections[3] != "-")
            {
                epTargetSquare = BoardUtility.GetSquareIndex(sections[3].ToString());
            }

            //Half move counter
            halfmoveClock = int.Parse(sections[4]);

            //Full move counter
            fullmoveClock = int.Parse(sections[5]);
        }
    }
}
