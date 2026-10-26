using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DLL_Explorer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btOpen_Click(object sender, EventArgs e)
        {
            string filePath = string.Empty;
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.Filter = "DLL Files (*.dll)|*.dll";
            if (ofd.ShowDialog() == DialogResult.OK) { 
                filePath = ofd.FileName;
                MessageBox.Show(filePath);
                
            }
        }
    }
}
