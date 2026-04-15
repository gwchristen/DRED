using System.Drawing;
using System.Windows.Forms;

namespace DRED
{
    public static class ThemeManager
    {
        public static readonly Color BackgroundColor    = Color.FromArgb(0x1E, 0x1E, 0x1E);
        public static readonly Color SurfaceColor       = Color.FromArgb(0x2D, 0x2D, 0x30);
        public static readonly Color AccentColor        = Color.FromArgb(0x19, 0x76, 0xD2);
        public static readonly Color TextColor          = Color.FromArgb(0xF1, 0xF1, 0xF1);
        public static readonly Color SecondaryTextColor = Color.FromArgb(0xCC, 0xCC, 0xCC);
        public static readonly Color GridAltRowColor    = Color.FromArgb(0x25, 0x25, 0x28);
        public static readonly Color GridLineColor      = Color.FromArgb(0x3C, 0x3C, 0x3C);
        public static readonly Color InputBackColor     = Color.FromArgb(0x32, 0x32, 0x32);
        public static readonly Color MenuBarColor       = Color.FromArgb(0x2D, 0x2D, 0x30);
        public static readonly Color MenuHighlightColor = Color.FromArgb(0x19, 0x76, 0xD2);
        public static readonly Color MenuBorderColor    = Color.FromArgb(0x3E, 0x3E, 0x42);
        public static readonly Color ToolbarColor       = Color.FromArgb(30, 30, 30);
        public static readonly Color SearchPanelColor   = Color.FromArgb(37, 37, 40);
        public static readonly Color SearchPanelFilteredColor = Color.FromArgb(30, 35, 50);
        public static readonly Color StatusBarColor     = Color.FromArgb(0x2A, 0x2A, 0x2D);
        public static readonly Color StatusBorderColor  = Color.FromArgb(0x3E, 0x3E, 0x42);
        public static readonly Color SplitterColor      = Color.FromArgb(0x3E, 0x3E, 0x42);
        public static readonly Color ButtonPanelColor   = Color.FromArgb(0x25, 0x25, 0x28);
        public static readonly Color FilterLabelColor   = Color.FromArgb(144, 202, 249);
        public static readonly Color TabDividerColor    = Color.FromArgb(55, 55, 55);
        public static readonly Color TabInactiveTextColor = Color.FromArgb(160, 160, 160);
        public static readonly Color FormBackColor      = Color.FromArgb(45, 45, 48);
        public static readonly Color ListBoxDarkColor   = Color.FromArgb(0x1A, 0x1A, 0x1A);

        /// <summary>
        /// Applies a dark theme renderer to the given MenuStrip.
        /// </summary>
        public static void ApplyDarkMenuStrip(MenuStrip menu)
        {
            menu.BackColor = MenuBarColor;
            menu.ForeColor = TextColor;
            menu.Renderer = new DarkMenuRenderer();
        }

        /// <summary>
        /// Custom ToolStrip renderer that paints MenuStrip items with dark colors.
        /// </summary>
        private sealed class DarkMenuRenderer : ToolStripProfessionalRenderer
        {
            public DarkMenuRenderer() : base(new DarkColorTable()) { }

            protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
            {
                var item = e.Item;
                var g = e.Graphics;
                if (item.Selected || item.Pressed)
                {
                    using var brush = new SolidBrush(MenuHighlightColor);
                    g.FillRectangle(brush, new Rectangle(Point.Empty, item.Size));
                }
                else
                {
                    using var brush = new SolidBrush(MenuBarColor);
                    g.FillRectangle(brush, new Rectangle(Point.Empty, item.Size));
                }
            }

            protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
            {
                using var brush = new SolidBrush(MenuBarColor);
                e.Graphics.FillRectangle(brush, e.AffectedBounds);
            }

            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                e.TextColor = e.Item.Enabled ? TextColor : SecondaryTextColor;
                base.OnRenderItemText(e);
            }

            protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
            {
                // Draw a subtle bottom border
                using var pen = new Pen(MenuBorderColor);
                int y = e.AffectedBounds.Bottom - 1;
                e.Graphics.DrawLine(pen, e.AffectedBounds.Left, y, e.AffectedBounds.Right, y);
            }

            protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
            {
                int y = e.Item.Height / 2;
                using var pen = new Pen(MenuBorderColor);
                e.Graphics.DrawLine(pen, 4, y, e.Item.Width - 4, y);
            }

            protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
            {
                e.ArrowColor = TextColor;
                base.OnRenderArrow(e);
            }
        }

        private sealed class DarkColorTable : ProfessionalColorTable
        {
            public override Color MenuItemSelected           => MenuHighlightColor;
            public override Color MenuItemBorder             => MenuHighlightColor;
            public override Color MenuBorder                 => MenuBorderColor;
            public override Color MenuItemSelectedGradientBegin => MenuHighlightColor;
            public override Color MenuItemSelectedGradientEnd   => MenuHighlightColor;
            public override Color MenuItemPressedGradientBegin  => MenuHighlightColor;
            public override Color MenuItemPressedGradientEnd    => MenuHighlightColor;
            public override Color MenuItemPressedGradientMiddle => MenuHighlightColor;
            public override Color ToolStripDropDownBackground   => SurfaceColor;
            public override Color ImageMarginGradientBegin   => SurfaceColor;
            public override Color ImageMarginGradientMiddle  => SurfaceColor;
            public override Color ImageMarginGradientEnd     => SurfaceColor;
            public override Color MenuStripGradientBegin     => MenuBarColor;
            public override Color MenuStripGradientEnd       => MenuBarColor;
            public override Color ToolStripBorder            => MenuBorderColor;
            public override Color ToolStripContentPanelGradientBegin => MenuBarColor;
            public override Color ToolStripContentPanelGradientEnd   => MenuBarColor;
            public override Color SeparatorDark              => MenuBorderColor;
            public override Color SeparatorLight             => MenuBorderColor;
        }

        /// <summary>
        /// Applies dark styling to non-Material controls such as DataGridView,
        /// NumericUpDown, DateTimePicker, and standard ComboBox.
        /// </summary>
        public static void StyleNonMaterialControls(Control parent)
        {
            foreach (Control c in parent.Controls)
            {
                switch (c)
                {
                    case DataGridView dgv:
                        ApplyToDataGridView(dgv);
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
                    case ComboBox cb:
                        cb.BackColor = InputBackColor;
                        cb.ForeColor = TextColor;
                        cb.FlatStyle = FlatStyle.Flat;
                        break;
                }

                if (c.HasChildren)
                    StyleNonMaterialControls(c);
            }
        }

        public static void ApplyToDataGridView(DataGridView dgv)
        {
            dgv.EnableHeadersVisualStyles = false;
            dgv.BackgroundColor = BackgroundColor;
            dgv.GridColor = GridLineColor;
            dgv.BorderStyle = BorderStyle.None;
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;

            dgv.ColumnHeadersDefaultCellStyle.BackColor = BackgroundColor;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = TextColor;
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = BackgroundColor;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 10F);
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;

            dgv.DefaultCellStyle.BackColor = SurfaceColor;
            dgv.DefaultCellStyle.ForeColor = TextColor;
            dgv.DefaultCellStyle.SelectionBackColor = AccentColor;
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F);

            dgv.AlternatingRowsDefaultCellStyle.BackColor = GridAltRowColor;
            dgv.AlternatingRowsDefaultCellStyle.ForeColor = TextColor;
            dgv.AlternatingRowsDefaultCellStyle.SelectionBackColor = AccentColor;
            dgv.AlternatingRowsDefaultCellStyle.SelectionForeColor = Color.White;

            dgv.RowHeadersVisible = false;
            dgv.RowTemplate.Height = 32;
        }
    }
}
