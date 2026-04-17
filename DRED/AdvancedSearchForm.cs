using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using MaterialSkin.Controls;

namespace DRED
{
    public class AdvancedSearchForm : Form
    {
        public AdvancedSearchCriteria? Criteria { get; private set; }

        private TextBox txtOpCo2 = null!;
        private TextBox txtStatus = null!;
        private TextBox txtMFR = null!;
        private TextBox txtDevCode = null!;
        private TextBox txtBegSer = null!;
        private TextBox txtEndSer = null!;
        private TextBox txtPONumber = null!;
        private TextBox txtVintage = null!;
        private TextBox txtCID = null!;
        private TextBox txtMENumber = null!;
        private TextBox txtPurCode = null!;
        private MaterialCheckbox chkEstFilter = null!;
        private MaterialCheckbox chkTextFileFilter = null!;
        private RichTextBox txtComments = null!;
        private DateTimePicker dtpPODateFrom = null!;
        private DateTimePicker dtpPODateTo = null!;
        private DateTimePicker dtpRecvDateFrom = null!;
        private DateTimePicker dtpRecvDateTo = null!;
        private MaterialCheckbox chkPODateFrom = null!;
        private MaterialCheckbox chkPODateTo = null!;
        private MaterialCheckbox chkRecvDateFrom = null!;
        private MaterialCheckbox chkRecvDateTo = null!;
        private NumericUpDown nudCostMin = null!;
        private NumericUpDown nudCostMax = null!;
        private NumericUpDown nudQtyMin = null!;
        private NumericUpDown nudQtyMax = null!;
        private MaterialCheckbox chkCostRange = null!;
        private MaterialCheckbox chkQtyRange = null!;
        private MaterialButton btnApply = null!;
        private MaterialButton btnClear = null!;
        private MaterialButton btnClose = null!;

        public AdvancedSearchForm()
        {
            InitializeAdvancedForm();
        }

        private void InitializeAdvancedForm()
        {
            this.Text = "Advanced Search";
            this.Size = new Size(700, 664);
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new Size(600, 614);

            this.BackColor = ThemeManager.FormBackColor;
            this.ForeColor = ThemeManager.TextColor;

            var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = ThemeManager.FormBackColor };
            var tlp = new TableLayoutPanel
            {
                ColumnCount = 4,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Top,
                Padding = new Padding(8, 6, 8, 6),
                BackColor = ThemeManager.FormBackColor,
            };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            int row = 0;

            AddSearchRow(tlp, row++, "OpCo2:", out txtOpCo2, "DevCode:", out txtDevCode);
            AddSearchRow(tlp, row++, "Status:", out txtStatus, "MFR:", out txtMFR);
            AddSearchRow(tlp, row++, "Beg Ser:", out txtBegSer, "End Ser:", out txtEndSer);
            AddSearchRow(tlp, row++, "PO Number:", out txtPONumber, "Vintage:", out txtVintage);
            AddSearchRow(tlp, row++, "CID:", out txtCID, "M.E. #:", out txtMENumber);
            AddSearchRow(tlp, row++, "Pur. Code:", out txtPurCode, "", out _);

