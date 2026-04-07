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
