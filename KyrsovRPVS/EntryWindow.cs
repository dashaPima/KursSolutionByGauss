namespace KyrsovRPVS
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.Text = "Курсовая работа";
        }
        private void btnCloseProgram_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void openMainWindow(object sender, EventArgs e)
        {
            MainForm mainForm = new MainForm();
            mainForm.Show();
            this.Hide();
        }
    }
}
