using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TortoiseBot.Core.Utility;

namespace TortoiseBot.Core.Board
{
    public class Bitboard
    {
        public ulong[] pieceBitboard;
        public ulong[] colourBitboard;
        public ulong allPieceBitboard;
        public BoardState boardState;

        public Bitboard()
        {
            pieceBitboard = new ulong[6];
            colourBitboard = new ulong[2];
            allPieceBitboard = 0UL;
            boardState = new BoardState();
        }

        public void AddPiece(int pieceType, int pieceColour, int index)
        {
            if (!BoardUtility.IsBitOnBitboard(allPieceBitboard, index))
            {
                allPieceBitboard |= 1UL << index;
                pieceBitboard[pieceType] |= 1UL << index;
                colourBitboard[pieceColour] |= 1UL << index;
            }
            else
            {
                throw new CustomException("Tried to add piece to occupied square");
            }
        }

        public void RemovePiece(int pieceType, int pieceColour, int index)
        {
            if (BoardUtility.IsBitOnBitboard(colourBitboard[pieceColour], index) & BoardUtility.IsBitOnBitboard(pieceBitboard[pieceType], index))
            {
                allPieceBitboard &= ~(1UL << index);
                pieceBitboard[pieceType] &= ~(1UL << index);
                colourBitboard[pieceColour] &= ~(1UL << index);
            }
            else
            {
                throw new CustomException("No piece to remove or tried to remove wrong piece");
            }
        }

        public void LoadStartingPosition()
        {
            LoadPosition(FenUtility.StartFen);
        }

        public void LoadPosition(string fen)
        {
            FenUtility.ParseFenString(this, fen);
        }
    }
}
