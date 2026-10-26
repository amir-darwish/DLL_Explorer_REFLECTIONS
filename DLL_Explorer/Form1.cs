using DLL_Explorer.Services;
using System;
using System.Reflection;
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
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                filePath = ofd.FileName;
                SendPathToAnlayseService(filePath);



            }
        }

        private void SendPathToAnlayseService(string filePath)
        {
            clsAssemblyReader reader = new clsAssemblyReader();
            Assembly assemblyDLL = reader.LoadFile(filePath, out string errorMessage);
            if (assemblyDLL == null)
            {
                MessageBox.Show(errorMessage, "Assembly Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return;
            }

            treeView1.Nodes.Clear();
            treeView1.Nodes.Add(new TreeNode(assemblyDLL.GetName().Name));


        }


    }
}
