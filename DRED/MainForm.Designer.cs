namespace DRED
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _refreshTimer?.Dispose();
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlToolbar          = new System.Windows.Forms.FlowLayoutPanel();
            this.btnAdd              = new MaterialSkin.Controls.MaterialButton();
            this.btnEdit             = new MaterialSkin.Controls.MaterialButton();
            this.btnDelete           = new MaterialSkin.Controls.MaterialButton();
            this.btnRefresh          = new MaterialSkin.Controls.MaterialButton();
            this.btnImport           = new MaterialSkin.Controls.MaterialButton();
            this.btnExport           = new MaterialSkin.Controls.MaterialButton();
            this.btnExportAll        = new MaterialSkin.Controls.MaterialButton();
            this.btnAdvancedSearch   = new MaterialSkin.Controls.MaterialButton();
            this.btnSettings         = new MaterialSkin.Controls.MaterialButton();
            this.pnlSearch           = new System.Windows.Forms.Panel();
            this.txtSearch           = new MaterialSkin.Controls.MaterialTextBox2();
            this.cboFilterColumn     = new MaterialSkin.Controls.MaterialComboBox();
            this.tabControl          = new MaterialSkin.Controls.MaterialTabControl();
            this.tabSelector         = new MaterialSkin.Controls.MaterialTabSelector();
            this.tabOHMeters         = new System.Windows.Forms.TabPage();
            this.tabIMMeters         = new System.Windows.Forms.TabPage();
            this.tabOHTransformers   = new System.Windows.Forms.TabPage();
            this.tabIMTransformers   = new System.Windows.Forms.TabPage();
            this.gridOHMeters        = CreateGrid();
            this.gridIMMeters        = CreateGrid();
            this.gridOHTransformers  = CreateGrid();
            this.gridIMTransformers  = CreateGrid();
            this.pnlStatus           = new System.Windows.Forms.Panel();
            this.lblStatusRecords    = new System.Windows.Forms.Label();
            this.lblStatusConnection = new System.Windows.Forms.Label();
            this.lblStatusUser       = new System.Windows.Forms.Label();

            // Wire CellDoubleClick for all grids
            this.gridOHMeters.CellDoubleClick       += new System.Windows.Forms.DataGridViewCellEventHandler(this.grid_CellDoubleClick);
            this.gridIMMeters.CellDoubleClick       += new System.Windows.Forms.DataGridViewCellEventHandler(this.grid_CellDoubleClick);
            this.gridOHTransformers.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.grid_CellDoubleClick);
            this.gridIMTransformers.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.grid_CellDoubleClick);

            this.pnlToolbar.SuspendLayout();
            this.pnlSearch.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.pnlStatus.SuspendLayout();
            this.SuspendLayout();

            // pnlToolbar
            this.pnlToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlToolbar.Height = 54;
            this.pnlToolbar.Padding = new System.Windows.Forms.Padding(4, 8, 4, 4);
            this.pnlToolbar.WrapContents = false;
            this.pnlToolbar.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);

            SetMaterialButton(btnAdd,            "Add (Ctrl+N)",              MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained, true,  this.btnAdd_Click);
            SetMaterialButton(btnEdit,           "Edit (Ctrl+E)",             MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined,  false, this.btnEdit_Click);
            SetMaterialButton(btnDelete,         "Delete (Del)",              MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined,  false, this.btnDelete_Click);
            SetMaterialButton(btnRefresh,        "Refresh (F5)",              MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined,  false, this.btnRefresh_Click);
            SetMaterialButton(btnImport,         "Import (Ctrl+I)",           MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined,  false, this.btnImport_Click);
            SetMaterialButton(btnExport,         "Export (Ctrl+S)",           MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined,  false, this.btnExport_Click);
            SetMaterialButton(btnExportAll,      "Export All (Ctrl+Shift+S)", MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined,  false, this.btnExportAll_Click);
            SetMaterialButton(btnAdvancedSearch, "Advanced Search (Ctrl+Shift+F)", MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined, false, this.btnAdvancedSearch_Click);
            SetMaterialButton(btnSettings,       "Settings (Ctrl+,)",         MaterialSkin.Controls.MaterialButton.MaterialButtonType.Text,      false, this.btnSettings_Click);

            this.pnlToolbar.Controls.AddRange(new System.Windows.Forms.Control[] {
                btnAdd, btnEdit, btnDelete, btnRefresh,
                btnImport, btnExport, btnExportAll, btnAdvancedSearch, btnSettings });

            // pnlSearch
            this.pnlSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlSearch.Height = 60;
            this.pnlSearch.Padding = new System.Windows.Forms.Padding(6, 4, 6, 4);
            this.pnlSearch.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);

            // txtSearch (MaterialTextBox2)
            this.txtSearch.Hint = "Search…";
            this.txtSearch.UseTallSize = false;
            this.txtSearch.Location = new System.Drawing.Point(6, 6);
            this.txtSearch.Size = new System.Drawing.Size(300, 48);
            this.txtSearch.TabIndex = 1;
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);

            // cboFilterColumn (MaterialComboBox)
            this.cboFilterColumn.Location = new System.Drawing.Point(314, 14);
            this.cboFilterColumn.Size = new System.Drawing.Size(180, 36);
            this.cboFilterColumn.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboFilterColumn.TabIndex = 2;

            this.pnlSearch.Controls.Add(this.txtSearch);
            this.pnlSearch.Controls.Add(this.cboFilterColumn);

            // tabControl (MaterialTabControl)
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.TabPages.AddRange(new System.Windows.Forms.TabPage[] {
                tabOHMeters, tabIMMeters, tabOHTransformers, tabIMTransformers });
            this.tabControl.TabIndex = 3;
            this.tabControl.SelectedIndexChanged += new System.EventHandler(this.tabControl_SelectedIndexChanged);

            SetupTab(tabOHMeters,       "OH - Meters",        gridOHMeters);
            SetupTab(tabIMMeters,       "I&M - Meters",       gridIMMeters);
            SetupTab(tabOHTransformers, "OH - Transformers",  gridOHTransformers);
            SetupTab(tabIMTransformers, "I&M - Transformers", gridIMTransformers);

            // tabSelector (MaterialTabSelector)
            this.tabSelector.BaseTabControl = this.tabControl;
            this.tabSelector.Dock = System.Windows.Forms.DockStyle.Top;
            this.tabSelector.Depth = 0;
            this.tabSelector.MouseState = MaterialSkin.MouseState.HOVER;
            this.tabSelector.TabIndex = 4;

            // pnlStatus
            this.pnlStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlStatus.Height = 26;
            this.pnlStatus.BackColor = System.Drawing.Color.FromArgb(37, 37, 40);
            this.pnlStatus.Padding = new System.Windows.Forms.Padding(6, 4, 6, 4);

            this.lblStatusRecords.AutoSize = true;
            this.lblStatusRecords.Location = new System.Drawing.Point(6, 5);
            this.lblStatusRecords.ForeColor = System.Drawing.Color.FromArgb(204, 204, 204);
            this.lblStatusRecords.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblStatusRecords.Text = "Records: 0";

            this.lblStatusConnection.AutoSize = true;
            this.lblStatusConnection.Location = new System.Drawing.Point(120, 5);
            this.lblStatusConnection.ForeColor = System.Drawing.Color.FromArgb(204, 204, 204);
            this.lblStatusConnection.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblStatusConnection.Text = "Connected: (none)";

            this.lblStatusUser.AutoSize = true;
            this.lblStatusUser.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.lblStatusUser.Location = new System.Drawing.Point(900, 5);
            this.lblStatusUser.ForeColor = System.Drawing.Color.FromArgb(204, 204, 204);
            this.lblStatusUser.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblStatusUser.Text = "User: ";

            this.pnlStatus.Controls.Add(this.lblStatusRecords);
            this.pnlStatus.Controls.Add(this.lblStatusConnection);
            this.pnlStatus.Controls.Add(this.lblStatusUser);

            // MainForm
            this.ClientSize = new System.Drawing.Size(1200, 764);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.tabSelector);
            this.Controls.Add(this.pnlStatus);
            this.Controls.Add(this.pnlSearch);
            this.Controls.Add(this.pnlToolbar);
            this.Text = "DRED – Device Record Established Database";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.MinimumSize = new System.Drawing.Size(800, 564);
            this.KeyPreview = true;

            this.pnlToolbar.ResumeLayout(false);
            this.pnlToolbar.PerformLayout();
            this.pnlSearch.ResumeLayout(false);
            this.pnlSearch.PerformLayout();
            this.tabControl.ResumeLayout(false);
            this.pnlStatus.ResumeLayout(false);
            this.pnlStatus.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private static System.Windows.Forms.DataGridView CreateGrid()
        {
            var g = new System.Windows.Forms.DataGridView
            {
                Dock = System.Windows.Forms.DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells,
                RowHeadersVisible = false,
                RowTemplate = { Height = 32 },
                BorderStyle = System.Windows.Forms.BorderStyle.None,
                CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal,
                BackgroundColor = System.Drawing.Color.FromArgb(30, 30, 30),
                GridColor = System.Drawing.Color.FromArgb(60, 60, 60),
                EnableHeadersVisualStyles = false,
            };

            g.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            g.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.White;
            g.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Segoe UI Semibold", 10F);
            g.ColumnHeadersDefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            g.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;

            g.DefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(45, 45, 48);
            g.DefaultCellStyle.ForeColor = System.Drawing.Color.FromArgb(241, 241, 241);
            g.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(25, 118, 210);
            g.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.White;
            g.DefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 9.5F);

            g.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(37, 37, 40);
            g.AlternatingRowsDefaultCellStyle.ForeColor = System.Drawing.Color.FromArgb(241, 241, 241);
            g.AlternatingRowsDefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(25, 118, 210);
            g.AlternatingRowsDefaultCellStyle.SelectionForeColor = System.Drawing.Color.White;

            return g;
        }

        private static void SetupTab(System.Windows.Forms.TabPage tab, string text,
            System.Windows.Forms.DataGridView grid)
        {
            tab.Text = text;
            tab.Controls.Add(grid);
        }

        private static void SetMaterialButton(MaterialSkin.Controls.MaterialButton btn,
            string text, MaterialSkin.Controls.MaterialButton.MaterialButtonType type,
            bool highEmphasis, System.EventHandler handler)
        {
            btn.Text = text;
            btn.Type = type;
            btn.HighEmphasis = highEmphasis;
            btn.AutoSize = true;
            btn.UseAccentColor = false;
            btn.Click += handler;
        }

        private System.Windows.Forms.FlowLayoutPanel pnlToolbar = null!;
        private MaterialSkin.Controls.MaterialButton btnAdd = null!;
        private MaterialSkin.Controls.MaterialButton btnEdit = null!;
        private MaterialSkin.Controls.MaterialButton btnDelete = null!;
        private MaterialSkin.Controls.MaterialButton btnRefresh = null!;
        private MaterialSkin.Controls.MaterialButton btnImport = null!;
        private MaterialSkin.Controls.MaterialButton btnExport = null!;
        private MaterialSkin.Controls.MaterialButton btnExportAll = null!;
        private MaterialSkin.Controls.MaterialButton btnAdvancedSearch = null!;
        private MaterialSkin.Controls.MaterialButton btnSettings = null!;
        private System.Windows.Forms.Panel pnlSearch = null!;
        private MaterialSkin.Controls.MaterialTextBox2 txtSearch = null!;
        private MaterialSkin.Controls.MaterialComboBox cboFilterColumn = null!;
        private MaterialSkin.Controls.MaterialTabControl tabControl = null!;
        private MaterialSkin.Controls.MaterialTabSelector tabSelector = null!;
        private System.Windows.Forms.TabPage tabOHMeters = null!;
        private System.Windows.Forms.TabPage tabIMMeters = null!;
        private System.Windows.Forms.TabPage tabOHTransformers = null!;
        private System.Windows.Forms.TabPage tabIMTransformers = null!;
        private System.Windows.Forms.DataGridView gridOHMeters = null!;
        private System.Windows.Forms.DataGridView gridIMMeters = null!;
        private System.Windows.Forms.DataGridView gridOHTransformers = null!;
        private System.Windows.Forms.DataGridView gridIMTransformers = null!;
        private System.Windows.Forms.Panel pnlStatus = null!;
        private System.Windows.Forms.Label lblStatusRecords = null!;
        private System.Windows.Forms.Label lblStatusConnection = null!;
        private System.Windows.Forms.Label lblStatusUser = null!;
    }
}

