namespace DRED
{
    partial class RecordForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.tableLayoutPanel  = new System.Windows.Forms.TableLayoutPanel();
            this.lblOpCo2          = new System.Windows.Forms.Label();
            this.txtOpCo2          = new System.Windows.Forms.TextBox();
            this.lblStatus         = new System.Windows.Forms.Label();
            this.txtStatus         = new System.Windows.Forms.TextBox();
            this.lblMFR            = new System.Windows.Forms.Label();
            this.txtMFR            = new System.Windows.Forms.TextBox();
            this.lblDevCode        = new System.Windows.Forms.Label();
            this.txtDevCode        = new System.Windows.Forms.TextBox();
            this.lblBegSer         = new System.Windows.Forms.Label();
            this.txtBegSer         = new System.Windows.Forms.TextBox();
            this.lblEndSer         = new System.Windows.Forms.Label();
            this.txtEndSer         = new System.Windows.Forms.TextBox();
            this.lblQty            = new System.Windows.Forms.Label();
            this.nudQty            = new System.Windows.Forms.NumericUpDown();
            this.chkAutoQty        = new System.Windows.Forms.CheckBox();
            this.lblPODate         = new System.Windows.Forms.Label();
            this.dtpPODate         = new System.Windows.Forms.DateTimePicker();
            this.chkNoPODate       = new System.Windows.Forms.CheckBox();
            this.lblVintage        = new System.Windows.Forms.Label();
            this.txtVintage        = new System.Windows.Forms.TextBox();
            this.lblPONumber       = new System.Windows.Forms.Label();
            this.txtPONumber       = new System.Windows.Forms.TextBox();
            this.lblRecvDate       = new System.Windows.Forms.Label();
            this.chkRecvDate       = new System.Windows.Forms.CheckBox();
            this.lblRecvDateDisplay = new System.Windows.Forms.Label();
            this.lblUnitCost       = new System.Windows.Forms.Label();
            this.txtUnitCost       = new System.Windows.Forms.TextBox();
            this.lblCID            = new System.Windows.Forms.Label();
            this.txtCID            = new System.Windows.Forms.TextBox();
            this.lblMENumber       = new System.Windows.Forms.Label();
            this.txtMENumber       = new System.Windows.Forms.TextBox();
            this.lblPurCode        = new System.Windows.Forms.Label();
            this.txtPurCode        = new System.Windows.Forms.TextBox();
            this.lblEst            = new System.Windows.Forms.Label();
            this.txtEst            = new System.Windows.Forms.TextBox();
            this.lblComments       = new System.Windows.Forms.Label();
            this.txtComments       = new System.Windows.Forms.TextBox();
            this.lblAuditInfo      = new System.Windows.Forms.Label();
            this.pnlButtons        = new System.Windows.Forms.Panel();
            this.btnSave           = new System.Windows.Forms.Button();
            this.btnCancel         = new System.Windows.Forms.Button();

            ((System.ComponentModel.ISupportInitialize)(this.nudQty)).BeginInit();
            this.pnlButtons.SuspendLayout();
            this.SuspendLayout();

