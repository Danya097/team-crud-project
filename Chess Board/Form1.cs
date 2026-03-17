using Chess_Board.Engine;
using Chess_Board.Models;
using Chess_Board.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Chess_Board
{
    public partial class Form1 : Form
    {
        ChessGame game = new ChessGame();

        Panel[,] squares = new Panel[8, 8];

        List<Move> possibleMoves = new List<Move>();

        int selectedRow = -1;
        int selectedCol = -1;

        ChessDbHelper db = new ChessDbHelper();

        public Form1()
        {
            InitializeComponent();
            CreateBoard();
            RenderBoard();
        }

        void CreateBoard()
        {
            tableLayoutPanel1.RowCount = 8;
            tableLayoutPanel1.ColumnCount = 8;

            tableLayoutPanel1.RowStyles.Clear();
            tableLayoutPanel1.ColumnStyles.Clear();

            for (int i = 0; i < 8; i++)
            {
                tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 12.5f));
                tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12.5f));
            }

            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    Panel panel = new Panel();
                    panel.Dock = DockStyle.Fill;
                    panel.Margin = new Padding(0);

                    panel.BackColor = (r + c) % 2 == 0 ? Color.Beige : Color.SaddleBrown;

                    panel.Tag = new Point(r, c);
                    panel.Click += Square_Click;

                    squares[r, c] = panel;

                    tableLayoutPanel1.Controls.Add(panel, c, r);
                }
            }
        }

        void RenderBoard()
        {
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    squares[r, c].Controls.Clear();

                    Piece piece = game.Board[r, c];

                    if (piece != null)
                    {
                        Label label = new Label();

                        label.Dock = DockStyle.Fill;
                        label.TextAlign = ContentAlignment.MiddleCenter;
                        label.Font = new Font("Segoe UI", 32, FontStyle.Bold);

                        label.Text = GetPieceSymbol(piece);

                        squares[r, c].Controls.Add(label);
                    }
                }
            }
        }

        string GetPieceSymbol(Piece piece)
        {
            if (piece.Color == PieceColor.White)
            {
                return piece.Type switch
                {
                    PieceType.Pawn => "♙",
                    PieceType.Rook => "♖",
                    PieceType.Knight => "♘",
                    PieceType.Bishop => "♗",
                    PieceType.Queen => "♕",
                    PieceType.King => "♔",
                    _ => ""
                };
            }
            else
            {
                return piece.Type switch
                {
                    PieceType.Pawn => "♟",
                    PieceType.Rook => "♜",
                    PieceType.Knight => "♞",
                    PieceType.Bishop => "♝",
                    PieceType.Queen => "♛",
                    PieceType.King => "♚",
                    _ => ""
                };
            }
        }

        void Square_Click(object sender, EventArgs e)
        {
            Panel panel = sender as Panel;

            Point pos = (Point)panel.Tag;

            int r = pos.X;
            int c = pos.Y;

            Piece piece = game.Board[r, c];

            if (selectedRow == -1)
            {
                if (piece == null) return;

                selectedRow = r;
                selectedCol = c;

                possibleMoves = MoveGenerator.GetMoves(piece, r, c, game.Board);

                HighlightMoves();
            }
            else
            {
                TryMovePiece(r, c);
            }
        }

        void HighlightMoves()
        {
            ClearHighlights();

            foreach (var move in possibleMoves)
            {
                squares[move.ToRow, move.ToCol].BackColor = Color.LightGreen;
            }
        }

        void ClearHighlights()
        {
            for (int r = 0; r < 8; r++)
                for (int c = 0; c < 8; c++)
                    squares[r, c].BackColor =
                        (r + c) % 2 == 0 ? Color.Beige : Color.SaddleBrown;
        }

        void TryMovePiece(int r, int c)
        {
            foreach (var move in possibleMoves)
            {
                if (move.ToRow == r && move.ToCol == c)
                {
                    game.Board[move.ToRow, move.ToCol] = game.Board[move.FromRow, move.FromCol];
                    game.Board[move.FromRow, move.FromCol] = null;

                    selectedRow = -1;
                    selectedCol = -1;

                    possibleMoves.Clear();

                    ClearHighlights();

                    RenderBoard();

                    return;
                }
            }

            selectedRow = -1;
            selectedCol = -1;

            ClearHighlights();
        }

        private void btnSaveGame_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Game saved");
        }

        private void btnLoadGame_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Load game clicked");
        }

        private void btShowGames_Click(object sender, EventArgs e)
        {
            GamesListForm form = new GamesListForm();

            form.ShowDialog();
        }
    }
}
