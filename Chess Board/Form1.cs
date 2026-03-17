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
        private Color checkHighlightColor = Color.FromArgb(220, 50, 50);
        private Panel? fenPanel;

        // ---------------- Castling rights tracking ----------------

        private bool whiteKingMoved = false;
        private bool blackKingMoved = false;

        private bool whiteRookA_Moved = false; // a1 rook
        private bool whiteRookH_Moved = false; // h1 rook

        private bool blackRookA_Moved = false; // a8 rook
        private bool blackRookH_Moved = false; // h8 rook

        // ---------------- Turn tracking ----------------
        private bool isWhiteTurn = true;
        private Label? turnLabel = null;

        // ---------------- Move history ----------------
        private List<string> moveHistory = new List<string>();
        private ListBox? moveListBox = null;
        private int moveNumber = 1;

        // ---------------- Game state ----------------
        private bool gameOver = false;


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
            CreateMoveHistoryPanel();
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
                Height = 50,
                BackColor = Color.FromArgb(34, 34, 34),
                Padding = new Padding(6, 6, 6, 6)
            };

            // Title label
            Label titleLabel = new Label
            {
                Text = "♟ Chess",
                Dock = DockStyle.Left,
                Width = 90,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = Color.White
            };

            // FEN textbox
            fenTextBox = new TextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                Font = new Font("Consolas", 9),
                BackColor = Color.FromArgb(55, 55, 55),
                ForeColor = Color.LightGray,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Turn indicator label
            turnLabel = new Label
            {
                Text = "⬜ White's Turn",
                Dock = DockStyle.Right,
                Width = 130,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(60, 60, 60)
            };

            // Helper to create styled toolbar buttons
            Button MakeBtn(string text, Color accent) => new Button
            {
                Text = text,
                Dock = DockStyle.Right,
                Width = 85,
                FlatStyle = FlatStyle.Flat,
                BackColor = accent,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                FlatAppearance = { BorderSize = 0 },
                Cursor = Cursors.Hand
            };

            Button copyBtn  = MakeBtn("📋 Copy FEN", Color.FromArgb(70, 70, 90));
            Button saveBtn  = MakeBtn("💾 Save",     Color.FromArgb(50, 100, 160));
            Button loadBtn  = MakeBtn("📂 Load",     Color.FromArgb(80, 120, 80));
            Button newBtn   = MakeBtn("🔄 New Game", Color.FromArgb(160, 80, 50));

            copyBtn.Click += (s, e) => { Clipboard.SetText(fenTextBox!.Text); MessageBox.Show("FEN copied!"); };
            saveBtn.Click += btnSaveGame_Click;
            loadBtn.Click += btnLoadGame_Click;
            newBtn.Click  += btnNewGame_Click;

            // Add right-to-left (WinForms adds Right-docked controls in reverse)
            fenPanel.Controls.Add(fenTextBox);
            fenPanel.Controls.Add(turnLabel);
            fenPanel.Controls.Add(copyBtn);
            fenPanel.Controls.Add(saveBtn);
            fenPanel.Controls.Add(loadBtn);
            fenPanel.Controls.Add(newBtn);
            fenPanel.Controls.Add(titleLabel);

            mainPanel.Controls.Add(fenPanel);
            mainPanel.Controls.Add(fenPanel, 0, 0);

            // Update row height to match new toolbar
            mainPanel.RowStyles[0] = new RowStyle(SizeType.Absolute, 50F);
            mainPanel.PerformLayout();
        }
        private void CreateMoveHistoryPanel()
        {
            Panel historyPanel = new Panel
            {
                Width = 160,
                Dock = DockStyle.Right,
                BackColor = Color.FromArgb(245, 245, 245),
                Padding = new Padding(4)
            };

            Label historyTitle = new Label
            {
                Text = "Move History",
                Dock = DockStyle.Top,
                Height = 24,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };

            moveListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 9),
                BorderStyle = BorderStyle.None
            };

            historyPanel.Controls.Add(moveListBox);
            historyPanel.Controls.Add(historyTitle);
            this.Controls.Add(historyPanel);
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
            if (gameOver) return;

            Label? clicked = sender as Label;
            if (clicked == null) return;

            char piece = ConvertToFenChar(clicked.Text);
            bool pieceIsWhite = char.IsUpper(piece);

            // Only allow moving the correct color's pieces
            if (pieceIsWhite != isWhiteTurn) return;

            draggedPiece = clicked;
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
            if (gameOver) return;

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

            // Record move notation before moving
            char movedPiece = ConvertToFenChar(draggedPiece.Text);
            string moveNotation = BuildMoveNotation(movedPiece, from, to);

            // Move the piece
            parent.Controls.Clear();
            targetSquare.Controls.Clear();
            targetSquare.Controls.Add(draggedPiece);

            // Track king/rook movement
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

            // Castling - also move the rook
            if (movedPiece == 'K' || movedPiece == 'k')
            {
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

            // Pawn promotion
            if (movedPiece == 'P' && to.X == 0)
                PromotePawn(to, true);
            else if (movedPiece == 'p' && to.X == 7)
                PromotePawn(to, false);

            draggedPiece = null;
            ClearHighlights();

            // Switch turns
            isWhiteTurn = !isWhiteTurn;

            // Check / checkmate detection
            char[,] board = GetBoardState();
            bool opponentInCheck = IsKingInCheck(board, isWhiteTurn);
            bool opponentHasMoves = PlayerHasLegalMoves(board, isWhiteTurn);

            if (opponentInCheck && !opponentHasMoves)
            {
                moveNotation += "#";
                AddMoveToHistory(moveNotation);
                UpdateFEN();
                string winner = isWhiteTurn ? "Black" : "White";
                gameOver = true;
                if (turnLabel != null) turnLabel.Text = $"🏆 {winner} wins!";
                MessageBox.Show($"Checkmate! {winner} wins!", "Game Over");
                return;
            }
            else if (!opponentInCheck && !opponentHasMoves)
            {
                moveNotation += " (stalemate)";
                AddMoveToHistory(moveNotation);
                UpdateFEN();
                gameOver = true;
                if (turnLabel != null) turnLabel.Text = "🤝 Stalemate!";
                MessageBox.Show("Stalemate! It's a draw.", "Game Over");
                return;
            }
            else if (opponentInCheck)
            {
                moveNotation += "+";
            }

            AddMoveToHistory(moveNotation);

            if (turnLabel != null)
                turnLabel.Text = isWhiteTurn ? "⬜ White's Turn" : "⬛ Black's Turn";

            ApplyCheckHighlight();
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

            // Re-apply check highlight if king is in check
            ApplyCheckHighlight();
        }

        private void ApplyCheckHighlight()
        {
            char[,] board = GetBoardState();
            if (IsKingInCheck(board, isWhiteTurn))
            {
                Point king = FindKing(board, isWhiteTurn);
                if (king.X != -1)
                    squares[king.X, king.Y].BackColor = checkHighlightColor;
            }
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

            fen += isWhiteTurn ? " w KQkq - 0 1" : " b KQkq - 0 1";
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

        // ---------------- Pawn Promotion ----------------

        private void PromotePawn(Point pos, bool white)
        {
            string queen = white ? "♕" : "♛";
            string rook  = white ? "♖" : "♜";
            string bishop = white ? "♗" : "♝";
            string knight = white ? "♘" : "♞";

            Form dialog = new Form
            {
                Text = "Pawn Promotion",
                Size = new Size(340, 100),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false, MinimizeBox = false
            };

            string chosen = queen;
            foreach (string sym in new[] { queen, rook, bishop, knight })
            {
                string s = sym;
                Button btn = new Button
                {
                    Text = s,
                    Font = new Font("Segoe UI", 20),
                    Width = 70, Height = 60,
                    Dock = DockStyle.Left
                };
                btn.Click += (sender, e) => { chosen = s; dialog.DialogResult = DialogResult.OK; };
                dialog.Controls.Add(btn);
            }

            dialog.ShowDialog(this);

            // Replace the pawn with chosen piece
            squares[pos.X, pos.Y].Controls.Clear();
            AddPiece(pos.X, pos.Y, chosen);
        }

        // ---------------- Check / Checkmate ----------------

        private bool IsKingInCheck(char[,] board, bool white)
        {
            Point king = FindKing(board, white);
            if (king.X == -1) return false;
            return IsSquareAttacked(board, king, !white);
        }

        private bool PlayerHasLegalMoves(char[,] board, bool white)
        {
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    char piece = board[r, c];
                    if (piece == '.') continue;
                    if (char.IsUpper(piece) != white) continue;

                    string symbol = ConvertToSymbol(piece);
                    for (int tr = 0; tr < 8; tr++)
                        for (int tc = 0; tc < 8; tc++)
                            if (CanMove(symbol, new Point(r, c), new Point(tr, tc)))
                                return true;
                }
            }
            return false;
        }

        private string ConvertToSymbol(char fen)
        {
            return fen switch
            {
                'r' => "♜", 'n' => "♞", 'b' => "♝", 'q' => "♛",
                'k' => "♚", 'p' => "♟", 'R' => "♖", 'N' => "♘",
                'B' => "♗", 'Q' => "♕", 'K' => "♔", 'P' => "♙",
                _ => ""
            };
        }

        // ---------------- Move History ----------------

        private string BuildMoveNotation(char piece, Point from, Point to)
        {
            char file = (char)('a' + to.Y);
            int rank = 8 - to.X;
            string dest = $"{file}{rank}";

            if (piece == 'P' || piece == 'p')
            {
                char[,] board = GetBoardState();
                bool capture = board[to.X, to.Y] != '.';
                if (capture)
                {
                    char fromFile = (char)('a' + from.Y);
                    return $"{fromFile}x{dest}";
                }
                return dest;
            }

            char pieceLetter = char.ToUpper(piece);
            char[,] b = GetBoardState();
            bool isCapture = b[to.X, to.Y] != '.';
            return $"{pieceLetter}{(isCapture ? "x" : "")}{dest}";
        }

        private void AddMoveToHistory(string notation)
        {
            if (moveListBox == null) return;

            // White move starts a new numbered entry, black appends to it
            if (!isWhiteTurn) // we already flipped, so !isWhiteTurn means white just moved
            {
                moveListBox.Items.Add($"{moveNumber}. {notation}");
            }
            else
            {
                // Black just moved — update the last entry
                if (moveListBox.Items.Count > 0)
                {
                    string last = moveListBox.Items[moveListBox.Items.Count - 1].ToString()!;
                    moveListBox.Items[moveListBox.Items.Count - 1] = $"{last}  {notation}";
                }
                moveNumber++;
            }

            moveListBox.TopIndex = moveListBox.Items.Count - 1;
        }

        // ---------------- New Game ----------------

        private void btnNewGame_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Start a new game? Current game will be lost.",
                "New Game", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes) return;

            ResetGame();
        }

        private void ResetGame()
        {
            // Clear board
            for (int r = 0; r < 8; r++)
                for (int c = 0; c < 8; c++)
                    squares[r, c].Controls.Clear();

            // Reset all flags
            whiteKingMoved = blackKingMoved = false;
            whiteRookA_Moved = whiteRookH_Moved = false;
            blackRookA_Moved = blackRookH_Moved = false;
            isWhiteTurn = true;
            gameOver = false;
            moveNumber = 1;
            moveHistory.Clear();
            moveListBox?.Items.Clear();

            if (turnLabel != null) turnLabel.Text = "⬜ White's Turn";

            SetupPieces();
            UpdateFEN();
        }

        private void btnSaveGame_Click(object sender, EventArgs e)
        {
            string fen = GenerateFEN();
            string gameName = "Game_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
            GameStorage.SaveGame(gameName, fen);
            MessageBox.Show($"Game saved as: {gameName}", "Game Saved");
        }

        private void btnLoadGame_Click(object sender, EventArgs e)
        {
            var saves = GameStorage.LoadAll();

            if (saves.Count == 0)
            {
                MessageBox.Show("No saved games found.", "Load Game");
                return;
            }

            // Show a selection dialog
            Form dialog = new Form
            {
                Text = "Load Game",
                Size = new Size(400, 300),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            ListBox listBox = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 10)
            };

            foreach (var key in saves.Keys)
                listBox.Items.Add(key);

            listBox.SelectedIndex = 0;

            Button btnLoad = new Button
            {
                Text = "Load",
                Dock = DockStyle.Bottom,
                Height = 35
            };

            btnLoad.Click += (s, ev) =>
            {
                if (listBox.SelectedItem == null) return;
                string selected = listBox.SelectedItem.ToString()!;
                string fen = saves[selected];
                LoadFEN(fen);
                dialog.Close();
            };

            dialog.Controls.Add(listBox);
            dialog.Controls.Add(btnLoad);
            dialog.ShowDialog(this);
        }

        private void LoadFEN(string fen)
        {
            // Clear the board
            for (int r = 0; r < 8; r++)
                for (int c = 0; c < 8; c++)
                    squares[r, c].Controls.Clear();

            // Reset castling flags
            whiteKingMoved = blackKingMoved = false;
            whiteRookA_Moved = whiteRookH_Moved = false;
            blackRookA_Moved = blackRookH_Moved = false;
            gameOver = false;
            moveNumber = 1;
            moveHistory.Clear();
            moveListBox?.Items.Clear();

            string[] parts = fen.Split(' ');
            string[] rows = parts[0].Split('/');

            var fenToSymbol = new Dictionary<char, string>
            {
                ['r'] = "♜", ['n'] = "♞", ['b'] = "♝", ['q'] = "♛",
                ['k'] = "♚", ['p'] = "♟", ['R'] = "♖", ['N'] = "♘",
                ['B'] = "♗", ['Q'] = "♕", ['K'] = "♔", ['P'] = "♙"
            };

            for (int r = 0; r < 8; r++)
            {
                int col = 0;
                foreach (char ch in rows[r])
                {
                    if (char.IsDigit(ch))
                        col += (ch - '0');
                    else if (fenToSymbol.TryGetValue(ch, out string? symbol))
                    {
                        AddPiece(r, col, symbol);
                        col++;
                    }
                }
            }

            // Restore turn
            if (parts.Length > 1)
            {
                isWhiteTurn = parts[1] == "w";
                if (turnLabel != null)
                    turnLabel.Text = isWhiteTurn ? "⬜ White's Turn" : "⬛ Black's Turn";
            }

            UpdateFEN();
            ApplyCheckHighlight();
        }
    }
}
