using System;
using System.Drawing;
using System.Windows.Forms;
using Chess_Board.Data;

namespace Chess_Board
{
    public partial class Form1 : Form
    {
        private Panel[,] squares = new Panel[8, 8];
        private Label draggedPiece = null;
        private Point mouseOffset;
        private TextBox fenTextBox;
        private Color highlightColor = Color.LightGreen;

        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CreateBoard();
            SetupPieces();
            CreateFENPanel();
            UpdateFEN();
        }

        private void CreateBoard()
        {
            tableLayoutPanel1.Controls.Clear();
            tableLayoutPanel1.RowCount = 8;
            tableLayoutPanel1.ColumnCount = 8;

            tableLayoutPanel1.RowStyles.Clear();
            tableLayoutPanel1.ColumnStyles.Clear();

            for (int i = 0; i < 8; i++)
            {
                tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 12.5f));
                tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12.5f));
            }

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    Panel p = new Panel
                    {
                        Dock = DockStyle.Fill,
                        Margin = new Padding(0),
                        BackColor = (row + col) % 2 == 0 ? Color.Beige : Color.SaddleBrown,
                        Tag = new Point(row, col),
                        AllowDrop = true
                    };

                    p.DragEnter += Square_DragEnter;
                    p.DragDrop += Square_DragDrop;
                    p.Click += Square_Click;

                    squares[row, col] = p;
                    tableLayoutPanel1.Controls.Add(p, col, row);
                }
            }
        }

        private void CreateFENPanel()
        {
            fenTextBox = new TextBox
            {
                Dock = DockStyle.Top,
                ReadOnly = true,
                Font = new Font("Consolas", 12),
                Height = 30
            };

            this.Controls.Add(fenTextBox);
            fenTextBox.BringToFront();

            Button copyBtn = new Button
            {
                Text = "Copy FEN",
                Dock = DockStyle.Top,
                Height = 30
            };

            copyBtn.Click += (s, e) =>
            {
                Clipboard.SetText(fenTextBox.Text);
                MessageBox.Show("FEN copied!");
            };

            this.Controls.Add(copyBtn);
            copyBtn.BringToFront();
        }

        private void AddPiece(int row, int col, string symbol)
        {
            Label piece = new Label
            {
                Text = symbol,
                Font = new Font("Segoe UI", 32, FontStyle.Bold),
                ForeColor = Color.Black,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            piece.MouseDown += Piece_MouseDown;
            piece.MouseMove += Piece_MouseMove;

            squares[row, col].Controls.Add(piece);
        }

        private void SetupPieces()
        {
            // Black pieces
            AddPiece(0, 0, "♜"); AddPiece(0, 1, "♞"); AddPiece(0, 2, "♝"); AddPiece(0, 3, "♛");
            AddPiece(0, 4, "♚"); AddPiece(0, 5, "♝"); AddPiece(0, 6, "♞"); AddPiece(0, 7, "♜");
            for (int c = 0; c < 8; c++) AddPiece(1, c, "♟");

            // White pieces
            AddPiece(7, 0, "♖"); AddPiece(7, 1, "♘"); AddPiece(7, 2, "♗"); AddPiece(7, 3, "♕");
            AddPiece(7, 4, "♔"); AddPiece(7, 5, "♗"); AddPiece(7, 6, "♘"); AddPiece(7, 7, "♖");
            for (int c = 0; c < 8; c++) AddPiece(6, c, "♙");
        }

        // ---------------- Drag & Drop ----------------

        private void Piece_MouseDown(object sender, MouseEventArgs e)
        {
            draggedPiece = sender as Label;
            mouseOffset = e.Location;
            HighlightMoves(draggedPiece);
        }

        private void Piece_MouseMove(object sender, MouseEventArgs e)
        {
            if (draggedPiece != null && e.Button == MouseButtons.Left)
            {
                draggedPiece.DoDragDrop(draggedPiece, DragDropEffects.Move);
            }
        }

        private void Square_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(Label)))
                e.Effect = DragDropEffects.Move;
            else
                e.Effect = DragDropEffects.None;
        }

        private void Square_DragDrop(object sender, DragEventArgs e)
        {
            Panel targetSquare = sender as Panel;
            if (draggedPiece == null || targetSquare == null) return;

            if (targetSquare.BackColor != highlightColor)
            {
                ClearHighlights();
                draggedPiece = null;
                return;
            }

            Panel parent = draggedPiece.Parent as Panel;
            parent.Controls.Clear();
            targetSquare.Controls.Clear();
            targetSquare.Controls.Add(draggedPiece);

            draggedPiece = null;
            ClearHighlights();
            UpdateFEN();
        }

        private void Square_Click(object sender, EventArgs e)
        {
            ClearHighlights();
        }

        // ---------------- Highlight moves ----------------

        private void HighlightMoves(Label piece)
        {
            ClearHighlights();
            if (piece == null) return;

            Panel current = piece.Parent as Panel;
            Point pos = (Point)current.Tag;
            string symbol = piece.Text;

            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    if (CanMove(symbol, pos, new Point(r, c)))
                    {
                        squares[r, c].BackColor = highlightColor;
                    }
                }
            }
        }

        private bool CanMove(string symbol, Point from, Point to)
        {
            int dr = to.X - from.X;
            int dc = to.Y - from.Y;

            if (squares[to.X, to.Y].Controls.Count > 0)
            {
                Label target = (Label)squares[to.X, to.Y].Controls[0];
                if (char.IsUpper(symbol[0]) == char.IsUpper(target.Text[0]))
                    return false;
            }

            switch (symbol)
            {
                case "♙": return dr == -1 && dc == 0 || dr == -1 && Math.Abs(dc) == 1;
                case "♟": return dr == 1 && dc == 0 || dr == 1 && Math.Abs(dc) == 1;
                case "♖":
                case "♜": return dr == 0 || dc == 0;
                case "♘":
                case "♞": return (Math.Abs(dr) == 2 && Math.Abs(dc) == 1) || (Math.Abs(dr) == 1 && Math.Abs(dc) == 2);
                case "♗":
                case "♝": return Math.Abs(dr) == Math.Abs(dc);
                case "♕":
                case "♛": return dr == 0 || dc == 0 || Math.Abs(dr) == Math.Abs(dc); // ← исправлено
                case "♔":
                case "♚": return Math.Abs(dr) <= 1 && Math.Abs(dc) <= 1;
            }

            return false;
        }

        private void ClearHighlights()
        {
            for (int r = 0; r < 8; r++)
                for (int c = 0; c < 8; c++)
                    squares[r, c].BackColor =
                        (r + c) % 2 == 0 ? Color.Beige : Color.SaddleBrown;
        }

        // ---------------- FEN ----------------

        private void UpdateFEN()
        {
            fenTextBox.Text = GenerateFEN();
        }

        private string GenerateFEN()
        {
            string fen = "";

            for (int row = 0; row < 8; row++)
            {
                int empty = 0;

                for (int col = 0; col < 8; col++)
                {
                    if (squares[row, col].Controls.Count == 0)
                        empty++;
                    else
                    {
                        if (empty > 0)
                        {
                            fen += empty;
                            empty = 0;
                        }

                        fen += ConvertToFenChar(
                            ((Label)squares[row, col].Controls[0]).Text);
                    }
                }

                if (empty > 0)
                    fen += empty;

                if (row < 7)
                    fen += "/";
            }

            fen += " w KQkq - 0 1";
            return fen;
        }

        private char ConvertToFenChar(string symbol)
        {
            return symbol switch
            {
                "♜" => 'r',
                "♞" => 'n',
                "♝" => 'b',
                "♛" => 'q',
                "♚" => 'k',
                "♟" => 'p',
                "♖" => 'R',
                "♘" => 'N',
                "♗" => 'B',
                "♕" => 'Q',
                "♔" => 'K',
                "♙" => 'P',
                _ => '1'
            };
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btnSaveGame_Click(object sender, EventArgs e)
        {
            string fen = GenerateFEN(); // используем наш метод FEN
            string gameName = "Game_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
            GameStorage.SaveGame(gameName, fen);
            MessageBox.Show("Game saved to JSON!");

        }
    }
}
