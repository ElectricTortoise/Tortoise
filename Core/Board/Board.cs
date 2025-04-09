using System.Runtime.CompilerServices;

namespace Tortoise.Core
{

    public unsafe struct Board
    {
        public fixed ulong pieceBitboard[6];
        public fixed ulong colourBitboard[2];
        public fixed int pieceTypesBitboard[64];
        public fixed int kingSquares[2];

        public ulong zobristHash;
        public ulong allPieceBitboard;
        public BoardState boardState;

        public Board()
        {
            for (int i = 0; i < 64; i++) {pieceTypesBitboard[i] = -1;}
            allPieceBitboard = 0UL;
            zobristHash = 0UL;

            boardState = new BoardState();
        }

        public void MakeMove(ushort move)
        {
            MakeMove(new Move(move));
        }

        public void MakeMove(Move move)
        {
            int myPieceColour = this.boardState.whiteToMove ? Colour.White : Colour.Black;
            int opponentPieceColour = this.boardState.whiteToMove ? Colour.Black : Colour.White;
            int kingSquare = kingSquares[myPieceColour];
            int myPieceType = pieceTypesBitboard[move.StartSquare];

            this.boardState.halfmoveClock++;

            if (myPieceType == PieceType.King)
            {
                kingSquare = move.FinalSquare;
                kingSquares[myPieceColour] = kingSquare;
            }

            if (myPieceType == PieceType.Pawn)
            {
                this.boardState.halfmoveClock = 0;
            }

            int targetPieceType;

            MoveFlag flag = move.flag;
            RemovePiece(myPieceType, myPieceColour, move.StartSquare);

            if (this.boardState.epTargetSquare != BoardConstants.NONE_SQUARE)
            {
                zobristHash ^= Zobrist.ZobristEnPassant[this.boardState.epTargetSquare % 8];
                this.boardState.epTargetSquare = BoardConstants.NONE_SQUARE;
            }


            switch (flag)
            {
                case MoveFlag.Default:
                    AddPiece(myPieceType, myPieceColour, move.FinalSquare);
                    break;

                case MoveFlag.Capture:
                    this.boardState.halfmoveClock = 0;
                    targetPieceType = pieceTypesBitboard[move.FinalSquare];
                    RemovePiece(targetPieceType, opponentPieceColour, move.FinalSquare);
                    AddPiece(myPieceType, myPieceColour, move.FinalSquare);
                    break;

                case MoveFlag.DoublePush:
                    this.boardState.epTargetSquare = this.boardState.whiteToMove ? move.FinalSquare + 8 : move.FinalSquare - 8;
                    ulong hash = Zobrist.ZobristEnPassant[this.boardState.epTargetSquare % 8];
                    zobristHash ^= Zobrist.ZobristEnPassant[this.boardState.epTargetSquare % 8];
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
                        case BoardConstants.g8:
                            RemovePiece(PieceType.Rook, Colour.Black, BoardConstants.h8);
                            AddPiece(PieceType.Rook, Colour.Black, BoardConstants.f8);
                            break;
                        case BoardConstants.c8:
                            RemovePiece(PieceType.Rook, Colour.Black, BoardConstants.a8);
                            AddPiece(PieceType.Rook, Colour.Black, BoardConstants.d8);
                            break;
                        case BoardConstants.g1:
                            RemovePiece(PieceType.Rook, Colour.White, BoardConstants.h1);
                            AddPiece(PieceType.Rook, Colour.White, BoardConstants.f1);
                            break;
                        case BoardConstants.c1:
                            RemovePiece(PieceType.Rook, Colour.White, BoardConstants.a1);
                            AddPiece(PieceType.Rook, Colour.White, BoardConstants.d1);
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


            if (this.boardState.whiteKingCastle || this.boardState.whiteQueenCastle)
            {
                if (pieceTypesBitboard[BoardConstants.e1] != PieceType.King)
                {
                    if (this.boardState.whiteKingCastle) { zobristHash ^= Zobrist.ZobristCastling[0]; }
                    if (this.boardState.whiteQueenCastle) { zobristHash ^= Zobrist.ZobristCastling[1]; }
                    this.boardState.DisableWhiteCastling();
                };
                if (pieceTypesBitboard[BoardConstants.h1] != PieceType.Rook)
                {
                    if (this.boardState.whiteKingCastle) { zobristHash ^= Zobrist.ZobristCastling[0]; }
                    this.boardState.DisableWhiteKingCastling();
                };
                if (pieceTypesBitboard[BoardConstants.a1] != PieceType.Rook)
                {
                    if (this.boardState.whiteQueenCastle) { zobristHash ^= Zobrist.ZobristCastling[1]; }
                    this.boardState.DisableWhiteQueenCastling();
                };
            }

            if (this.boardState.blackKingCastle || this.boardState.blackQueenCastle)
            {
                if (pieceTypesBitboard[BoardConstants.e8] != PieceType.King)
                {
                    if (this.boardState.blackKingCastle) { zobristHash ^= Zobrist.ZobristCastling[2]; }
                    if (this.boardState.blackQueenCastle) { zobristHash ^= Zobrist.ZobristCastling[3]; }
                    this.boardState.DisableBlackCastling();
                }
                if (pieceTypesBitboard[BoardConstants.h8] != PieceType.Rook)
                {
                    if (this.boardState.blackKingCastle) { zobristHash ^= Zobrist.ZobristCastling[2]; }
                    this.boardState.DisableBlackKingCastling();
                }
                if (pieceTypesBitboard[BoardConstants.a8] != PieceType.Rook)
                {
                    if (this.boardState.blackQueenCastle) { zobristHash ^= Zobrist.ZobristCastling[3]; }
                    this.boardState.DisableBlackQueenCastling();
                }
            }


            this.boardState.whiteToMove = !this.boardState.whiteToMove;
            zobristHash ^= Zobrist.ZobristBlackToMove;
        }

        public void AddPiece(int pieceType, int pieceColour, int square)
        {
            if (!Utility.IsBitOnBitboard(allPieceBitboard, square))
            {
                allPieceBitboard |= 1UL << square;
                pieceBitboard[pieceType] |= 1UL << square;
                pieceTypesBitboard[square] = pieceType;
                colourBitboard[pieceColour] |= 1UL << square;
                zobristHash ^= Zobrist.ZobristPieces[pieceType + (6 * pieceColour), square];
            }
            else
            {
                throw new CustomException("Tried to add piece to occupied square");
            }
        }

        public void RemovePiece(int pieceType, int pieceColour, int square)
        {
            if (Utility.IsBitOnBitboard(colourBitboard[pieceColour], square) && Utility.IsBitOnBitboard(pieceBitboard[pieceType], square) && (pieceTypesBitboard[square] == pieceType))
            {
                allPieceBitboard &= ~(1UL << square);
                pieceBitboard[pieceType] &= ~(1UL << square);
                colourBitboard[pieceColour] &= ~(1UL << square);
                pieceTypesBitboard[square] = -1;
                zobristHash ^= Zobrist.ZobristPieces[pieceType + (6 * pieceColour), square];
            }
            else
            {
                if (pieceTypesBitboard[square] == -1)
                {
                    throw new CustomException("No piece to remove");
                }
                if (!Utility.IsBitOnBitboard(colourBitboard[pieceColour], square))
                {
                    throw new CustomException("Tried to remove wrong colour");
                }
                if (!Utility.IsBitOnBitboard(pieceBitboard[pieceType], square))
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
