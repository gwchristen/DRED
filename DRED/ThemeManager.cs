using System.Drawing;
using System.Windows.Forms;

namespace DRED
{
    public static class ThemeManager
    {
        public static readonly Color BackgroundColor    = Color.FromArgb(0x1E, 0x1E, 0x1E);
        public static readonly Color SurfaceColor       = Color.FromArgb(0x3E, 0x3E, 0x42);
        public static readonly Color AccentColor        = Color.FromArgb(0x00, 0x7A, 0xCC);
        public static readonly Color TextColor          = Color.FromArgb(0xF1, 0xF1, 0xF1);
        public static readonly Color SecondaryTextColor = Color.FromArgb(0xCC, 0xCC, 0xCC);
        public static readonly Color GridAltRowColor    = Color.FromArgb(0x2D, 0x2D, 0x30);
        public static readonly Color GridLineColor      = Color.FromArgb(0x55, 0x55, 0x55);
        public static readonly Color InputBackColor     = Color.FromArgb(0x2D, 0x2D, 0x30);
        public static readonly Color DisabledTextColor  = Color.FromArgb(0x88, 0x88, 0x88);

        public static void Apply(Form form)
        {
            form.BackColor = BackgroundColor;
            form.ForeColor = TextColor;
            ApplyToControls(form.Controls);
        }

        private static void ApplyToControls(Control.ControlCollection controls)
        {
            foreach (Control c in controls)
            {
                ApplyToControl(c);
                if (c.HasChildren)
                    ApplyToControls(c.Controls);
            }
        }

        private static void ApplyToControl(Control c)
        {
            c.ForeColor = TextColor;

            switch (c)
            {
                case MenuStrip ms:
                    ms.BackColor = SurfaceColor;
                    ms.ForeColor = TextColor;
                    ms.Renderer = new ToolStripProfessionalRenderer(new DarkColorTable());
                    break;

                case StatusStrip ss:
                    ss.BackColor = SurfaceColor;
                    ss.ForeColor = TextColor;
                    ss.Renderer = new ToolStripProfessionalRenderer(new DarkColorTable());
                    foreach (ToolStripItem item in ss.Items)
                        ApplyToToolStripItem(item);
                    break;

                case ToolStrip ts:
                    ts.BackColor = SurfaceColor;
                    ts.ForeColor = TextColor;
                    ts.Renderer = new ToolStripProfessionalRenderer(new DarkColorTable());
                    foreach (ToolStripItem item in ts.Items)
                        ApplyToToolStripItem(item);
                    break;

                case DataGridView dgv:
                    ApplyToDataGridView(dgv);
                    break;

                case TabControl tc:
                    tc.BackColor = BackgroundColor;
                    foreach (TabPage tp in tc.TabPages)
                    {
                        tp.BackColor = BackgroundColor;
                        tp.ForeColor = TextColor;
                    }
                    break;

                case TextBox tb:
                    tb.BackColor = InputBackColor;
                    tb.ForeColor = TextColor;
                    tb.BorderStyle = BorderStyle.FixedSingle;
                    break;

                case ComboBox cb:
                    cb.BackColor = InputBackColor;
                    cb.ForeColor = TextColor;
                    cb.FlatStyle = FlatStyle.Flat;
                    break;

                case NumericUpDown nud:
                    nud.BackColor = InputBackColor;
                    nud.ForeColor = TextColor;
                    break;

                case DateTimePicker dtp:
                    dtp.BackColor = InputBackColor;
                    dtp.ForeColor = TextColor;
                    dtp.CalendarMonthBackground = SurfaceColor;
                    dtp.CalendarForeColor = TextColor;
                    break;

                case CheckBox chk:
                    chk.BackColor = Color.Transparent;
                    chk.ForeColor = TextColor;
                    chk.FlatStyle = FlatStyle.Flat;
                    break;

                case Button btn:
                    btn.BackColor = AccentColor;
                    btn.ForeColor = TextColor;
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderColor = AccentColor;
                    break;

                case TableLayoutPanel tlp:
                    tlp.BackColor = BackgroundColor;
                    break;

                case FlowLayoutPanel flp:
                    flp.BackColor = BackgroundColor;
                    break;

                case Panel pnl:
                    pnl.BackColor = BackgroundColor;
                    break;

                case GroupBox gb:
                    gb.BackColor = BackgroundColor;
                    gb.ForeColor = TextColor;
                    break;

                case Label lbl:
                    lbl.BackColor = Color.Transparent;
                    lbl.ForeColor = TextColor;
                    break;

                case SplitContainer sc:
                    sc.BackColor = BackgroundColor;
                    break;

                default:
                    c.BackColor = BackgroundColor;
                    break;
            }
        }

        private static void ApplyToToolStripItem(ToolStripItem item)
        {
            item.BackColor = SurfaceColor;
            item.ForeColor = TextColor;
        }

