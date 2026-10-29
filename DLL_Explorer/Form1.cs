using DLL_Explorer.Services;
using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace DLL_Explorer
{
    public partial class Form1 : Form
    {
        enum enType { Class, Interface, Struct, Enum, Unknown };
        public Form1()
        {
            InitializeComponent();
        }

        private enType GetReturnType(Type type)
        {
            if (type == null)
                return enType.Unknown;

            if (type.IsEnum)
                return enType.Enum;

            if (type.IsValueType)
                return enType.Struct;

            if (type.IsInterface)
                return enType.Interface;

            if (type.IsClass)
                return enType.Class;

            return enType.Unknown;
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

                PropertyInfo[] properties = reader.GetPropertiesByType(type);
                if (properties.Length > 0)
                {
                    TreeNode propsFolder = new TreeNode($"Properties ({properties.Length})");
                    foreach (var prop in properties)
                    {
                        TreeNode propNode = new TreeNode(prop.Name);
                        propNode.Tag = prop;
                        propsFolder.Nodes.Add(propNode);
                    }
                    typeNode.Nodes.Add(propsFolder);
                }

                FieldInfo[] fields = reader.GetFieldsInfosByType(type);
                if (fields.Length > 0)
                {
                    TreeNode fieldsFolder = new TreeNode($"Fields ({fields.Length})");
                    foreach (var field in fields)
                    {
                        TreeNode fieldNode = new TreeNode(field.Name);
                        fieldNode.Tag = field;
                        fieldsFolder.Nodes.Add(fieldNode);
                    }
                    typeNode.Nodes.Add(fieldsFolder);
                }


                MethodInfo[] methods = reader.GetMethodByType(type);
                if (methods.Length > 0)
                {
                    TreeNode methodsFolder = new TreeNode($"Methods ({methods.Length})");
                    foreach (var method in methods)
                    {
                        TreeNode methodNode = new TreeNode(method.Name);
                        methodNode.Tag = method;
                        methodsFolder.Nodes.Add(methodNode);
                    }
                    typeNode.Nodes.Add(methodsFolder);
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
                lblItemType.Text = e.Node?.Parent == null ? "Assembly Root" : "Category Folder";
                txtSignature.Clear();
                return;
            }

            if (e.Node.Tag is Type type)
            {
                lblItemName.Text = type.Name;
                lblItemType.Text = this.GetReturnType(type).ToString();

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
            else if (e.Node.Tag is PropertyInfo prop)
            {
                lblItemName.Text = prop.Name;
                lblItemType.Text = prop.PropertyType.Name;

                string getters = prop.CanRead ? "get; " : "";
                string setters = prop.CanWrite ? "set; " : "";

                txtSignature.Text = $"{prop.PropertyType.Name} {prop.Name} {{ {getters}{setters}}}";
            }
            else if (e.Node.Tag is FieldInfo field)
            {
                lblItemName.Text = field.Name;
                lblItemType.Text = field.FieldType.Name;

                string access = field.IsPublic ? "public" : "private/internal";
                string isStatic = field.IsStatic ? "static " : "";

                txtSignature.Text = $"{access} {isStatic}{field.FieldType.Name} {field.Name};";
            }
        }
    }
}
