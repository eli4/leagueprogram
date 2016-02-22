using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LeagueAppWF
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private async void searchButton_Click(object sender, EventArgs e)
        {
            List<winInfo> x = await leagueInfo.Core(summonerName.Text.Replace(" ", ""));

            Form2 results = new Form2();
            results.Text = "test";
            results.tableLayoutPanel1.ColumnCount = 4;
            results.tableLayoutPanel1.RowCount = x.Count() + 1;
            results.tableLayoutPanel1.ColumnStyles.Clear();
            results.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            results.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            results.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            results.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            results.tableLayoutPanel1.RowStyles.Clear();
            results.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            results.tableLayoutPanel1.Controls.Add(new Label() { Text = "Summoner Names", Font = new Font(this.Font, FontStyle.Bold)}, 0, 0);
            results.tableLayoutPanel1.Controls.Add(new Label() { Text = "Games Played", Font = new Font(this.Font, FontStyle.Bold) },  1, 0);
            results.tableLayoutPanel1.Controls.Add(new Label() { Text = "Games Won", Font = new Font(this.Font, FontStyle.Bold) }, 2, 0);
            results.tableLayoutPanel1.Controls.Add(new Label() { Text = "Win Percentage", Font = new Font(this.Font, FontStyle.Bold) }, 3, 0);
            for (int i = 0; i<x.Count(); i++)
            {
                results.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
                results.tableLayoutPanel1.Controls.Add(new Label() { Text = x[i].name }, 0, i+1);
                results.tableLayoutPanel1.Controls.Add(new Label() { Text = x[i].games.ToString() }, 1, i+1);
                results.tableLayoutPanel1.Controls.Add(new Label() { Text = x[i].wins.ToString() }, 2, i+1);
                results.tableLayoutPanel1.Controls.Add(new Label() { Text = x[i].winPercent.ToString() }, 3, i+1);
            }

            results.ShowDialog();
        }

        private void summonerName_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(summonerName.Text))
            {
                searchButton.Enabled = true;
            }
        }
    }
}
