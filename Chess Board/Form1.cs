using System;
using System.Drawing;
using System.Windows.Forms;
using Chess_Board.Data;

namespace Chess_Board
{
    public partial class Form1 : Form
    {
        private Panel[,] squares = new Panel[8, 8];
        private Label? draggedPiece = null;
        private Point mouseOffset;
        private TextBox? fenTextBox;
        private Color highlightColor = Color.LightGreen;
        private Panel? fenPanel;

        // ---------------- Castling rights tracking ----------------

        private bool whiteKingMoved = false;
        private bool blackKingMoved = false;

        private bool whiteRookA_Moved = false; // a1 rook
        private bool whiteRookH_Moved = false; // h1 rook

        private bool blackRookA_Moved = false; // a8 rook
        private bool blackRookH_Moved = false; // h8 rook


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
            tableLayoutPanel1.Dock = DockStyle.Fill;

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
            fenPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = Color.FromArgb(240, 240, 240)
            };

            fenTextBox = new TextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                Font = new Font("Consolas", 12)
            };

            Button copyBtn = new Button
            {
                Text = "Copy FEN",
                Dock = DockStyle.Right,
                Width = 100
            };

            copyBtn.Click += (s, e) =>
            {
                Clipboard.SetText(fenTextBox!.Text);
                MessageBox.Show("FEN copied!");
            };

            fenPanel.Controls.Add(fenTextBox);
            fenPanel.Controls.Add(copyBtn);

            mainPanel.Controls.Add(fenPanel);
            mainPanel.Controls.Add(fenPanel, 0, 0);
            mainPanel.PerformLayout();

        }
        // ---------------- Move validation ---------------- 
        private char[,] GetBoardState()
        {
            char[,] board = new char[8, 8];

            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    if (squares[r, c].Controls.Count == 0)
                        board[r, c] = '.';
                    else
                    {
                        string symbol = ((Label)squares[r, c].Controls[0]).Text;
                        board[r, c] = ConvertToFenChar(symbol);
                    }
                }
            }

            return board;
        }
        private bool IsPathClear(char[,] board, Point from, Point to)
        {
            int dr = Math.Sign(to.X - from.X);
            int dc = Math.Sign(to.Y - from.Y);

            int r = from.X + dr;
            int c = from.Y + dc;

            while (r != to.X || c != to.Y)
            {
                if (board[r, c] != '.')
                    return false;

                r += dr;
                c += dc;
            }

            return true;
        }
        private bool CanMoveWhitePawn(char[,] board, Point from, Point to, int dr, int dc)
        {
            if (dc == 0)
            {
                if (dr == -1 && board[to.X, to.Y] == '.') return true;
                if (dr == -2 && from.X == 6 && board[from.X - 1, from.Y] == '.' && board[to.X, to.Y] == '.') return true;
            }

            if (dr == -1 && Math.Abs(dc) == 1 && board[to.X, to.Y] != '.')
                return true;

            return false;
        }

        private bool CanMoveBlackPawn(char[,] board, Point from, Point to, int dr, int dc)
        {
            if (dc == 0)
            {
                if (dr == 1 && board[to.X, to.Y] == '.') return true;
                if (dr == 2 && from.X == 1 && board[from.X + 1, from.Y] == '.' && board[to.X, to.Y] == '.') return true;
            }

            if (dr == 1 && Math.Abs(dc) == 1 && board[to.X, to.Y] != '.')
                return true;

            return false;
        }
        private Point FindKing(char[,] board, bool white)
        {
            char target = white ? 'K' : 'k';

            for (int r = 0; r < 8; r++)
                for (int c = 0; c < 8; c++)
                    if (board[r, c] == target)
                        return new Point(r, c);

            return new Point(-1, -1);
        }

        private char[,] SimulateMove(char[,] board, Point from, Point to)
        {
            char[,] copy = (char[,])board.Clone();
            copy[to.X, to.Y] = copy[from.X, from.Y];
            copy[from.X, from.Y] = '.';
            return copy;
        }
        private bool LeavesKingInCheck(char[,] board, Point from, Point to)
        {
            char piece = board[from.X, from.Y];
            bool white = char.IsUpper(piece);

            // Simulate the king move
            char[,] next = SimulateMove(board, from, to);

            // Add rook movement for castling 
            if ((piece == 'K' || piece == 'k') && from.X == to.X && Math.Abs(to.Y - from.Y) == 2)
            {
                int row = from.X;

                if (to.Y == 6) // king-side
                {
                    // Move rook from H-file to F-file
                    next[row, 5] = next[row, 7];
                    next[row, 7] = '.';
                }
                else if (to.Y == 2) // queen-side
                {
                    // Move rook from A-file to D-file
                    next[row, 3] = next[row, 0];
                    next[row, 0] = '.';
                }
            }

            Point kingPos = FindKing(next, white);

            // Check if king is attacked
            return IsSquareAttacked(next, kingPos, !white);
        }
        private bool CanMovePieceIgnoringCheck(char[,] board, char piece, Point from, Point to)
        {
            int dr = to.X - from.X;
            int dc = to.Y - from.Y;

            return piece switch
            {
                'P' => CanMoveWhitePawn(board, from, to, dr, dc),
                'p' => CanMoveBlackPawn(board, from, to, dr, dc),
                'R' or 'r' => (dr == 0 || dc == 0) && IsPathClear(board, from, to),
                'B' or 'b' => Math.Abs(dr) == Math.Abs(dc) && IsPathClear(board, from, to),
                'Q' or 'q' => (dr == 0 || dc == 0 || Math.Abs(dr) == Math.Abs(dc)) && IsPathClear(board, from, to),
                'N' or 'n' => (Math.Abs(dr) == 2 && Math.Abs(dc) == 1) || (Math.Abs(dr) == 1 && Math.Abs(dc) == 2),
                'K' or 'k' => Math.Abs(dr) <= 1 && Math.Abs(dc) <= 1,
                _ => false
            };
        }
        private bool IsSquareAttacked(char[,] board, Point square, bool byWhite)
        {
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    char piece = board[r, c];
                    if (piece == '.') continue;

                    if (char.IsUpper(piece) != byWhite) continue;

                    if (CanMovePieceIgnoringCheck(board, piece, new Point(r, c), square))
                        return true;
                }
            }

            return false;
        }
        private bool CanCastle(char[,] board, Point from, Point to, char king)
        {
            bool white = char.IsUpper(king);
            int row = white ? 7 : 0;

            // King must be on E1/E8
            if (from.X != row || from.Y != 4)
                return false;

            bool kingSide = to.Y == 6;
            bool queenSide = to.Y == 2;

            // King must not have moved
            if (white && whiteKingMoved) return false;
            if (!white && blackKingMoved) return false;

            // Rook must not have moved
            bool rookMoved = white
                ? (kingSide ? whiteRookH_Moved : whiteRookA_Moved)
                : (kingSide ? blackRookH_Moved : blackRookA_Moved);

            if (rookMoved) return false;

            // Path must be empty
            if (kingSide)
            {
                if (board[row, 5] != '.') return false;
                if (board[row, 6] != '.') return false;
            }
            else if (queenSide)
            {
                if (board[row, 3] != '.') return false;
                if (board[row, 2] != '.') return false;
                if (board[row, 1] != '.') return false;
            }
            else
            {
                return false;
            }


            // 1. King cannot be in check on E1/E8
            if (IsSquareAttacked(board, new Point(row, 4), !white))
                return false;

            // 2. King cannot pass through check (F1/F8 or D1/D8)
            if (kingSide)
            {
                if (IsSquareAttacked(board, new Point(row, 5), !white))
                    return false;
            }
            else // queenSide
            {
                if (IsSquareAttacked(board, new Point(row, 3), !white))
                    return false;
            }

            // 3. King cannot end in check (G1/G8 or C1/C8)
            if (IsSquareAttacked(board, new Point(row, to.Y), !white))
                return false;

            return true;
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
            HighlightMoves(draggedPiece);

            // Start drag immediately on mouse down
            draggedPiece.DoDragDrop(draggedPiece, DragDropEffects.Move);
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

            Panel parent = draggedPiece.Parent as Panel;
            if (parent == null) return;

            Point from = (Point)parent.Tag;
            Point to = (Point)targetSquare.Tag;

            if (targetSquare.BackColor != highlightColor)
            {
                ClearHighlights();
                draggedPiece = null;
                return;
            }

            // Move the piece
            parent.Controls.Clear();
            targetSquare.Controls.Clear();
            targetSquare.Controls.Add(draggedPiece);

            // Track king/rook movement
            char movedPiece = ConvertToFenChar(draggedPiece.Text);

            if (movedPiece == 'K') whiteKingMoved = true;
            if (movedPiece == 'k') blackKingMoved = true;

            if (movedPiece == 'R')
            {
                if (from.X == 7 && from.Y == 0) whiteRookA_Moved = true;
                if (from.X == 7 && from.Y == 7) whiteRookH_Moved = true;
            }

            if (movedPiece == 'r')
            {
                if (from.X == 0 && from.Y == 0) blackRookA_Moved = true;
                if (from.X == 0 && from.Y == 7) blackRookH_Moved = true;
            }

            // Castling move - also move the rook
            if (movedPiece == 'K' || movedPiece == 'k')
            {
                // King-side
                if (to.Y == 6)
                {
                    Panel rookFrom = squares[from.X, 7];
                    Panel rookTo = squares[from.X, 5];

                    if (rookFrom.Controls.Count > 0)
                    {
                        Label rook = (Label)rookFrom.Controls[0];
                        rookFrom.Controls.Clear();
                        rookTo.Controls.Clear();
                        rookTo.Controls.Add(rook);
                    }
                }

                // Queen-side
                if (to.Y == 2)
                {
                    Panel rookFrom = squares[from.X, 0];
                    Panel rookTo = squares[from.X, 3];

                    if (rookFrom.Controls.Count > 0)
                    {
                        Label rook = (Label)rookFrom.Controls[0];
                        rookFrom.Controls.Clear();
                        rookTo.Controls.Clear();
                        rookTo.Controls.Add(rook);
                    }
                }
            }

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

            // Highlight castling moves for kings
            if (symbol == "♔") // white king
            {
                if (CanMove(symbol, pos, new Point(7, 6))) // G1
                    squares[7, 6].BackColor = highlightColor;

                if (CanMove(symbol, pos, new Point(7, 2))) // C1
                    squares[7, 2].BackColor = highlightColor;
            }

            if (symbol == "♚") // black king
            {
                if (CanMove(symbol, pos, new Point(0, 6))) // G8
                    squares[0, 6].BackColor = highlightColor;

                if (CanMove(symbol, pos, new Point(0, 2))) // C8
                    squares[0, 2].BackColor = highlightColor;
            }



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
            char[,] board = GetBoardState();
            char piece = ConvertToFenChar(symbol);
            char target = board[to.X, to.Y];

            if (target != '.' && char.IsUpper(piece) == char.IsUpper(target))
                return false;

            int dr = to.X - from.X;
            int dc = to.Y - from.Y;

            if ((piece == 'K' || piece == 'k') && dr == 0 && Math.Abs(dc) == 2)
            {
                return CanCastle(board, from, to, piece);
            }

            bool legal = piece switch
            {
                'P' => CanMoveWhitePawn(board, from, to, dr, dc),
                'p' => CanMoveBlackPawn(board, from, to, dr, dc),
                'R' or 'r' => (dr == 0 || dc == 0) && IsPathClear(board, from, to),
                'B' or 'b' => Math.Abs(dr) == Math.Abs(dc) && IsPathClear(board, from, to),
                'Q' or 'q' => (dr == 0 || dc == 0 || Math.Abs(dr) == Math.Abs(dc)) && IsPathClear(board, from, to),
                'N' or 'n' => (Math.Abs(dr) == 2 && Math.Abs(dc) == 1) || (Math.Abs(dr) == 1 && Math.Abs(dc) == 2),
                'K' or 'k' => Math.Abs(dr) <= 1 && Math.Abs(dc) <= 1,
                _ => false
            };

            if (!legal)
                return false;

            bool isCastling = (piece == 'K' || piece == 'k') && dr == 0 && Math.Abs(dc) == 2;

            if (!isCastling && LeavesKingInCheck(board, from, to))
                return false;

            return true;
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
            string fen = GenerateFEN(); // используем наш метод (use our method) FEN
            string gameName = "Game_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
            GameStorage.SaveGame(gameName, fen);
            MessageBox.Show("Game saved to JSON!");

        }
    }
}
