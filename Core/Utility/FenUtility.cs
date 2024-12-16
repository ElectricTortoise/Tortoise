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
        
        static Dictionary<char, Piece> pieceTypeFromSymbol = new Dictionary<char, Piece>()
        {
            ['k'] = Piece.King,
            ['p'] = Piece.Pawn,
            ['n'] = Piece.Knight,
            ['b'] = Piece.Bishop,
            ['r'] = Piece.Rook,
            ['q'] = Piece.Queen
        };

        public static PositionInfo FenStringParser(string fenString)
        {
            PositionInfo loadedPositionInfo = new PositionInfo();

            string[] sections = fenString.Split(" ");


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
                        loadedPositionInfo.squares[row, file] = pieceColor | pieceType;
                        file += 1;
                    }
                }
            }

            //Side to move
            loadedPositionInfo.whiteToMove = (sections[1] == "w");

            //Castling rights
            loadedPositionInfo.whiteKingCastle = sections[2].Contains("K");
            loadedPositionInfo.whiteQueenCastle = sections[2].Contains("Q");
            loadedPositionInfo.blackKingCastle = sections[2].Contains("k");
            loadedPositionInfo.blackQueenCastle = sections[2].Contains("q");

            //En Passant

            
            //Half move counter


            //Full move counter



            return loadedPositionInfo;

        }

        
    }

    public class PositionInfo
    {
        public Piece[,] squares;
        public bool whiteToMove;
        public bool whiteKingCastle;
        public bool whiteQueenCastle;
        public bool blackKingCastle;
        public bool blackQueenCastle;
        public int epFile;
        public int halfmoveClock;
        public int fullmoveClock;

        public PositionInfo()
        {
            squares = new Piece[8,8];
        }
    }
}
