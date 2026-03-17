using System;
using System.Windows.Forms;

namespace Chess_Board
{
    public partial class GamesListForm : Form
    {
        public string SelectedFen { get; private set; }

        public GamesListForm()
        {
            InitializeComponent(); 
            LoadSavedGames();
        }

        private void LoadSavedGames()
        {
            
            listBoxGames.Items.Add("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
            listBoxGames.Items.Add("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR b KQkq - 0 1");
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            if (listBoxGames.SelectedItem != null)
            {
                SelectedFen = listBoxGames.SelectedItem.ToString();
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show("Select a game first.");
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
