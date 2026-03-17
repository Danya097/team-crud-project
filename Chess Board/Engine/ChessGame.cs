using Chess_Board.Models;

namespace Chess_Board.Engine
{
    public class ChessGame
    {
        public Piece[,] Board = new Piece[8, 8];

        public ChessGame()
        {
            SetupBoard();
        }

        void SetupBoard()
        {
            for (int i = 0; i < 8; i++)
            {
                Board[1, i] = new Piece(PieceType.Pawn, PieceColor.Black);
                Board[6, i] = new Piece(PieceType.Pawn, PieceColor.White);
            }

            Board[0, 0] = new Piece(PieceType.Rook, PieceColor.Black);
            Board[0, 7] = new Piece(PieceType.Rook, PieceColor.Black);

            Board[7, 0] = new Piece(PieceType.Rook, PieceColor.White);
            Board[7, 7] = new Piece(PieceType.Rook, PieceColor.White);

            Board[0, 1] = new Piece(PieceType.Knight, PieceColor.Black);
            Board[0, 6] = new Piece(PieceType.Knight, PieceColor.Black);

            Board[7, 1] = new Piece(PieceType.Knight, PieceColor.White);
            Board[7, 6] = new Piece(PieceType.Knight, PieceColor.White);

            Board[0, 2] = new Piece(PieceType.Bishop, PieceColor.Black);
            Board[0, 5] = new Piece(PieceType.Bishop, PieceColor.Black);

            Board[7, 2] = new Piece(PieceType.Bishop, PieceColor.White);
            Board[7, 5] = new Piece(PieceType.Bishop, PieceColor.White);

            Board[0, 3] = new Piece(PieceType.Queen, PieceColor.Black);
            Board[7, 3] = new Piece(PieceType.Queen, PieceColor.White);

            Board[0, 4] = new Piece(PieceType.King, PieceColor.Black);
            Board[7, 4] = new Piece(PieceType.King, PieceColor.White);
        }
    }
}
