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
                LoadAssembly(filePath);
            }
        }

        private void LoadAssembly(string filePath)
        {
            clsAssemblyReader reader = new clsAssemblyReader();
            Assembly assemblyDLL = reader.LoadFile(filePath, out string errorMessage);

            if (assemblyDLL == null)
            {
                MessageBox.Show(errorMessage, "Assembly Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            DisplayAssembly(assemblyDLL, reader);
        }

        private void DisplayAssembly(Assembly assembly, clsAssemblyReader reader)
        {
            treeView1.Nodes.Clear();
            TreeNode rootNode = new TreeNode(assembly.GetName().Name);

            foreach (var type in reader.GetAssemblyTypes(assembly))
            {
                rootNode.Nodes.Add(CreateTypeNode(type, reader));
            }

            treeView1.Nodes.Add(rootNode);
            rootNode.Expand();
        }

        private TreeNode CreateTypeNode(Type type, clsAssemblyReader reader)
        {
            TreeNode typeNode = new TreeNode(type.Name);
            typeNode.Tag = type;

            AddMembersNodes(typeNode, "Properties", reader.GetPropertiesByType(type));

            AddMembersNodes(typeNode, "Fields", reader.GetFieldsInfosByType(type));

            AddMembersNodes(typeNode, "Methods", reader.GetMethodByType(type));

            return typeNode;
        }

        private void AddMembersNodes(TreeNode parent, string categoryName, MemberInfo[] members)
        {
            if (members.Length == 0)
                return;

            TreeNode folder = new TreeNode(
                $"{categoryName} ({members.Length})");

            foreach (var member in members)
            {
                TreeNode memberNode = new TreeNode(member.Name);
                memberNode.Tag = member;
                folder.Nodes.Add(memberNode);
            }

            parent.Nodes.Add(folder);
        }


        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node?.Tag == null)
            {
                SetDetails(e.Node?.Text ?? "-",
                    e.Node?.Parent == null ? "Assembly Root" : "Category Folder",
                    string.Empty);
                return;
            }

            if (e.Node.Tag is Type type)
            {
                ShowTypeDetails(type);
            }
            else if (e.Node.Tag is MethodInfo method)
            {
                ShowMethodDetails(method);
            }
            else if (e.Node.Tag is PropertyInfo prop)
            {
                ShowPropertyDetails(prop);
            }
            else if (e.Node.Tag is FieldInfo field)
            {
                ShowFieldDetails(field);
            }
        }

        private void SetDetails(string name, string type, string signature)
        {
            lblItemName.Text = name;
            lblItemType.Text = type;
            txtSignature.Text = signature;
        }

        private void ShowTypeDetails(Type type)
        {
            string signature = $"Full Name: {type.FullName}{Environment.NewLine}" +
                               $"Namespace: {type.Namespace}{Environment.NewLine}" +
                               $"Base Type: {type.BaseType?.Name ?? "None"}";

            SetDetails(type.Name, this.GetReturnType(type).ToString(), signature);
        }

        private void ShowMethodDetails(MethodInfo method)
        {
            var parameters = method.GetParameters()
                                   .Select(p => $"{p.ParameterType.Name} {p.Name}");

            string paramString = string.Join(", ", parameters);

            string accessModifier = method.IsPublic ? "public" : "private/internal";
            string staticModifier = method.IsStatic ? "static " : "";

            string signature = $"{accessModifier} {staticModifier}{method.ReturnType.Name} {method.Name}({paramString})";

            SetDetails(method.Name, method.ReturnType.Name, signature);
        }

        private void ShowPropertyDetails(PropertyInfo prop)
        {
            string getters = prop.CanRead ? "get; " : "";
            string setters = prop.CanWrite ? "set; " : "";

            string signature = $"{prop.PropertyType.Name} {prop.Name} {{ {getters}{setters}}}";

            SetDetails(prop.Name, prop.PropertyType.Name, signature);
        }

        private void ShowFieldDetails(FieldInfo field)
        {
            string access = field.IsPublic ? "public" : "private/internal";
            string isStatic = field.IsStatic ? "static " : "";

            string signature = $"{access} {isStatic}{field.FieldType.Name} {field.Name};";

            SetDetails(field.Name, field.FieldType.Name, signature);
        }
    }
}
