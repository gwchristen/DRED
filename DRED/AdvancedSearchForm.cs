using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

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
        private TextBox txtEst = null!;
        private TextBox txtComments = null!;
        private DateTimePicker dtpPODateFrom = null!;
        private DateTimePicker dtpPODateTo = null!;
        private DateTimePicker dtpRecvDateFrom = null!;
        private DateTimePicker dtpRecvDateTo = null!;
        private CheckBox chkPODateFrom = null!;
        private CheckBox chkPODateTo = null!;
        private CheckBox chkRecvDateFrom = null!;
        private CheckBox chkRecvDateTo = null!;
        private NumericUpDown nudCostMin = null!;
        private NumericUpDown nudCostMax = null!;
        private NumericUpDown nudQtyMin = null!;
        private NumericUpDown nudQtyMax = null!;
        private CheckBox chkCostRange = null!;
        private CheckBox chkQtyRange = null!;
        private Button btnApply = null!;
        private Button btnClear = null!;
        private Button btnClose = null!;

        public AdvancedSearchForm()
        {
            InitializeAdvancedForm();
            ThemeManager.Apply(this);
        }

        private void InitializeAdvancedForm()
        {
            this.Text = "Advanced Search";
            this.Size = new Size(700, 600);
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new Size(600, 550);

            var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            var tlp = new TableLayoutPanel
            {
                ColumnCount = 4,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Top,
                Padding = new Padding(10),
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
            AddSearchRow(tlp, row++, "Pur. Code:", out txtPurCode, "Est.:", out txtEst);

            // Comments spans full width
            var lblComments = new Label { Text = "Comments:", AutoSize = true, Anchor = AnchorStyles.Left | AnchorStyles.Top, Margin = new Padding(0, 6, 0, 0) };
            txtComments = new TextBox { Dock = DockStyle.Fill, Margin = new Padding(0, 4, 6, 4) };
            tlp.Controls.Add(lblComments, 0, row);
            tlp.Controls.Add(txtComments, 1, row);
            tlp.SetColumnSpan(txtComments, 3);
            row++;

            // PO Date range
            chkPODateFrom = new CheckBox { Text = "PO Date From:", AutoSize = true, Anchor = AnchorStyles.Left };
            dtpPODateFrom = new DateTimePicker { Format = DateTimePickerFormat.Short, Enabled = false, Dock = DockStyle.Fill };
            chkPODateTo = new CheckBox { Text = "PO Date To:", AutoSize = true, Anchor = AnchorStyles.Left };
            dtpPODateTo = new DateTimePicker { Format = DateTimePickerFormat.Short, Enabled = false, Dock = DockStyle.Fill };
            chkPODateFrom.CheckedChanged += (s, e) => dtpPODateFrom.Enabled = chkPODateFrom.Checked;
            chkPODateTo.CheckedChanged += (s, e) => dtpPODateTo.Enabled = chkPODateTo.Checked;
            tlp.Controls.Add(chkPODateFrom, 0, row); tlp.Controls.Add(dtpPODateFrom, 1, row);
            tlp.Controls.Add(chkPODateTo, 2, row); tlp.Controls.Add(dtpPODateTo, 3, row);
            row++;

            // Recv Date range
            chkRecvDateFrom = new CheckBox { Text = "Recv Date From:", AutoSize = true, Anchor = AnchorStyles.Left };
            dtpRecvDateFrom = new DateTimePicker { Format = DateTimePickerFormat.Short, Enabled = false, Dock = DockStyle.Fill };
            chkRecvDateTo = new CheckBox { Text = "Recv Date To:", AutoSize = true, Anchor = AnchorStyles.Left };
            dtpRecvDateTo = new DateTimePicker { Format = DateTimePickerFormat.Short, Enabled = false, Dock = DockStyle.Fill };
            chkRecvDateFrom.CheckedChanged += (s, e) => dtpRecvDateFrom.Enabled = chkRecvDateFrom.Checked;
            chkRecvDateTo.CheckedChanged += (s, e) => dtpRecvDateTo.Enabled = chkRecvDateTo.Checked;
            tlp.Controls.Add(chkRecvDateFrom, 0, row); tlp.Controls.Add(dtpRecvDateFrom, 1, row);
            tlp.Controls.Add(chkRecvDateTo, 2, row); tlp.Controls.Add(dtpRecvDateTo, 3, row);
            row++;

            // Cost range
            chkCostRange = new CheckBox { Text = "Cost Min:", AutoSize = true, Anchor = AnchorStyles.Left };
            nudCostMin = new NumericUpDown { Minimum = 0, Maximum = 9999999, DecimalPlaces = 2, Dock = DockStyle.Fill, Enabled = false };
            var lblCostMax = new Label { Text = "Cost Max:", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 6, 0, 0) };
            nudCostMax = new NumericUpDown { Minimum = 0, Maximum = 9999999, DecimalPlaces = 2, Dock = DockStyle.Fill, Enabled = false };
            chkCostRange.CheckedChanged += (s, e) => { nudCostMin.Enabled = chkCostRange.Checked; nudCostMax.Enabled = chkCostRange.Checked; };
            tlp.Controls.Add(chkCostRange, 0, row); tlp.Controls.Add(nudCostMin, 1, row);
            tlp.Controls.Add(lblCostMax, 2, row); tlp.Controls.Add(nudCostMax, 3, row);
            row++;

            // Qty range
            chkQtyRange = new CheckBox { Text = "Qty Min:", AutoSize = true, Anchor = AnchorStyles.Left };
            nudQtyMin = new NumericUpDown { Minimum = 0, Maximum = 999999, Dock = DockStyle.Fill, Enabled = false };
            var lblQtyMax = new Label { Text = "Qty Max:", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 6, 0, 0) };
            nudQtyMax = new NumericUpDown { Minimum = 0, Maximum = 999999, Dock = DockStyle.Fill, Enabled = false };
            chkQtyRange.CheckedChanged += (s, e) => { nudQtyMin.Enabled = chkQtyRange.Checked; nudQtyMax.Enabled = chkQtyRange.Checked; };
            tlp.Controls.Add(chkQtyRange, 0, row); tlp.Controls.Add(nudQtyMin, 1, row);
            tlp.Controls.Add(lblQtyMax, 2, row); tlp.Controls.Add(nudQtyMax, 3, row);

            scroll.Controls.Add(tlp);

            // Button panel
            var pnlBtn = new Panel { Dock = DockStyle.Bottom, Height = 45 };
            btnApply = new Button { Text = "Apply", Location = new Point(8, 8), Size = new Size(90, 28) };
            btnClear = new Button { Text = "Clear All", Location = new Point(106, 8), Size = new Size(90, 28) };
            btnClose = new Button { Text = "Close", Location = new Point(204, 8), Size = new Size(90, 28) };
            btnApply.Click += BtnApply_Click;
            btnClear.Click += BtnClear_Click;
            btnClose.Click += (s, e) => this.Close();
            pnlBtn.Controls.AddRange(new Control[] { btnApply, btnClear, btnClose });

            this.Controls.Add(scroll);
            this.Controls.Add(pnlBtn);
            this.CancelButton = btnClose;
        }

        private void AddSearchRow(TableLayoutPanel tlp, int row, string lbl1Text, out TextBox txt1, string lbl2Text, out TextBox txt2)
        {
            var lbl1 = new Label { Text = lbl1Text, AutoSize = true, Anchor = AnchorStyles.Left | AnchorStyles.Top, Margin = new Padding(0, 6, 0, 0) };
            txt1 = new TextBox { Dock = DockStyle.Fill, Margin = new Padding(0, 4, 6, 4) };
            var lbl2 = new Label { Text = lbl2Text, AutoSize = true, Anchor = AnchorStyles.Left | AnchorStyles.Top, Margin = new Padding(0, 6, 0, 0) };
            txt2 = new TextBox { Dock = DockStyle.Fill, Margin = new Padding(0, 4, 6, 4) };
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
                Est      = NullIfEmpty(txtEst.Text),
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
                                        txtPONumber, txtVintage, txtCID, txtMENumber, txtPurCode, txtEst, txtComments })
                txt.Text = "";
            chkPODateFrom.Checked = chkPODateTo.Checked = false;
            chkRecvDateFrom.Checked = chkRecvDateTo.Checked = false;
            chkCostRange.Checked = false;
            chkQtyRange.Checked = false;
            Criteria = null;
        }

        private static string? NullIfEmpty(string s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
    }

    public class AdvancedSearchCriteria
    {
        public string? OpCo2 { get; set; }
        public string? Status { get; set; }
        public string? MFR { get; set; }
        public string? DevCode { get; set; }
        public string? BegSer { get; set; }
        public string? EndSer { get; set; }
        public string? PONumber { get; set; }
        public string? Vintage { get; set; }
        public string? CID { get; set; }
        public string? MENumber { get; set; }
        public string? PurCode { get; set; }
        public string? Est { get; set; }
        public string? Comments { get; set; }
        public DateTime? PODateFrom { get; set; }
        public DateTime? PODateTo { get; set; }
        public DateTime? RecvDateFrom { get; set; }
        public DateTime? RecvDateTo { get; set; }
        public decimal? CostMin { get; set; }
        public decimal? CostMax { get; set; }
        public int? QtyMin { get; set; }
        public int? QtyMax { get; set; }
    }
}
