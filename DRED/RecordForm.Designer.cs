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
            this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.lblKey       = new System.Windows.Forms.Label();
            this.txtKey       = new System.Windows.Forms.TextBox();
            this.lblOpCo2     = new System.Windows.Forms.Label();
            this.txtOpCo2     = new System.Windows.Forms.TextBox();
            this.lblStatus    = new System.Windows.Forms.Label();
            this.txtStatus    = new System.Windows.Forms.TextBox();
            this.lblMFR       = new System.Windows.Forms.Label();
            this.txtMFR       = new System.Windows.Forms.TextBox();
            this.lblDevCode   = new System.Windows.Forms.Label();
            this.txtDevCode   = new System.Windows.Forms.TextBox();
            this.lblBegSer    = new System.Windows.Forms.Label();
            this.txtBegSer    = new System.Windows.Forms.TextBox();
            this.lblEndSer    = new System.Windows.Forms.Label();
            this.txtEndSer    = new System.Windows.Forms.TextBox();
            this.lblQty       = new System.Windows.Forms.Label();
            this.nudQty       = new System.Windows.Forms.NumericUpDown();
            this.lblPODate    = new System.Windows.Forms.Label();
            this.chkPODate    = new System.Windows.Forms.CheckBox();
            this.dtpPODate    = new System.Windows.Forms.DateTimePicker();
            this.lblVintage   = new System.Windows.Forms.Label();
            this.txtVintage   = new System.Windows.Forms.TextBox();
            this.lblPONumber  = new System.Windows.Forms.Label();
            this.txtPONumber  = new System.Windows.Forms.TextBox();
            this.lblRecvDate  = new System.Windows.Forms.Label();
            this.chkRecvDate  = new System.Windows.Forms.CheckBox();
            this.dtpRecvDate  = new System.Windows.Forms.DateTimePicker();
            this.lblUnitCost  = new System.Windows.Forms.Label();
            this.txtUnitCost  = new System.Windows.Forms.TextBox();
            this.lblCID       = new System.Windows.Forms.Label();
            this.txtCID       = new System.Windows.Forms.TextBox();
            this.lblMENumber  = new System.Windows.Forms.Label();
            this.txtMENumber  = new System.Windows.Forms.TextBox();
            this.lblPurCode   = new System.Windows.Forms.Label();
            this.txtPurCode   = new System.Windows.Forms.TextBox();
            this.lblEst       = new System.Windows.Forms.Label();
            this.txtEst       = new System.Windows.Forms.TextBox();
            this.lblComments  = new System.Windows.Forms.Label();
            this.txtComments  = new System.Windows.Forms.TextBox();
            this.pnlButtons   = new System.Windows.Forms.Panel();
            this.btnSave      = new System.Windows.Forms.Button();
            this.btnCancel    = new System.Windows.Forms.Button();

            ((System.ComponentModel.ISupportInitialize)(this.nudQty)).BeginInit();
            this.pnlButtons.SuspendLayout();
            this.SuspendLayout();

            // tableLayoutPanel
            this.tableLayoutPanel.ColumnCount = 4;
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 110F));
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 110F));
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel.Padding = new System.Windows.Forms.Padding(8);
            this.tableLayoutPanel.RowCount = 12;
            for (int i = 0; i < 12; i++)
                this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tableLayoutPanel.Name = "tableLayoutPanel";
            this.tableLayoutPanel.TabIndex = 0;

            // Row 0: Key | OpCo2
            AddLabelControl(0, 0, lblKey,  "Key:");
            AddTextControl(0, 1, txtKey, 0);
            AddLabelControl(0, 2, lblOpCo2, "OpCo2:");
            AddTextControl(0, 3, txtOpCo2, 1);

            // Row 1: Status | MFR
            AddLabelControl(1, 0, lblStatus, "Status:");
            AddTextControl(1, 1, txtStatus, 2);
            AddLabelControl(1, 2, lblMFR, "MFR:");
            AddTextControl(1, 3, txtMFR, 3);

            // Row 2: Dev Code | Beg Ser
            AddLabelControl(2, 0, lblDevCode, "Dev Code:");
            AddTextControl(2, 1, txtDevCode, 4);
            AddLabelControl(2, 2, lblBegSer, "Beg Ser:");
            AddTextControl(2, 3, txtBegSer, 5);

            // Row 3: End Ser | Qty
            AddLabelControl(3, 0, lblEndSer, "End Ser:");
            AddTextControl(3, 1, txtEndSer, 6);
            AddLabelControl(3, 2, lblQty, "Qty:");
            this.nudQty.Location = new System.Drawing.Point(0, 0);
            this.nudQty.Maximum = 999999;
            this.nudQty.TabIndex = 7;
            this.nudQty.Size = new System.Drawing.Size(100, 23);
            this.tableLayoutPanel.Controls.Add(this.nudQty, 3, 3);
            this.nudQty.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Top;

            // Row 4: PO Date | Vintage
            AddLabelControl(4, 0, lblPODate, "PO Date:");
            var poDatePanel = new System.Windows.Forms.FlowLayoutPanel { AutoSize = true, Dock = System.Windows.Forms.DockStyle.Fill };
            this.chkPODate.Text = "Set";
            this.chkPODate.AutoSize = true;
            this.chkPODate.TabIndex = 8;
            this.chkPODate.CheckedChanged += new System.EventHandler(this.chkPODate_CheckedChanged);
            this.dtpPODate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpPODate.Enabled = false;
            this.dtpPODate.TabIndex = 9;
            poDatePanel.Controls.Add(this.chkPODate);
            poDatePanel.Controls.Add(this.dtpPODate);
            this.tableLayoutPanel.Controls.Add(poDatePanel, 1, 4);
            AddLabelControl(4, 2, lblVintage, "Vintage:");
            AddTextControl(4, 3, txtVintage, 10);

            // Row 5: PO Number | Recv Date
            AddLabelControl(5, 0, lblPONumber, "PO Number:");
            AddTextControl(5, 1, txtPONumber, 11);
            AddLabelControl(5, 2, lblRecvDate, "Recv Date:");
            var recvDatePanel = new System.Windows.Forms.FlowLayoutPanel { AutoSize = true, Dock = System.Windows.Forms.DockStyle.Fill };
            this.chkRecvDate.Text = "Set";
            this.chkRecvDate.AutoSize = true;
            this.chkRecvDate.TabIndex = 12;
            this.chkRecvDate.CheckedChanged += new System.EventHandler(this.chkRecvDate_CheckedChanged);
            this.dtpRecvDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpRecvDate.Enabled = false;
            this.dtpRecvDate.TabIndex = 13;
            recvDatePanel.Controls.Add(this.chkRecvDate);
            recvDatePanel.Controls.Add(this.dtpRecvDate);
            this.tableLayoutPanel.Controls.Add(recvDatePanel, 3, 5);

            // Row 6: Unit Cost | CID
            AddLabelControl(6, 0, lblUnitCost, "Unit Cost:");
            AddTextControl(6, 1, txtUnitCost, 14);
            AddLabelControl(6, 2, lblCID, "CID:");
            AddTextControl(6, 3, txtCID, 15);

            // Row 7: M.E. # | Pur. Code
            AddLabelControl(7, 0, lblMENumber, "M.E. #:");
            AddTextControl(7, 1, txtMENumber, 16);
            AddLabelControl(7, 2, lblPurCode, "Pur. Code:");
            AddTextControl(7, 3, txtPurCode, 17);

            // Row 8: Est. (span 2 cols)
            AddLabelControl(8, 0, lblEst, "Est.:");
            AddTextControl(8, 1, txtEst, 18);

            // Row 9-11: Comments (span all)
            AddLabelControl(9, 0, lblComments, "Comments:");
            this.txtComments.Multiline = true;
            this.txtComments.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtComments.Size = new System.Drawing.Size(100, 80);
            this.txtComments.TabIndex = 19;
            this.txtComments.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel.Controls.Add(this.txtComments, 1, 9);
            this.tableLayoutPanel.SetColumnSpan(this.txtComments, 3);
            this.tableLayoutPanel.SetRowSpan(this.txtComments, 3);

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
            this.ClientSize = new System.Drawing.Size(640, 520);
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
        private System.Windows.Forms.Label lblKey = null!;
        private System.Windows.Forms.TextBox txtKey = null!;
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
        private System.Windows.Forms.Label lblPODate = null!;
        private System.Windows.Forms.CheckBox chkPODate = null!;
        private System.Windows.Forms.DateTimePicker dtpPODate = null!;
        private System.Windows.Forms.Label lblVintage = null!;
        private System.Windows.Forms.TextBox txtVintage = null!;
        private System.Windows.Forms.Label lblPONumber = null!;
        private System.Windows.Forms.TextBox txtPONumber = null!;
        private System.Windows.Forms.Label lblRecvDate = null!;
        private System.Windows.Forms.CheckBox chkRecvDate = null!;
        private System.Windows.Forms.DateTimePicker dtpRecvDate = null!;
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
        private System.Windows.Forms.Panel pnlButtons = null!;
        private System.Windows.Forms.Button btnSave = null!;
        private System.Windows.Forms.Button btnCancel = null!;
    }
}
