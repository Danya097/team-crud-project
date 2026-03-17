using Chess_Board.Models;
using System.Collections.Generic;

namespace Chess_Board.Engine
{
    public static class MoveGenerator
    {
        public static List<Move> GetMoves(Piece piece, int r, int c, Piece[,] board)
        {
            List<Move> moves = new List<Move>();

            if (piece.Type == PieceType.Pawn)
            {
                int dir = piece.Color == PieceColor.White ? -1 : 1;

                int newRow = r + dir;

                if (newRow >= 0 && newRow < 8)
                {
                    if (board[newRow, c] == null)
                        moves.Add(new Move(r, c, newRow, c));
                }
            }

            if (piece.Type == PieceType.Rook)
            {
                for (int i = r + 1; i < 8; i++)
                {
                    if (board[i, c] == null)
                        moves.Add(new Move(r, c, i, c));
                    else
                        break;
                }
            }

            return moves;
        }
    }
}