            // Est and TextFile checkboxes (full-width rows)
            chkEstFilter = new MaterialCheckbox { Text = "Established", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 4, 0, 4) };
            tlp.Controls.Add(chkEstFilter, 0, row);
            tlp.SetColumnSpan(chkEstFilter, 2);
            chkTextFileFilter = new MaterialCheckbox { Text = "Text File", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 4, 0, 4) };
            tlp.Controls.Add(chkTextFileFilter, 2, row);
            tlp.SetColumnSpan(chkTextFileFilter, 2);
            row++;

            // Comments spans full width
            var lblComments = MakeLabel("Comments:");
            txtComments = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 2, 4, 2),
                BackColor = ThemeManager.InputBackColor,
                ForeColor = ThemeManager.TextColor,
                Height = 50,
            };
            tlp.Controls.Add(lblComments, 0, row);
            tlp.Controls.Add(txtComments, 1, row);
            tlp.SetColumnSpan(txtComments, 3);
            row++;

            // PO Date range
            chkPODateFrom = new MaterialCheckbox { Text = "PO Date From:", AutoSize = true, Anchor = AnchorStyles.Left };
            dtpPODateFrom = new DateTimePicker { Format = DateTimePickerFormat.Short, Enabled = false, Dock = DockStyle.Fill, BackColor = ThemeManager.InputBackColor, ForeColor = Color.White };
            chkPODateTo = new MaterialCheckbox { Text = "PO Date To:", AutoSize = true, Anchor = AnchorStyles.Left };
            dtpPODateTo = new DateTimePicker { Format = DateTimePickerFormat.Short, Enabled = false, Dock = DockStyle.Fill, BackColor = ThemeManager.InputBackColor, ForeColor = Color.White };
            chkPODateFrom.CheckedChanged += (s, e) => dtpPODateFrom.Enabled = chkPODateFrom.Checked;
            chkPODateTo.CheckedChanged += (s, e) => dtpPODateTo.Enabled = chkPODateTo.Checked;
            tlp.Controls.Add(chkPODateFrom, 0, row); tlp.Controls.Add(dtpPODateFrom, 1, row);
            tlp.Controls.Add(chkPODateTo, 2, row); tlp.Controls.Add(dtpPODateTo, 3, row);
            row++;

            // Recv Date range
            chkRecvDateFrom = new MaterialCheckbox { Text = "Est Date From:", AutoSize = true, Anchor = AnchorStyles.Left };
            dtpRecvDateFrom = new DateTimePicker { Format = DateTimePickerFormat.Short, Enabled = false, Dock = DockStyle.Fill, BackColor = ThemeManager.InputBackColor, ForeColor = Color.White };
            chkRecvDateTo = new MaterialCheckbox { Text = "Est Date To:", AutoSize = true, Anchor = AnchorStyles.Left };
            dtpRecvDateTo = new DateTimePicker { Format = DateTimePickerFormat.Short, Enabled = false, Dock = DockStyle.Fill, BackColor = ThemeManager.InputBackColor, ForeColor = Color.White };
            chkRecvDateFrom.CheckedChanged += (s, e) => dtpRecvDateFrom.Enabled = chkRecvDateFrom.Checked;
            chkRecvDateTo.CheckedChanged += (s, e) => dtpRecvDateTo.Enabled = chkRecvDateTo.Checked;
            tlp.Controls.Add(chkRecvDateFrom, 0, row); tlp.Controls.Add(dtpRecvDateFrom, 1, row);
            tlp.Controls.Add(chkRecvDateTo, 2, row); tlp.Controls.Add(dtpRecvDateTo, 3, row);
            row++;

            // Cost range
            chkCostRange = new MaterialCheckbox { Text = "Cost Min:", AutoSize = true, Anchor = AnchorStyles.Left };
            nudCostMin = MakeNud(9999999, 2, false);
            var lblCostMax = MakeLabel("Cost Max:");
            nudCostMax = MakeNud(9999999, 2, false);
            chkCostRange.CheckedChanged += (s, e) => { nudCostMin.Enabled = chkCostRange.Checked; nudCostMax.Enabled = chkCostRange.Checked; };
            tlp.Controls.Add(chkCostRange, 0, row); tlp.Controls.Add(nudCostMin, 1, row);
            tlp.Controls.Add(lblCostMax, 2, row); tlp.Controls.Add(nudCostMax, 3, row);
            row++;

            // Qty range
            chkQtyRange = new MaterialCheckbox { Text = "Qty Min:", AutoSize = true, Anchor = AnchorStyles.Left };
            nudQtyMin = MakeNud(999999, 0, false);
            var lblQtyMax = MakeLabel("Qty Max:");
            nudQtyMax = MakeNud(999999, 0, false);
            chkQtyRange.CheckedChanged += (s, e) => { nudQtyMin.Enabled = chkQtyRange.Checked; nudQtyMax.Enabled = chkQtyRange.Checked; };
            tlp.Controls.Add(chkQtyRange, 0, row); tlp.Controls.Add(nudQtyMin, 1, row);
            tlp.Controls.Add(lblQtyMax, 2, row); tlp.Controls.Add(nudQtyMax, 3, row);

            scroll.Controls.Add(tlp);

            // Button panel
            var pnlBtn = new Panel { Dock = DockStyle.Bottom, Height = 46, BackColor = ThemeManager.SearchPanelColor };
            btnApply = new MaterialButton { Text = "Apply", Location = new Point(8, 6), Type = MaterialButton.MaterialButtonType.Contained, HighEmphasis = true, AutoSize = true };
            btnClear = new MaterialButton { Text = "Clear All", Location = new Point(120, 6), Type = MaterialButton.MaterialButtonType.Outlined, AutoSize = true };
            btnClose = new MaterialButton { Text = "Close", Location = new Point(230, 6), Type = MaterialButton.MaterialButtonType.Text, AutoSize = true };
            btnApply.Click += BtnApply_Click;
            btnClear.Click += BtnClear_Click;
            btnClose.Click += (s, e) => this.Close();
            pnlBtn.Controls.AddRange(new Control[] { btnApply, btnClear, btnClose });

            this.Controls.Add(scroll);
            this.Controls.Add(pnlBtn);
            this.CancelButton = btnClose;
        }

        private static Label MakeLabel(string text) =>
            new Label
            {
                Text = text,
                AutoSize = true,
                Anchor = AnchorStyles.Left | AnchorStyles.Top,
                Margin = new Padding(0, 6, 0, 0),
                ForeColor = ThemeManager.SecondaryTextColor,
            };

        private static NumericUpDown MakeNud(decimal max, int decimals, bool enabled) =>
            new NumericUpDown
            {
                Minimum = 0,
                Maximum = max,
                DecimalPlaces = decimals,
                Dock = DockStyle.Fill,
                Enabled = enabled,
                BackColor = ThemeManager.InputBackColor,
                ForeColor = Color.White,
            };

        private void AddSearchRow(TableLayoutPanel tlp, int row, string lbl1Text, out TextBox txt1, string lbl2Text, out TextBox txt2)
        {
            var lbl1 = MakeLabel(lbl1Text);
            txt1 = new TextBox { Dock = DockStyle.Fill, Margin = new Padding(0, 2, 4, 2), BackColor = ThemeManager.InputBackColor, ForeColor = ThemeManager.TextColor, BorderStyle = BorderStyle.FixedSingle };
            var lbl2 = MakeLabel(lbl2Text);
            txt2 = new TextBox { Dock = DockStyle.Fill, Margin = new Padding(0, 2, 4, 2), BackColor = ThemeManager.InputBackColor, ForeColor = ThemeManager.TextColor, BorderStyle = BorderStyle.FixedSingle };
            tlp.Controls.Add(lbl1, 0, row);
            tlp.Controls.Add(txt1, 1, row);
            tlp.Controls.Add(lbl2, 2, row);
            tlp.Controls.Add(txt2, 3, row);
        }

        private void BtnApply_Click(object? sender, EventArgs e)
        {
            Criteria = new AdvancedSearchCriteria
            {
                OpCo2    = NullIfEmpty(txtOpCo2.Text),
                Status   = NullIfEmpty(txtStatus.Text),
                MFR      = NullIfEmpty(txtMFR.Text),
                DevCode  = NullIfEmpty(txtDevCode.Text),
                BegSer   = NullIfEmpty(txtBegSer.Text),
                EndSer   = NullIfEmpty(txtEndSer.Text),
                PONumber = NullIfEmpty(txtPONumber.Text),
                Vintage  = NullIfEmpty(txtVintage.Text),
                CID      = NullIfEmpty(txtCID.Text),
                MENumber = NullIfEmpty(txtMENumber.Text),
                PurCode  = NullIfEmpty(txtPurCode.Text),
                Est      = chkEstFilter.Checked ? (bool?)true : null,
                TextFile = chkTextFileFilter.Checked ? (bool?)true : null,
                Comments = NullIfEmpty(txtComments.Text),
                PODateFrom   = chkPODateFrom.Checked ? dtpPODateFrom.Value.Date : (DateTime?)null,
                PODateTo     = chkPODateTo.Checked ? dtpPODateTo.Value.Date : (DateTime?)null,
                RecvDateFrom = chkRecvDateFrom.Checked ? dtpRecvDateFrom.Value.Date : (DateTime?)null,
                RecvDateTo   = chkRecvDateTo.Checked ? dtpRecvDateTo.Value.Date : (DateTime?)null,
                CostMin = chkCostRange.Checked ? nudCostMin.Value : (decimal?)null,
                CostMax = chkCostRange.Checked ? nudCostMax.Value : (decimal?)null,
                QtyMin  = chkQtyRange.Checked ? (int)nudQtyMin.Value : (int?)null,
                QtyMax  = chkQtyRange.Checked ? (int)nudQtyMax.Value : (int?)null,
            };
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void BtnClear_Click(object? sender, EventArgs e)
        {
            foreach (var txt in new[] { txtOpCo2, txtStatus, txtMFR, txtDevCode, txtBegSer, txtEndSer,
                                        txtPONumber, txtVintage, txtCID, txtMENumber, txtPurCode })
                txt.Text = "";
            txtComments.Text = "";
            chkEstFilter.Checked = false;
            chkTextFileFilter.Checked = false;
            chkPODateFrom.Checked = chkPODateTo.Checked = false;
            chkRecvDateFrom.Checked = chkRecvDateTo.Checked = false;
            chkCostRange.Checked = false;
            chkQtyRange.Checked = false;
            Criteria = null;
        }

        private static string? NullIfEmpty(string s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
    }
}
