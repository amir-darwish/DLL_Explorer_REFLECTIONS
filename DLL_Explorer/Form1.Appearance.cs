using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace DLL_Explorer
{
    public partial class Form1
    {
        private static readonly Color Ink = Color.FromArgb(44, 68, 59);
        private static readonly Color Muted = Color.FromArgb(117, 137, 127);
        private static readonly Color Accent = Color.FromArgb(25, 121, 105);
        private static readonly Color PrivateColor = Color.FromArgb(139, 90, 145);
        private static readonly Color Surface = Color.FromArgb(247, 250, 248);
        private static readonly Color Selected = Color.FromArgb(224, 241, 234);
        private readonly List<Font> _appearanceFonts = new List<Font>();

        private Font CreateAppearanceFont(string family, float size, FontStyle style = FontStyle.Regular)
        {
            Font font = new Font(family, size, style);
            _appearanceFonts.Add(font);
            return font;
        }

        private void ApplyAppearance()
        {
            Font = CreateAppearanceFont("Segoe UI", 9F);
            BackColor = Color.White;
            ForeColor = Ink;
            headerLayout.BackColor = Color.White;
            filterBar.BackColor = Surface;
            splitMain.BackColor = Color.FromArgb(230, 238, 234);
            splitMain.Panel1.BackColor = Color.FromArgb(250, 252, 251);
            splitMain.Panel2.BackColor = Color.White;
            treeView1.BackColor = splitMain.Panel1.BackColor;
            treeView1.ForeColor = Ink;
            txtSearch.BackColor = Surface;
            txtSearch.ForeColor = Ink;
            signaturePanel.BackColor = txtSignature.BackColor = Surface;
            txtSignature.Font = CreateAppearanceFont("Consolas", 11F);
            lblItemName.Font = CreateAppearanceFont("Segoe UI", 23F);
            lblAssemblyName.Font = CreateAppearanceFont("Segoe UI", 13F, FontStyle.Bold);
            lblItemType.BackColor = Selected;
            lblItemType.ForeColor = Accent;

            foreach (Label label in new[] { lblAssemblyInfo, lblSearch, lblFilter, lblExplorer, lblSummary,
                lblLegend, lblBreadcrumb, lblDescription, lblSignature, lblMetadata })
            {
                label.ForeColor = Muted;
                label.Font = CreateAppearanceFont("Segoe UI", label == lblLegend ? 8F : 8.5F);
            }
            foreach (TableLayoutPanel panel in new[] { namespaceLayout, accessLayout, declaringLayout, returnLayout })
            {
                panel.ColumnCount = 1;
                panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 23F));
                panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
                panel.Padding = new Padding(0, 5, 12, 0);
            }
            foreach (Label label in new[] { lblNamespaceTitle, lblAccessTitle, lblDeclaringTitle, lblReturnTitle })
            {
                label.ForeColor = Muted;
                label.Font = CreateAppearanceFont("Segoe UI", 8.5F);
                label.Dock = DockStyle.Fill;
            }
            foreach (Label label in new[] { lblNamespaceValue, lblAccessValue, lblDeclaringValue, lblReturnValue })
            {
                label.Dock = DockStyle.Fill;
                label.AutoEllipsis = true;
            }
            StyleButton(btOpen, true);
            StyleButton(btCopy, false);
            StyleButton(btReset, false);
            statusStrip.BackColor = Surface;
            statusStrip.ForeColor = Muted;
            statusStrip.RenderMode = ToolStripRenderMode.System;
            lblCounts.ForeColor = Muted;
            toolTip.SetToolTip(chkHideGenerated, "Hide auto-property backing fields and other compiler-generated members and types.");
            toolTip.SetToolTip(cmbAccess, "Filter member visibility. A property uses the visibility of its most accessible accessor.");
            toolTip.SetToolTip(txtSearch, "Search type names, namespaces, member names, or parameter type names.");
            AddNodeIcon("assembly", "A", Accent);
            AddNodeIcon("folder", "N", Muted);
            AddNodeIcon("type", "T", Color.FromArgb(158, 122, 62));
            AddNodeIcon("property", "P", Color.FromArgb(83, 129, 159));
            AddNodeIcon("field", "F", Color.FromArgb(114, 143, 86));
            AddNodeIcon("method", "M", PrivateColor);
        }

        private static void StyleButton(Button button, bool primary)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.BackColor = primary ? Accent : Surface;
            button.ForeColor = primary ? Color.White : Accent;
            button.FlatAppearance.MouseOverBackColor = primary ? Color.FromArgb(20, 103, 89) : Selected;
            button.Cursor = Cursors.Hand;
        }

        private void AddNodeIcon(string key, string letter, Color color)
        {
            using (Bitmap bitmap = new Bitmap(18, 18))
            using (Graphics graphics = Graphics.FromImage(bitmap))
            using (Brush brush = new SolidBrush(Color.FromArgb(26, color)))
            using (Font font = new Font("Consolas", 9F, FontStyle.Bold))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.FillEllipse(brush, 0, 0, 17, 17);
                TextRenderer.DrawText(graphics, letter, font, new Rectangle(0, 0, 18, 18), color,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                nodeImages.Images.Add(key, (Bitmap)bitmap.Clone());
            }
        }

        private static Color GetAccessColor(string access)
        {
            if (access == "private") return PrivateColor;
            if (access == "public") return Accent;
            if (access.Contains("protected")) return Color.FromArgb(155, 113, 45);
            return Color.FromArgb(79, 119, 155);
        }

        private void treeView1_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            if (e.Bounds.Height == 0) return;
            Rectangle bounds = new Rectangle(e.Bounds.X, e.Bounds.Y,
                Math.Max(1, treeView1.ClientSize.Width - e.Bounds.X), e.Bounds.Height);
            bool selected = e.Node == treeView1.SelectedNode;
            using (Brush brush = new SolidBrush(selected ? Selected : treeView1.BackColor))
                e.Graphics.FillRectangle(brush, bounds);
            TextRenderer.DrawText(e.Graphics, e.Node.Text, treeView1.Font, bounds,
                e.Node.ForeColor.IsEmpty ? Ink : e.Node.ForeColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine);
            if ((e.State & TreeNodeStates.Focused) != 0 && treeView1.Focused)
                ControlPaint.DrawFocusRectangle(e.Graphics, bounds, Accent, Selected);
        }

        private void signaturePanel_Paint(object sender, PaintEventArgs e)
        {
            using (Pen pen = new Pen(Color.FromArgb(224, 234, 228)))
                e.Graphics.DrawRectangle(pen, 0, 0, signaturePanel.Width - 1, signaturePanel.Height - 1);
        }

        private void HighlightSignature()
        {
            txtSignature.SelectAll();
            txtSignature.SelectionColor = Ink;
            ColorTokens(@"\b(public|internal)\b", Accent);
            ColorTokens(@"\b(private|static|readonly|const|abstract|virtual|override|sealed|class|struct|interface|enum|delegate|get|set|ref|out|params)\b", PrivateColor);
            ColorTokens(@"\bprotected\b", Color.FromArgb(155, 113, 45));
            ColorTokens(@"\b(void|bool|byte|sbyte|short|ushort|int|uint|long|ulong|float|double|decimal|string|char|object)\b", Color.FromArgb(43, 133, 135));
            txtSignature.Select(0, 0);
        }

        private void ColorTokens(string pattern, Color color)
        {
            foreach (Match match in Regex.Matches(txtSignature.Text, pattern))
            {
                txtSignature.Select(match.Index, match.Length);
                txtSignature.SelectionColor = color;
            }
        }
    }
}
