using TortoiseBot.Core.MoveGeneration;
using TortoiseBot.Core.Utility;

namespace TortoiseBot.Core.Board
{

    public unsafe struct Board
    {
        public fixed ulong pieceBitboard[6];
        public fixed ulong colourBitboard[2];
        public fixed int pieceTypesBitboard[64];
        public fixed int kingSquares[2];
        public ulong allPieceBitboard;
        public BoardState boardState;

        public Board()
        {
            for (int i = 0; i < 64; i++) {pieceTypesBitboard[i] = -1;}
            allPieceBitboard = 0UL;

            boardState = new BoardState();
        }

        public void MakeMove(Move move)
        {
            int myPieceColour = this.boardState.whiteToMove ? Colour.White : Colour.Black;
            int opponentPieceColour = this.boardState.whiteToMove ? Colour.Black : Colour.White;
            int kingSquare = kingSquares[myPieceColour];
            int myPieceType = pieceTypesBitboard[move.StartSquare];

            if (myPieceType == PieceType.King)
            {
                kingSquare = move.FinalSquare;
                kingSquares[myPieceColour] = kingSquare;
            }

            int targetPieceType;

            MoveFlag flag = move.flag;
            RemovePiece(myPieceType, myPieceColour, move.StartSquare);
            this.boardState.epTargetSquare = 0;

            switch (flag)
            {
                case MoveFlag.Default:
                    AddPiece(myPieceType, myPieceColour, move.FinalSquare);
                    break;

                case MoveFlag.Capture:
                    targetPieceType = pieceTypesBitboard[move.FinalSquare];
                    RemovePiece(targetPieceType, opponentPieceColour, move.FinalSquare);
                    AddPiece(myPieceType, myPieceColour, move.FinalSquare);
                    break;

                case MoveFlag.DoublePush:
                    this.boardState.epTargetSquare = this.boardState.whiteToMove ? move.FinalSquare + 8 : move.FinalSquare - 8;
                    AddPiece(myPieceType, myPieceColour, move.FinalSquare);
                    break;

                case MoveFlag.EnPassant | MoveFlag.Capture:
                    int epPieceSquare = this.boardState.whiteToMove ? move.FinalSquare + 8 : move.FinalSquare - 8;
                    RemovePiece(PieceType.Pawn, opponentPieceColour, epPieceSquare);
                    AddPiece(myPieceType, myPieceColour, move.FinalSquare);
                    break;

                case MoveFlag.Castle:
                    AddPiece(myPieceType, myPieceColour, move.FinalSquare);
                    switch (move.FinalSquare)
                    {
                        case 6:
                            RemovePiece(PieceType.Rook, Colour.Black, 7);
                            AddPiece(PieceType.Rook, Colour.Black, 5);
                            break;
                        case 2:
                            RemovePiece(PieceType.Rook, Colour.Black, 0);
                            AddPiece(PieceType.Rook, Colour.Black, 3);
                            break;
                        case 62:
                            RemovePiece(PieceType.Rook, Colour.White, 63);
                            AddPiece(PieceType.Rook, Colour.White, 61);
                            break;
                        case 58:
                            RemovePiece(PieceType.Rook, Colour.White, 56);
                            AddPiece(PieceType.Rook, Colour.White, 59);
                            break;
                    }
                    break;

                case MoveFlag.PromoteToQueen:
                case MoveFlag.PromoteToBishop:
                case MoveFlag.PromoteToKnight:
                case MoveFlag.PromoteToRook:
                case MoveFlag.PromoteToBishop | MoveFlag.Capture:
                case MoveFlag.PromoteToKnight | MoveFlag.Capture:
                case MoveFlag.PromoteToRook | MoveFlag.Capture:
                case MoveFlag.PromoteToQueen | MoveFlag.Capture:
                    if ((flag & MoveFlag.Capture) != 0)
                    {
                        targetPieceType = pieceTypesBitboard[move.FinalSquare];
                        RemovePiece(targetPieceType, opponentPieceColour, move.FinalSquare);
                    }

                    int promotedPiece = ((int)flag & 0b11) + 1;
                    AddPiece(promotedPiece, myPieceColour, move.FinalSquare);
                    break;

            }


            if (pieceTypesBitboard[60] != PieceType.King) { this.boardState.ToggleWhiteCastling(); } ;
            if (pieceTypesBitboard[63] != PieceType.Rook) { this.boardState.ToggleWhiteKingCastling(); };
            if (pieceTypesBitboard[56] != PieceType.Rook) { this.boardState.ToggleWhiteQueenCastling(); };
            if (pieceTypesBitboard[4] != PieceType.King) { this.boardState.ToggleBlackCastling(); };
            if (pieceTypesBitboard[7] != PieceType.Rook) { this.boardState.ToggleBlackKingCastling(); };
            if (pieceTypesBitboard[0] != PieceType.Rook) { this.boardState.ToggleBlackQueenCastling(); };


            this.boardState.whiteToMove = !this.boardState.whiteToMove;
        }

        public void AddPiece(int pieceType, int pieceColour, int square)
        {
            if (!BoardUtility.IsBitOnBitboard(allPieceBitboard, square))
            {
                allPieceBitboard |= 1UL << square;
                pieceBitboard[pieceType] |= 1UL << square;
                pieceTypesBitboard[square] = pieceType;
                colourBitboard[pieceColour] |= 1UL << square;
            }
            else
            {
                throw new CustomException("Tried to add piece to occupied square");
            }
        }

        public void RemovePiece(int pieceType, int pieceColour, int square)
        {
            if (BoardUtility.IsBitOnBitboard(colourBitboard[pieceColour], square) && BoardUtility.IsBitOnBitboard(pieceBitboard[pieceType], square) && (pieceTypesBitboard[square] == pieceType))
            {
                allPieceBitboard &= ~(1UL << square);
                pieceBitboard[pieceType] &= ~(1UL << square);
                colourBitboard[pieceColour] &= ~(1UL << square);
                pieceTypesBitboard[square] = -1;
            }
            else
            {
                if (pieceTypesBitboard[square] == -1)
                {
                    throw new CustomException("No piece to remove");
                }
                if (!BoardUtility.IsBitOnBitboard(colourBitboard[pieceColour], square))
                {
                    throw new CustomException("Tried to remove wrong colour");
                }
                if (!BoardUtility.IsBitOnBitboard(pieceBitboard[pieceType], square))
                {
                    throw new CustomException("Tried to remove wrong piece");
                }
            }
        }

        public void LoadStartingPosition()
        {
            LoadPosition(FenUtility.StartFen);
        }

        public void LoadPosition(string fen)
        {
            FenUtility.ParseFenString(ref this, fen);
        }

        public void Clear()
        {
            this = new Board();
        }
    }
}
