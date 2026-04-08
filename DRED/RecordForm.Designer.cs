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
            // ── Controls ────────────────────────────────────────────────
            this.pnlScroll         = new System.Windows.Forms.Panel();
            this.pnlDeviceInfo     = new System.Windows.Forms.Panel();
            this.pnlSerialRange    = new System.Windows.Forms.Panel();
            this.pnlPurchaseInfo   = new System.Windows.Forms.Panel();
            this.pnlIdentifiers    = new System.Windows.Forms.Panel();
            this.pnlCommentsCard   = new System.Windows.Forms.Panel();

            this.lblOpCo2          = new System.Windows.Forms.Label();
            this.txtOpCo2          = new MaterialSkin.Controls.MaterialTextBox2();
            this.lblStatus         = new System.Windows.Forms.Label();
            this.txtStatus         = new MaterialSkin.Controls.MaterialTextBox2();
            this.lblMFR            = new System.Windows.Forms.Label();
            this.txtMFR            = new MaterialSkin.Controls.MaterialTextBox2();
            this.lblDevCode        = new System.Windows.Forms.Label();
            this.txtDevCode        = new MaterialSkin.Controls.MaterialTextBox2();

            this.lblBegSer         = new System.Windows.Forms.Label();
            this.txtBegSer         = new MaterialSkin.Controls.MaterialTextBox2();
            this.lblEndSer         = new System.Windows.Forms.Label();
            this.txtEndSer         = new MaterialSkin.Controls.MaterialTextBox2();
            this.lblQty            = new System.Windows.Forms.Label();
            this.nudQty            = new System.Windows.Forms.NumericUpDown();
            this.chkAutoQty        = new MaterialSkin.Controls.MaterialCheckbox();

            this.lblPODate         = new System.Windows.Forms.Label();
            this.txtPODate         = new MaterialSkin.Controls.MaterialTextBox2();
            this.lblPONumber       = new System.Windows.Forms.Label();
            this.txtPONumber       = new MaterialSkin.Controls.MaterialTextBox2();
            this.lblVintage        = new System.Windows.Forms.Label();
            this.txtVintage        = new MaterialSkin.Controls.MaterialTextBox2();
            this.lblRecvDate       = new System.Windows.Forms.Label();
            this.chkRecvDate       = new MaterialSkin.Controls.MaterialCheckbox();
            this.lblRecvDateDisplay = new System.Windows.Forms.Label();
            this.lblUnitCost       = new System.Windows.Forms.Label();
            this.txtUnitCost       = new MaterialSkin.Controls.MaterialTextBox2();

            this.lblCID            = new System.Windows.Forms.Label();
            this.txtCID            = new MaterialSkin.Controls.MaterialTextBox2();
            this.lblMENumber       = new System.Windows.Forms.Label();
            this.txtMENumber       = new MaterialSkin.Controls.MaterialTextBox2();
            this.lblPurCode        = new System.Windows.Forms.Label();
            this.txtPurCode        = new MaterialSkin.Controls.MaterialTextBox2();
            this.lblEst            = new System.Windows.Forms.Label();
            this.txtEst            = new MaterialSkin.Controls.MaterialTextBox2();

            this.lblComments       = new System.Windows.Forms.Label();
            this.txtComments       = new System.Windows.Forms.RichTextBox();

            this.lblAuditInfo      = new System.Windows.Forms.Label();
            this.pnlButtons        = new System.Windows.Forms.Panel();
            this.btnSave           = new MaterialSkin.Controls.MaterialButton();
            this.btnCancel         = new MaterialSkin.Controls.MaterialButton();

            // Keep tableLayoutPanel for backward compatibility (not used in layout)
            this.tableLayoutPanel  = new System.Windows.Forms.TableLayoutPanel();

            ((System.ComponentModel.ISupportInitialize)(this.nudQty)).BeginInit();
            this.pnlButtons.SuspendLayout();
            this.SuspendLayout();

            // ── Shared colors ────────────────────────────────────────────
            var cardBack  = System.Drawing.Color.FromArgb(56, 56, 58);    // #383838 — card bg
            var formBack  = System.Drawing.Color.FromArgb(45, 45, 48);    // form bg
            var labelFore = System.Drawing.Color.FromArgb(180, 180, 180); // label text
            var headerFore = System.Drawing.Color.FromArgb(100, 181, 246); // accent blue for section headers
            var accentBorder = System.Drawing.Color.FromArgb(62, 62, 66); // subtle border

            // ── pnlScroll — outer scrollable container ───────────────────
            this.pnlScroll.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlScroll.AutoScroll = true;
            this.pnlScroll.Padding = new System.Windows.Forms.Padding(10, 6, 10, 6);
            this.pnlScroll.BackColor = formBack;

            // ── Card: Device Information ─────────────────────────────────
            var tblDevice = MakeCardTable(2);
            var lblDevHeader = MakeCardHeader("DEVICE INFORMATION", headerFore);
            // Row 0: OpCo2 | Status
            AddLabelToCard(tblDevice, lblOpCo2, "OpCo2:", labelFore, 0, 0);
            ConfigureMaterialText(txtOpCo2, 0); tblDevice.Controls.Add(txtOpCo2, 1, 0);
            AddLabelToCard(tblDevice, lblStatus, "Status:", labelFore, 0, 2);
            ConfigureMaterialText(txtStatus, 1); tblDevice.Controls.Add(txtStatus, 3, 0);
            // Row 1: MFR | DevCode
            AddLabelToCard(tblDevice, lblMFR, "MFR:", labelFore, 1, 0);
            ConfigureMaterialText(txtMFR, 2); tblDevice.Controls.Add(txtMFR, 1, 1);
            AddLabelToCard(tblDevice, lblDevCode, "Dev Code:", labelFore, 1, 2);
            ConfigureMaterialText(txtDevCode, 3); tblDevice.Controls.Add(txtDevCode, 3, 1);

            pnlDeviceInfo = MakeCard(cardBack, accentBorder, lblDevHeader, tblDevice);

            // ── Card: Serial Range & Quantity ────────────────────────────
            var tblSerial = MakeCardTable(2);
            var lblSerialHeader = MakeCardHeader("SERIAL RANGE & QUANTITY", headerFore);
            // Row 0: BegSer | EndSer
            AddLabelToCard(tblSerial, lblBegSer, "Beg Ser:", labelFore, 0, 0);
            ConfigureMaterialText(txtBegSer, 4);
            txtBegSer.TextChanged += new System.EventHandler(this.SerialField_TextChanged);
            tblSerial.Controls.Add(txtBegSer, 1, 0);
            AddLabelToCard(tblSerial, lblEndSer, "End Ser:", labelFore, 0, 2);
            ConfigureMaterialText(txtEndSer, 5);
            txtEndSer.TextChanged += new System.EventHandler(this.SerialField_TextChanged);
            tblSerial.Controls.Add(txtEndSer, 3, 0);
            // Row 1: Qty (+Auto)
            AddLabelToCard(tblSerial, lblQty, "Qty:", labelFore, 1, 0);
            var qtyPanel = new System.Windows.Forms.FlowLayoutPanel
            {
                Dock = System.Windows.Forms.DockStyle.Fill,
                FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight,
                BackColor = System.Drawing.Color.Transparent,
                Margin = System.Windows.Forms.Padding.Empty,
                Padding = System.Windows.Forms.Padding.Empty,
            };
            this.nudQty.Maximum = 999999;
            this.nudQty.TabIndex = 6;
            this.nudQty.Size = new System.Drawing.Size(90, 36);
            this.nudQty.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudQty.BackColor = System.Drawing.Color.FromArgb(50, 50, 50);
            this.nudQty.ForeColor = System.Drawing.Color.White;
            this.nudQty.ValueChanged += new System.EventHandler(this.nudQty_ValueChanged);
            this.chkAutoQty.Text = "Auto";
            this.chkAutoQty.AutoSize = true;
            this.chkAutoQty.TabIndex = 7;
            this.chkAutoQty.Checked = true;
            this.chkAutoQty.CheckedChanged += new System.EventHandler(this.chkAutoQty_CheckedChanged);
            qtyPanel.Controls.Add(this.nudQty);
            qtyPanel.Controls.Add(this.chkAutoQty);
            tblSerial.Controls.Add(qtyPanel, 1, 1);
            // Span qty panel across remaining 3 columns
            tblSerial.SetColumnSpan(qtyPanel, 3);

            pnlSerialRange = MakeCard(cardBack, accentBorder, lblSerialHeader, tblSerial);

            // ── Card: Purchase Information ───────────────────────────────
            var tblPurchase = MakeCardTable(3);
            var lblPurchaseHeader = MakeCardHeader("PURCHASE INFORMATION", headerFore);
            // Row 0: PO Date | txtPODate | PO Number | txtPONumber
            AddLabelToCard(tblPurchase, lblPODate, "PO Date:", labelFore, 0, 0);
            ConfigureMaterialText(txtPODate, 8);
            this.txtPODate.Hint = "MM/dd/yyyy";
            tblPurchase.Controls.Add(this.txtPODate, 1, 0);
            AddLabelToCard(tblPurchase, lblPONumber, "PO Number:", labelFore, 0, 2);
            ConfigureMaterialText(txtPONumber, 9);
            tblPurchase.Controls.Add(txtPONumber, 3, 0);
            // Row 1: Vintage | txtVintage | Unit Cost | txtUnitCost
            AddLabelToCard(tblPurchase, lblVintage, "Vintage:", labelFore, 1, 0);
            ConfigureMaterialText(txtVintage, 10); tblPurchase.Controls.Add(txtVintage, 1, 1);
            AddLabelToCard(tblPurchase, lblUnitCost, "Unit Cost:", labelFore, 1, 2);
            ConfigureMaterialText(txtUnitCost, 11);
            this.txtUnitCost.Enter += new System.EventHandler(this.txtUnitCost_Enter);
            this.txtUnitCost.Leave += new System.EventHandler(this.txtUnitCost_Leave);
            tblPurchase.Controls.Add(txtUnitCost, 3, 1);
            // Row 2: Recv Date alone
            AddLabelToCard(tblPurchase, lblRecvDate, "Recv Date:", labelFore, 2, 0);
            var recvPanel = new System.Windows.Forms.FlowLayoutPanel
            {
                Dock = System.Windows.Forms.DockStyle.Fill,
                FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight,
                BackColor = System.Drawing.Color.Transparent,
                Margin = System.Windows.Forms.Padding.Empty,
            };
            this.chkRecvDate.Text = "Received";
            this.chkRecvDate.AutoSize = true;
            this.chkRecvDate.TabIndex = 12;
            this.chkRecvDate.CheckedChanged += new System.EventHandler(this.chkRecvDate_CheckedChanged);
            this.lblRecvDateDisplay.AutoSize = true;
            this.lblRecvDateDisplay.Visible = false;
            this.lblRecvDateDisplay.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
            this.lblRecvDateDisplay.ForeColor = System.Drawing.Color.FromArgb(144, 202, 249);
            this.lblRecvDateDisplay.Margin = new System.Windows.Forms.Padding(6, 6, 0, 0);
            recvPanel.Controls.Add(this.chkRecvDate);
            recvPanel.Controls.Add(this.lblRecvDateDisplay);
            tblPurchase.Controls.Add(recvPanel, 1, 2);

            pnlPurchaseInfo = MakeCard(cardBack, accentBorder, lblPurchaseHeader, tblPurchase);

            // ── Card: Identifiers ────────────────────────────────────────
            var tblIdent = MakeCardTable(2);
            var lblIdentHeader = MakeCardHeader("IDENTIFIERS", headerFore);
            // Row 0: CID | ME Number
            AddLabelToCard(tblIdent, lblCID, "CID:", labelFore, 0, 0);
            ConfigureMaterialText(txtCID, 14); tblIdent.Controls.Add(txtCID, 1, 0);
            AddLabelToCard(tblIdent, lblMENumber, "M.E. #:", labelFore, 0, 2);
            ConfigureMaterialText(txtMENumber, 15); tblIdent.Controls.Add(txtMENumber, 3, 0);
            // Row 1: Pur Code | Est
            AddLabelToCard(tblIdent, lblPurCode, "Pur. Code:", labelFore, 1, 0);
            ConfigureMaterialText(txtPurCode, 16); tblIdent.Controls.Add(txtPurCode, 1, 1);
            AddLabelToCard(tblIdent, lblEst, "Est.:", labelFore, 1, 2);
            ConfigureMaterialText(txtEst, 17); tblIdent.Controls.Add(txtEst, 3, 1);

            pnlIdentifiers = MakeCard(cardBack, accentBorder, lblIdentHeader, tblIdent);

            // ── Card: Comments & Notes ───────────────────────────────────
            var lblCommentsHeader = MakeCardHeader("COMMENTS & NOTES", headerFore);
            this.lblComments.Text = "";
            this.lblComments.AutoSize = false;
            this.lblComments.Visible = false;

            this.txtComments.Multiline = true;
            this.txtComments.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.txtComments.TabIndex = 18;
            this.txtComments.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtComments.BackColor = System.Drawing.Color.FromArgb(50, 50, 50);
            this.txtComments.ForeColor = System.Drawing.Color.FromArgb(241, 241, 241);
            this.txtComments.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtComments.MinimumSize = new System.Drawing.Size(0, 100);

            var commentsInner = new System.Windows.Forms.Panel
            {
                Dock = System.Windows.Forms.DockStyle.Fill,
                Padding = new System.Windows.Forms.Padding(0),
                Height = 110,
                MinimumSize = new System.Drawing.Size(0, 110),
            };
            commentsInner.Controls.Add(this.txtComments);

            pnlCommentsCard = MakeCardWithContent(cardBack, accentBorder, lblCommentsHeader, commentsInner, 160);

            // ── lblAuditInfo ─────────────────────────────────────────────
            this.lblAuditInfo.AutoSize = false;
            this.lblAuditInfo.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblAuditInfo.Height = 24;
            this.lblAuditInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblAuditInfo.Font = new System.Drawing.Font("Segoe UI", 8f, System.Drawing.FontStyle.Italic);
            this.lblAuditInfo.ForeColor = System.Drawing.Color.FromArgb(144, 202, 249);
            this.lblAuditInfo.Margin = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.lblAuditInfo.Visible = false;
            this.lblAuditInfo.BackColor = System.Drawing.Color.Transparent;

            // ── Assemble scroll panel ────────────────────────────────────
            // Add in reverse order (last added appears at top with DockStyle.Top)
            this.pnlScroll.Controls.Add(this.lblAuditInfo);
            this.pnlScroll.Controls.Add(pnlCommentsCard);
            this.pnlScroll.Controls.Add(pnlIdentifiers);
            this.pnlScroll.Controls.Add(pnlPurchaseInfo);
            this.pnlScroll.Controls.Add(pnlSerialRange);
            this.pnlScroll.Controls.Add(pnlDeviceInfo);

            // ── pnlButtons ───────────────────────────────────────────────
            this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlButtons.Height = 46;
            this.pnlButtons.BackColor = System.Drawing.Color.FromArgb(37, 37, 40);
            this.pnlButtons.Padding = new System.Windows.Forms.Padding(12);
            this.pnlButtons.Controls.Add(this.btnSave);
            this.pnlButtons.Controls.Add(this.btnCancel);

            this.btnSave.Location = new System.Drawing.Point(12, 8);
            this.btnSave.Text = "Save";
            this.btnSave.TabIndex = 20;
            this.btnSave.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnSave.HighEmphasis = true;
            this.btnSave.AutoSize = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);

            this.btnCancel.Location = new System.Drawing.Point(130, 8);
            this.btnCancel.Text = "Cancel";
            this.btnCancel.TabIndex = 21;
            this.btnCancel.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined;
            this.btnCancel.HighEmphasis = false;
            this.btnCancel.AutoSize = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);

            // tableLayoutPanel kept for reference but not used in new layout
            this.tableLayoutPanel.Visible = false;

            // ── RecordForm ───────────────────────────────────────────────
            this.AcceptButton = this.btnSave;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(720, 920);
            this.Controls.Add(this.pnlScroll);
            this.Controls.Add(this.pnlButtons);
            this.Controls.Add(this.tableLayoutPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.MinimumSize = new System.Drawing.Size(700, 850);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Record";

            ((System.ComponentModel.ISupportInitialize)(this.nudQty)).EndInit();
            this.pnlButtons.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        // ── Layout helpers ────────────────────────────────────────────────

        /// <summary>
        /// Creates a 4-column TableLayoutPanel for a card section (label | input | label | input).
        /// </summary>
        private static System.Windows.Forms.TableLayoutPanel MakeCardTable(int rows)
        {
            var tbl = new System.Windows.Forms.TableLayoutPanel
            {
                ColumnCount = 4,
                RowCount = rows,
                Dock = System.Windows.Forms.DockStyle.Top,
                AutoSize = true,
                BackColor = System.Drawing.Color.Transparent,
                Padding = System.Windows.Forms.Padding.Empty,
                Margin = System.Windows.Forms.Padding.Empty,
            };
            tbl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 110F));
            tbl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tbl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 110F));
            tbl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            for (int i = 0; i < rows; i++)
                tbl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            return tbl;
        }

        /// <summary>Creates a bold accent-colored section header label.</summary>
        private static System.Windows.Forms.Label MakeCardHeader(string text, System.Drawing.Color color)
        {
            return new System.Windows.Forms.Label
            {
                Text = text,
                Dock = System.Windows.Forms.DockStyle.Top,
                Height = 22,
                Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold),
                ForeColor = color,
                BackColor = System.Drawing.Color.Transparent,
                TextAlign = System.Drawing.ContentAlignment.BottomLeft,
                Padding = new System.Windows.Forms.Padding(2, 0, 0, 0),
                Margin = System.Windows.Forms.Padding.Empty,
                UseMnemonic = false,
            };
        }

        /// <summary>
        /// Wraps a header label and a content table into a styled card panel.
        /// </summary>
        private static System.Windows.Forms.Panel MakeCard(
            System.Drawing.Color backColor,
            System.Drawing.Color borderColor,
            System.Windows.Forms.Label header,
            System.Windows.Forms.TableLayoutPanel content)
        {
            var panel = new System.Windows.Forms.Panel
            {
                Dock = System.Windows.Forms.DockStyle.Top,
                AutoSize = true,
                BackColor = backColor,
                Padding = new System.Windows.Forms.Padding(8, 4, 8, 4),
                Margin = new System.Windows.Forms.Padding(0, 0, 0, 4),
            };
            panel.Paint += (s, e) =>
            {
                using var pen = new System.Drawing.Pen(borderColor);
                e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1);
            };

            // Add content first, then header (header docks Top, so it appears above)
            panel.Controls.Add(content);
            panel.Controls.Add(header);
            return panel;
        }

        /// <summary>
        /// Wraps a header and arbitrary content control into a card panel with a fixed inner height.
        /// </summary>
        private static System.Windows.Forms.Panel MakeCardWithContent(
            System.Drawing.Color backColor,
            System.Drawing.Color borderColor,
            System.Windows.Forms.Label header,
            System.Windows.Forms.Control content,
            int contentHeight)
        {
            content.Dock = System.Windows.Forms.DockStyle.Top;
            content.Height = contentHeight;

            var panel = new System.Windows.Forms.Panel
            {
                Dock = System.Windows.Forms.DockStyle.Top,
                BackColor = backColor,
                Padding = new System.Windows.Forms.Padding(8, 4, 8, 4),
                Margin = new System.Windows.Forms.Padding(0, 0, 0, 4),
            };
            panel.Paint += (s, e) =>
            {
                using var pen = new System.Drawing.Pen(borderColor);
                e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1);
            };

            panel.Controls.Add(content);
            panel.Controls.Add(header);

            // Set panel height to account for header + content + vertical padding (top:4 + bottom:4)
            const int verticalPadding = 8; // top (4) + bottom (4) from Padding(8, 4, 8, 4)
            panel.Height = header.Height + contentHeight + verticalPadding;
            return panel;
        }

        private static void AddLabelToCard(
            System.Windows.Forms.TableLayoutPanel tbl,
            System.Windows.Forms.Label lbl,
            string text,
            System.Drawing.Color foreColor,
            int row, int col)
        {
            lbl.Text = text;
            lbl.AutoSize = true;
            lbl.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Top;
            lbl.Margin = new System.Windows.Forms.Padding(0, 8, 4, 0);
            lbl.ForeColor = foreColor;
            lbl.BackColor = System.Drawing.Color.Transparent;
            tbl.Controls.Add(lbl, col, row);
        }

        private static void ConfigureMaterialText(MaterialSkin.Controls.MaterialTextBox2 txt, int tabIndex)
        {
            txt.UseTallSize = false;
            txt.Dock = System.Windows.Forms.DockStyle.Fill;
            txt.TabIndex = tabIndex;
            txt.Margin = new System.Windows.Forms.Padding(0, 2, 4, 2);
        }

        // ── Field declarations ────────────────────────────────────────────
        private System.Windows.Forms.Panel pnlScroll = null!;
        private System.Windows.Forms.Panel pnlDeviceInfo = null!;
        private System.Windows.Forms.Panel pnlSerialRange = null!;
        private System.Windows.Forms.Panel pnlPurchaseInfo = null!;
        private System.Windows.Forms.Panel pnlIdentifiers = null!;
        private System.Windows.Forms.Panel pnlCommentsCard = null!;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel = null!;
        private System.Windows.Forms.Label lblOpCo2 = null!;
        private MaterialSkin.Controls.MaterialTextBox2 txtOpCo2 = null!;
        private System.Windows.Forms.Label lblStatus = null!;
        private MaterialSkin.Controls.MaterialTextBox2 txtStatus = null!;
        private System.Windows.Forms.Label lblMFR = null!;
        private MaterialSkin.Controls.MaterialTextBox2 txtMFR = null!;
        private System.Windows.Forms.Label lblDevCode = null!;
        private MaterialSkin.Controls.MaterialTextBox2 txtDevCode = null!;
        private System.Windows.Forms.Label lblBegSer = null!;
        private MaterialSkin.Controls.MaterialTextBox2 txtBegSer = null!;
        private System.Windows.Forms.Label lblEndSer = null!;
        private MaterialSkin.Controls.MaterialTextBox2 txtEndSer = null!;
        private System.Windows.Forms.Label lblQty = null!;
        private System.Windows.Forms.NumericUpDown nudQty = null!;
        private MaterialSkin.Controls.MaterialCheckbox chkAutoQty = null!;
        private System.Windows.Forms.Label lblPODate = null!;
        private MaterialSkin.Controls.MaterialTextBox2 txtPODate = null!;
        private System.Windows.Forms.Label lblVintage = null!;
        private MaterialSkin.Controls.MaterialTextBox2 txtVintage = null!;
        private System.Windows.Forms.Label lblPONumber = null!;
        private MaterialSkin.Controls.MaterialTextBox2 txtPONumber = null!;
        private System.Windows.Forms.Label lblRecvDate = null!;
        private MaterialSkin.Controls.MaterialCheckbox chkRecvDate = null!;
        private System.Windows.Forms.Label lblRecvDateDisplay = null!;
        private System.Windows.Forms.Label lblUnitCost = null!;
        private MaterialSkin.Controls.MaterialTextBox2 txtUnitCost = null!;
        private System.Windows.Forms.Label lblCID = null!;
        private MaterialSkin.Controls.MaterialTextBox2 txtCID = null!;
        private System.Windows.Forms.Label lblMENumber = null!;
        private MaterialSkin.Controls.MaterialTextBox2 txtMENumber = null!;
        private System.Windows.Forms.Label lblPurCode = null!;
        private MaterialSkin.Controls.MaterialTextBox2 txtPurCode = null!;
        private System.Windows.Forms.Label lblEst = null!;
        private MaterialSkin.Controls.MaterialTextBox2 txtEst = null!;
        private System.Windows.Forms.Label lblComments = null!;
        private System.Windows.Forms.RichTextBox txtComments = null!;
        private System.Windows.Forms.Label lblAuditInfo = null!;
        private System.Windows.Forms.Panel pnlButtons = null!;
        private MaterialSkin.Controls.MaterialButton btnSave = null!;
        private MaterialSkin.Controls.MaterialButton btnCancel = null!;
    }
}
