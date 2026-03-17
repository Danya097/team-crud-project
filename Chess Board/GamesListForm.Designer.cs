using System.Windows.Forms;

namespace Chess_Board
{
    partial class GamesListForm
    {
        private System.ComponentModel.IContainer components = null;
        private ListBox listBoxGames;
        private Button btnLoad;
        private Button btnCancel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.listBoxGames = new ListBox();
            this.btnLoad = new Button();
            this.btnCancel = new Button();
            this.SuspendLayout();

            // 
            // listBoxGames
            // 
            this.listBoxGames.Dock = DockStyle.Top;
            this.listBoxGames.Height = 200;

            // 
            // btnLoad
            // 
            this.btnLoad.Text = "Load";
            this.btnLoad.Dock = DockStyle.Left;
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);

            // 
            // btnCancel
            // 
            this.btnCancel.Text = "Cancel";
            this.btnCancel.Dock = DockStyle.Right;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);

            // 
            // GamesListForm
            // 
            this.ClientSize = new System.Drawing.Size(400, 250);
            this.Controls.Add(this.listBoxGames);
            this.Controls.Add(this.btnLoad);
            this.Controls.Add(this.btnCancel);
            this.Text = "Saved Games";
            this.ResumeLayout(false);
        }
    }
}

