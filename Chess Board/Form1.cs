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

                    squares[row, col] = p;
                    tableLayoutPanel1.Controls.Add(p, col, row);
                }
            }
        }
    }
}