        public static void ApplyToDataGridView(DataGridView dgv)
        {
            dgv.BackgroundColor = BackgroundColor;
            dgv.ForeColor = TextColor;
            dgv.GridColor = GridLineColor;
            dgv.BorderStyle = BorderStyle.None;

            dgv.DefaultCellStyle.BackColor = BackgroundColor;
            dgv.DefaultCellStyle.ForeColor = TextColor;
            dgv.DefaultCellStyle.SelectionBackColor = AccentColor;
            dgv.DefaultCellStyle.SelectionForeColor = TextColor;
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 9f);

            dgv.AlternatingRowsDefaultCellStyle.BackColor = GridAltRowColor;
            dgv.AlternatingRowsDefaultCellStyle.ForeColor = TextColor;
            dgv.AlternatingRowsDefaultCellStyle.SelectionBackColor = AccentColor;
            dgv.AlternatingRowsDefaultCellStyle.SelectionForeColor = TextColor;

            dgv.ColumnHeadersDefaultCellStyle.BackColor = SurfaceColor;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = TextColor;
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = SurfaceColor;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            dgv.EnableHeadersVisualStyles = false;

            dgv.RowHeadersDefaultCellStyle.BackColor = SurfaceColor;
            dgv.RowHeadersDefaultCellStyle.ForeColor = TextColor;
        }
    }

    public class DarkColorTable : ProfessionalColorTable
    {
        public override Color MenuStripGradientBegin => ThemeManager.SurfaceColor;
        public override Color MenuStripGradientEnd => ThemeManager.SurfaceColor;
        public override Color ToolStripGradientBegin => ThemeManager.SurfaceColor;
        public override Color ToolStripGradientMiddle => ThemeManager.SurfaceColor;
        public override Color ToolStripGradientEnd => ThemeManager.SurfaceColor;
        public override Color ToolStripBorder => ThemeManager.SurfaceColor;
        public override Color ToolStripContentPanelGradientBegin => ThemeManager.BackgroundColor;
        public override Color ToolStripContentPanelGradientEnd => ThemeManager.BackgroundColor;
        public override Color ButtonSelectedHighlight => ThemeManager.AccentColor;
        public override Color ButtonSelectedHighlightBorder => ThemeManager.AccentColor;
        public override Color ButtonPressedHighlight => ThemeManager.AccentColor;
        public override Color ButtonCheckedHighlight => ThemeManager.AccentColor;
        public override Color MenuItemSelected => ThemeManager.AccentColor;
        public override Color MenuItemSelectedGradientBegin => ThemeManager.AccentColor;
        public override Color MenuItemSelectedGradientEnd => ThemeManager.AccentColor;
        public override Color MenuItemPressedGradientBegin => ThemeManager.AccentColor;
        public override Color MenuItemPressedGradientEnd => ThemeManager.AccentColor;
        public override Color MenuItemBorder => ThemeManager.AccentColor;
        public override Color MenuBorder => ThemeManager.SurfaceColor;
        public override Color MenuItemPressedGradientMiddle => ThemeManager.AccentColor;
        public override Color SeparatorLight => ThemeManager.GridLineColor;
        public override Color SeparatorDark => ThemeManager.GridLineColor;
        public override Color StatusStripGradientBegin => ThemeManager.SurfaceColor;
        public override Color StatusStripGradientEnd => ThemeManager.SurfaceColor;
        public override Color ButtonSelectedGradientBegin => ThemeManager.AccentColor;
        public override Color ButtonSelectedGradientEnd => ThemeManager.AccentColor;
        public override Color ButtonSelectedGradientMiddle => ThemeManager.AccentColor;
        public override Color ButtonSelectedBorder => ThemeManager.AccentColor;
        public override Color ButtonPressedGradientBegin => ThemeManager.AccentColor;
        public override Color ButtonPressedGradientEnd => ThemeManager.AccentColor;
        public override Color ButtonPressedGradientMiddle => ThemeManager.AccentColor;
        public override Color ButtonPressedBorder => ThemeManager.AccentColor;
        public override Color CheckBackground => ThemeManager.AccentColor;
        public override Color CheckPressedBackground => ThemeManager.AccentColor;
        public override Color CheckSelectedBackground => ThemeManager.AccentColor;
        public override Color ImageMarginGradientBegin => ThemeManager.SurfaceColor;
        public override Color ImageMarginGradientMiddle => ThemeManager.SurfaceColor;
        public override Color ImageMarginGradientEnd => ThemeManager.SurfaceColor;
        public override Color ToolStripDropDownBackground => ThemeManager.SurfaceColor;
        public override Color OverflowButtonGradientBegin => ThemeManager.SurfaceColor;
        public override Color OverflowButtonGradientMiddle => ThemeManager.SurfaceColor;
        public override Color OverflowButtonGradientEnd => ThemeManager.SurfaceColor;
        public override Color RaftingContainerGradientBegin => ThemeManager.SurfaceColor;
        public override Color RaftingContainerGradientEnd => ThemeManager.SurfaceColor;
    }
}
