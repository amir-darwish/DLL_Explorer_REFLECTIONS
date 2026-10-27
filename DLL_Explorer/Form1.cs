using DLL_Explorer.Services;
using System;
using System.Linq;
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
            Type[] types = reader.GetAssemblyTypes(assemblyDLL);

            treeView1.Nodes.Clear();
            TreeNode rootNode = new TreeNode(assemblyDLL.GetName().Name);

            foreach (var type in types)
            {
                TreeNode typeNode = new TreeNode(type.Name);
                typeNode.Tag = type;
                MethodInfo[] methods = reader.GetMethodByType(type);

                foreach (var method in methods)
                {
                    TreeNode methodNode = new TreeNode(method.Name);
                    methodNode.Tag = method;
                    typeNode.Nodes.Add(methodNode);
                }

                rootNode.Nodes.Add(typeNode);
            }
            treeView1.Nodes.Add(rootNode);
            rootNode.Expand();

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node?.Tag == null)
            {
                lblItemName.Text = e.Node?.Text ?? "-";
                lblItemType.Text = "Assembly Root";
                txtSignature.Clear();
                return;
            }

            if (e.Node.Tag is Type type)
            {
                lblItemName.Text = type.Name;
                lblItemType.Text = type.IsInterface ? "Interface" : "Class";

                txtSignature.Text = $"Full Name: {type.FullName}{Environment.NewLine}" +
                                    $"Namespace: {type.Namespace}{Environment.NewLine}" +
                                    $"Base Type: {type.BaseType?.Name ?? "None"}";
            }

            else if (e.Node.Tag is MethodInfo method)
            {
                lblItemName.Text = method.Name;
                lblItemType.Text = method.ReturnType.Name;

                var parameters = method.GetParameters()
                                       .Select(p => $"{p.ParameterType.Name} {p.Name}");

                string paramString = string.Join(", ", parameters);

                string accessModifier = method.IsPublic ? "public" : "private/internal";
                string staticModifier = method.IsStatic ? "static " : "";

                txtSignature.Text = $"{accessModifier} {staticModifier}{method.ReturnType.Name} {method.Name}({paramString})";
            }
        }
    }
}
