using System;
using System.Drawing;
using System.Windows.Forms;

namespace Chess_Board
{
    public partial class Form1 : Form
    {
        private Panel[,] squares = new Panel[8, 8];

        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;   // attach load event
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CreateBoard();
            SetupPieces();
        }

        private void CreateBoard()
        {
            tableLayoutPanel1.Controls.Clear();

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    Panel p = new Panel();
                    p.Dock = DockStyle.Fill;
                    p.Margin = new Padding(0);

                    bool isLight = (row + col) % 2 == 0;
                    p.BackColor = isLight ? Color.Beige : Color.SaddleBrown;

                    p.Tag = ToChessCoord(row, col);
                    p.Click += Square_Click;

                    squares[row, col] = p;
                    tableLayoutPanel1.Controls.Add(p, col, row);


                }
            }
        }

        private string ToChessCoord(int row, int col)
        {
            char file = (char)('A' + col); // A–H
            int rank = 8 - row;            // 8–1
            return $"{file}{rank}";
        }

        private void Square_Click(object sender, EventArgs e)
        {
            Panel clicked = (Panel)sender;
            string coord = (string)clicked.Tag;

            MessageBox.Show($"You clicked {coord}");
        }

        private void AddPiece(int row, int col, string symbol)
        {
            Label piece = new Label();
            piece.Text = symbol;
            piece.Font = new Font("Segoe UI", 32, FontStyle.Bold);
            piece.ForeColor = Color.Black;
            piece.Dock = DockStyle.Fill;
            piece.TextAlign = ContentAlignment.MiddleCenter;
            piece.BackColor = Color.Transparent;

            squares[row, col].Controls.Add(piece);
        }

        private void SetupPieces()
        {
            // Black pieces
            AddPiece(0, 0, "♜");
            AddPiece(0, 1, "♞");
            AddPiece(0, 2, "♝");
            AddPiece(0, 3, "♛");
            AddPiece(0, 4, "♚");
            AddPiece(0, 5, "♝");
            AddPiece(0, 6, "♞");
            AddPiece(0, 7, "♜");

            for (int col = 0; col < 8; col++)
                AddPiece(1, col, "♟");

            // White pieces
            AddPiece(7, 0, "♖");
            AddPiece(7, 1, "♘");
            AddPiece(7, 2, "♗");
            AddPiece(7, 3, "♕");
            AddPiece(7, 4, "♔");
            AddPiece(7, 5, "♗");
            AddPiece(7, 6, "♘");
            AddPiece(7, 7, "♖");

            for (int col = 0; col < 8; col++)
                AddPiece(6, col, "♙");
        }
    }
}