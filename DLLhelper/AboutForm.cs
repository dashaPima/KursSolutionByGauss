using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DLLhelper
{
    public partial class AboutForm : Form
    {
        public AboutForm(string topicFile)
        {
            InitializeComponent();
            string helpPath = Path.Combine(Application.StartupPath, "HelpFiles", topicFile);
            if (File.Exists(helpPath))
            {
                webBrowser1.Navigate(helpPath);
            }
            else
            {
                MessageBox.Show("Файл справки не найден: " + helpPath, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
