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

        public static PositionInfo FenStringParser(string fen)
        {
            PositionInfo loadedPositionInfo = new PositionInfo(fen);
            return loadedPositionInfo;
        } 
    }

    public readonly struct PositionInfo
    {
        public readonly string fen;
        public readonly Piece[] squares;
        public readonly bool whiteToMove;
        public readonly bool whiteKingCastle;
        public readonly bool whiteQueenCastle;
        public readonly bool blackKingCastle;
        public readonly bool blackQueenCastle;
        public readonly int epTargetSquare;
        public readonly int halfmoveClock;
        public readonly int fullmoveClock;

        private static Dictionary<char, Piece> pieceTypeFromSymbol = new Dictionary<char, Piece>()
        {
            ['k'] = Piece.King,
            ['p'] = Piece.Pawn,
            ['n'] = Piece.Knight,
            ['b'] = Piece.Bishop,
            ['r'] = Piece.Rook,
            ['q'] = Piece.Queen
        };

        public PositionInfo(string fen)
        {
            this.fen = fen;
            squares = new Piece[64];

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
                        Piece pieceColor = (char.IsUpper(c)) ? Piece.White : Piece.Black;
                        Piece pieceType = pieceTypeFromSymbol[char.ToLower(c)];
                        squares[row * 8 + file] = pieceColor | pieceType;
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