            // tableLayoutPanel — 4 cols, 12 rows
            this.tableLayoutPanel.ColumnCount = 4;
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 110F));
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 110F));
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel.Padding = new System.Windows.Forms.Padding(8);
            this.tableLayoutPanel.RowCount = 12;
            for (int i = 0; i < 11; i++)
                this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F)); // Row 11: audit
            this.tableLayoutPanel.Name = "tableLayoutPanel";
            this.tableLayoutPanel.TabIndex = 0;

            // Row 0: OpCo2 | Status
            AddLabelControl(0, 0, lblOpCo2, "OpCo2:");
            AddTextControl(0, 1, txtOpCo2, 0);
            AddLabelControl(0, 2, lblStatus, "Status:");
            AddTextControl(0, 3, txtStatus, 1);

            // Row 1: MFR | DevCode
            AddLabelControl(1, 0, lblMFR, "MFR:");
            AddTextControl(1, 1, txtMFR, 2);
            AddLabelControl(1, 2, lblDevCode, "Dev Code:");
            AddTextControl(1, 3, txtDevCode, 3);

            // Row 2: BegSer | EndSer
            AddLabelControl(2, 0, lblBegSer, "Beg Ser:");
            AddTextControl(2, 1, txtBegSer, 4);
            this.txtBegSer.TextChanged += new System.EventHandler(this.SerialField_TextChanged);
            AddLabelControl(2, 2, lblEndSer, "End Ser:");
            AddTextControl(2, 3, txtEndSer, 5);
            this.txtEndSer.TextChanged += new System.EventHandler(this.SerialField_TextChanged);

            // Row 3: Qty (+Auto) | PO Date (+No PO Date)
            AddLabelControl(3, 0, lblQty, "Qty:");
            var qtyPanel = new System.Windows.Forms.FlowLayoutPanel
            {
                AutoSize = true,
                Dock = System.Windows.Forms.DockStyle.Fill,
                FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight,
            };
            this.nudQty.Maximum = 999999;
            this.nudQty.TabIndex = 6;
            this.nudQty.Size = new System.Drawing.Size(90, 23);
            this.nudQty.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudQty.ValueChanged += new System.EventHandler(this.nudQty_ValueChanged);
            this.chkAutoQty.Text = "Auto";
            this.chkAutoQty.AutoSize = true;
            this.chkAutoQty.TabIndex = 7;
            this.chkAutoQty.Checked = true;
            this.chkAutoQty.CheckedChanged += new System.EventHandler(this.chkAutoQty_CheckedChanged);
            qtyPanel.Controls.Add(this.nudQty);
            qtyPanel.Controls.Add(this.chkAutoQty);
            this.tableLayoutPanel.Controls.Add(qtyPanel, 1, 3);

            AddLabelControl(3, 2, lblPODate, "PO Date:");
            var poDatePanel = new System.Windows.Forms.FlowLayoutPanel
            {
                AutoSize = true,
                Dock = System.Windows.Forms.DockStyle.Fill,
                FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight,
            };
            this.dtpPODate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpPODate.TabIndex = 8;
            this.chkNoPODate.Text = "No PO Date";
            this.chkNoPODate.AutoSize = true;
            this.chkNoPODate.TabIndex = 9;
            this.chkNoPODate.CheckedChanged += new System.EventHandler(this.chkNoPODate_CheckedChanged);
            poDatePanel.Controls.Add(this.dtpPODate);
            poDatePanel.Controls.Add(this.chkNoPODate);
            this.tableLayoutPanel.Controls.Add(poDatePanel, 3, 3);

            // Row 4: Vintage | PO Number
            AddLabelControl(4, 0, lblVintage, "Vintage:");
            AddTextControl(4, 1, txtVintage, 10);
            AddLabelControl(4, 2, lblPONumber, "PO Number:");
            AddTextControl(4, 3, txtPONumber, 11);

            // Row 5: Recv Date | Unit Cost
            AddLabelControl(5, 0, lblRecvDate, "Recv Date:");
            var recvDatePanel = new System.Windows.Forms.FlowLayoutPanel
            {
                AutoSize = true,
                Dock = System.Windows.Forms.DockStyle.Fill,
                FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight,
            };
            this.chkRecvDate.Text = "Received";
            this.chkRecvDate.AutoSize = true;
            this.chkRecvDate.TabIndex = 12;
            this.chkRecvDate.CheckedChanged += new System.EventHandler(this.chkRecvDate_CheckedChanged);
            this.lblRecvDateDisplay.AutoSize = true;
            this.lblRecvDateDisplay.Visible = false;
            this.lblRecvDateDisplay.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
            this.lblRecvDateDisplay.Margin = new System.Windows.Forms.Padding(6, 3, 0, 0);
            recvDatePanel.Controls.Add(this.chkRecvDate);
            recvDatePanel.Controls.Add(this.lblRecvDateDisplay);
            this.tableLayoutPanel.Controls.Add(recvDatePanel, 1, 5);
            AddLabelControl(5, 2, lblUnitCost, "Unit Cost:");
            AddTextControl(5, 3, txtUnitCost, 13);
            this.txtUnitCost.Enter += new System.EventHandler(this.txtUnitCost_Enter);
            this.txtUnitCost.Leave += new System.EventHandler(this.txtUnitCost_Leave);

            // Row 6: CID | ME Number
            AddLabelControl(6, 0, lblCID, "CID:");
            AddTextControl(6, 1, txtCID, 14);
            AddLabelControl(6, 2, lblMENumber, "M.E. #:");
            AddTextControl(6, 3, txtMENumber, 15);

            // Row 7: Pur Code | Est
            AddLabelControl(7, 0, lblPurCode, "Pur. Code:");
            AddTextControl(7, 1, txtPurCode, 16);
            AddLabelControl(7, 2, lblEst, "Est.:");
            AddTextControl(7, 3, txtEst, 17);

            // Row 8: Comments label
            AddLabelControl(8, 0, lblComments, "Comments:");

            // Rows 8-10: Comments text area (span 3 cols, span 3 rows)
            this.txtComments.Multiline = true;
            this.txtComments.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtComments.TabIndex = 18;
            this.txtComments.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel.Controls.Add(this.txtComments, 1, 8);
            this.tableLayoutPanel.SetColumnSpan(this.txtComments, 3);
            this.tableLayoutPanel.SetRowSpan(this.txtComments, 3);

            // Row 11: Audit info label (span all 4 cols)
            this.lblAuditInfo.AutoSize = false;
            this.lblAuditInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblAuditInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblAuditInfo.Font = new System.Drawing.Font("Segoe UI", 8f, System.Drawing.FontStyle.Italic);
            this.lblAuditInfo.Margin = new System.Windows.Forms.Padding(6, 2, 6, 2);
            this.lblAuditInfo.Visible = false;
            this.tableLayoutPanel.Controls.Add(this.lblAuditInfo, 0, 11);
            this.tableLayoutPanel.SetColumnSpan(this.lblAuditInfo, 4);

            // pnlButtons
            this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlButtons.Height = 45;
            this.pnlButtons.Controls.Add(this.btnSave);
            this.pnlButtons.Controls.Add(this.btnCancel);

            // btnSave
            this.btnSave.Location = new System.Drawing.Point(8, 8);
            this.btnSave.Size = new System.Drawing.Size(90, 28);
            this.btnSave.Text = "Save";
            this.btnSave.TabIndex = 20;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);

            // btnCancel
            this.btnCancel.Location = new System.Drawing.Point(106, 8);
            this.btnCancel.Size = new System.Drawing.Size(90, 28);
            this.btnCancel.Text = "Cancel";
            this.btnCancel.TabIndex = 21;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);

            // RecordForm
            this.AcceptButton = this.btnSave;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(660, 500);
            this.Controls.Add(this.tableLayoutPanel);
            this.Controls.Add(this.pnlButtons);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.MinimumSize = new System.Drawing.Size(640, 520);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Record";

            ((System.ComponentModel.ISupportInitialize)(this.nudQty)).EndInit();
            this.pnlButtons.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private void AddLabelControl(int row, int col, System.Windows.Forms.Label lbl, string text)
        {
            lbl.Text = text;
            lbl.AutoSize = true;
            lbl.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Top;
            lbl.Margin = new System.Windows.Forms.Padding(6, 8, 0, 0);
            this.tableLayoutPanel.Controls.Add(lbl, col, row);
        }

        private void AddTextControl(int row, int col, System.Windows.Forms.TextBox txt, int tabIndex)
        {
            txt.Dock = System.Windows.Forms.DockStyle.Fill;
            txt.TabIndex = tabIndex;
            txt.Margin = new System.Windows.Forms.Padding(0, 4, 6, 4);
            this.tableLayoutPanel.Controls.Add(txt, col, row);
        }

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel = null!;
        private System.Windows.Forms.Label lblOpCo2 = null!;
        private System.Windows.Forms.TextBox txtOpCo2 = null!;
        private System.Windows.Forms.Label lblStatus = null!;
        private System.Windows.Forms.TextBox txtStatus = null!;
        private System.Windows.Forms.Label lblMFR = null!;
        private System.Windows.Forms.TextBox txtMFR = null!;
        private System.Windows.Forms.Label lblDevCode = null!;
        private System.Windows.Forms.TextBox txtDevCode = null!;
        private System.Windows.Forms.Label lblBegSer = null!;
        private System.Windows.Forms.TextBox txtBegSer = null!;
        private System.Windows.Forms.Label lblEndSer = null!;
        private System.Windows.Forms.TextBox txtEndSer = null!;
        private System.Windows.Forms.Label lblQty = null!;
        private System.Windows.Forms.NumericUpDown nudQty = null!;
        private System.Windows.Forms.CheckBox chkAutoQty = null!;
        private System.Windows.Forms.Label lblPODate = null!;
        private System.Windows.Forms.DateTimePicker dtpPODate = null!;
        private System.Windows.Forms.CheckBox chkNoPODate = null!;
        private System.Windows.Forms.Label lblVintage = null!;
        private System.Windows.Forms.TextBox txtVintage = null!;
        private System.Windows.Forms.Label lblPONumber = null!;
        private System.Windows.Forms.TextBox txtPONumber = null!;
        private System.Windows.Forms.Label lblRecvDate = null!;
        private System.Windows.Forms.CheckBox chkRecvDate = null!;
        private System.Windows.Forms.Label lblRecvDateDisplay = null!;
        private System.Windows.Forms.Label lblUnitCost = null!;
        private System.Windows.Forms.TextBox txtUnitCost = null!;
        private System.Windows.Forms.Label lblCID = null!;
        private System.Windows.Forms.TextBox txtCID = null!;
        private System.Windows.Forms.Label lblMENumber = null!;
        private System.Windows.Forms.TextBox txtMENumber = null!;
        private System.Windows.Forms.Label lblPurCode = null!;
        private System.Windows.Forms.TextBox txtPurCode = null!;
        private System.Windows.Forms.Label lblEst = null!;
        private System.Windows.Forms.TextBox txtEst = null!;
        private System.Windows.Forms.Label lblComments = null!;
        private System.Windows.Forms.TextBox txtComments = null!;
        private System.Windows.Forms.Label lblAuditInfo = null!;
        private System.Windows.Forms.Panel pnlButtons = null!;
        private System.Windows.Forms.Button btnSave = null!;
        private System.Windows.Forms.Button btnCancel = null!;
    }
}

