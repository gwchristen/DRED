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
            this.toolStrip           = new System.Windows.Forms.ToolStrip();
            this.tsbAdd              = new System.Windows.Forms.ToolStripButton();
            this.tsbEdit             = new System.Windows.Forms.ToolStripButton();
            this.tsbDelete           = new System.Windows.Forms.ToolStripButton();
            this.tsSep1              = new System.Windows.Forms.ToolStripSeparator();
            this.tsbRefresh          = new System.Windows.Forms.ToolStripButton();
            this.tsSep2              = new System.Windows.Forms.ToolStripSeparator();
            this.tsbImport           = new System.Windows.Forms.ToolStripButton();
            this.tsbExport           = new System.Windows.Forms.ToolStripButton();
            this.tsbExportAll        = new System.Windows.Forms.ToolStripButton();
            this.tsSep3              = new System.Windows.Forms.ToolStripSeparator();
            this.tsbAdvancedSearch   = new System.Windows.Forms.ToolStripButton();
            this.tsSep4              = new System.Windows.Forms.ToolStripSeparator();
            this.tsbSettings         = new System.Windows.Forms.ToolStripButton();
            this.pnlSearch           = new System.Windows.Forms.Panel();
            this.lblSearch           = new System.Windows.Forms.Label();
            this.txtSearch           = new System.Windows.Forms.TextBox();
            this.cboFilterColumn     = new System.Windows.Forms.ComboBox();
            this.tabControl          = new System.Windows.Forms.TabControl();
            this.tabOHMeters         = new System.Windows.Forms.TabPage();
            this.tabIMMeters         = new System.Windows.Forms.TabPage();
            this.tabOHTransformers   = new System.Windows.Forms.TabPage();
            this.tabIMTransformers   = new System.Windows.Forms.TabPage();
            this.gridOHMeters        = CreateGrid();
            this.gridIMMeters        = CreateGrid();
            this.gridOHTransformers  = CreateGrid();
            this.gridIMTransformers  = CreateGrid();
            this.statusStrip         = new System.Windows.Forms.StatusStrip();
            this.lblStatusRecords    = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblStatusConnection = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblStatusUser       = new System.Windows.Forms.ToolStripStatusLabel();

            // Wire CellDoubleClick for all grids
            this.gridOHMeters.CellDoubleClick       += new System.Windows.Forms.DataGridViewCellEventHandler(this.grid_CellDoubleClick);
            this.gridIMMeters.CellDoubleClick       += new System.Windows.Forms.DataGridViewCellEventHandler(this.grid_CellDoubleClick);
            this.gridOHTransformers.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.grid_CellDoubleClick);
            this.gridIMTransformers.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.grid_CellDoubleClick);

            this.toolStrip.SuspendLayout();
            this.pnlSearch.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();

            // toolStrip
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                tsbAdd, tsbEdit, tsbDelete, tsSep1, tsbRefresh, tsSep2,
                tsbImport, tsbExport, tsbExportAll, tsSep3, tsbAdvancedSearch, tsSep4, tsbSettings });
            this.toolStrip.Location = new System.Drawing.Point(0, 0);
            this.toolStrip.TabIndex = 0;

            SetToolButton(tsbAdd,            "Add (Ctrl+N)",         "➕", this.btnAdd_Click);
            SetToolButton(tsbEdit,           "Edit (Ctrl+E)",         "✏",  this.btnEdit_Click);
            SetToolButton(tsbDelete,         "Delete (Del)",          "🗑", this.btnDelete_Click);
            SetToolButton(tsbRefresh,        "Refresh (F5)",          "🔄", this.btnRefresh_Click);
            SetToolButton(tsbImport,         "Import (Ctrl+I)",       "📥", this.btnImport_Click);
            SetToolButton(tsbExport,         "Export (Ctrl+S)",       "📤", this.btnExport_Click);
            SetToolButton(tsbExportAll,      "Export All (Ctrl+Shift+S)", "📦", this.btnExportAll_Click);
            SetToolButton(tsbAdvancedSearch, "Advanced Search (Ctrl+Shift+F)", "🔍", this.btnAdvancedSearch_Click);
            SetToolButton(tsbSettings,       "Settings (Ctrl+,)",     "⚙",  this.btnSettings_Click);

            // pnlSearch
            this.pnlSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlSearch.Height = 36;
            this.pnlSearch.Padding = new System.Windows.Forms.Padding(6, 4, 6, 4);
            this.lblSearch.Text = "Search:";
            this.lblSearch.AutoSize = true;
            this.lblSearch.Location = new System.Drawing.Point(6, 8);
            this.txtSearch.Location = new System.Drawing.Point(60, 6);
            this.txtSearch.Size = new System.Drawing.Size(280, 23);
            this.txtSearch.TabIndex = 1;
            this.txtSearch.PlaceholderText = "Filter across all text columns…";
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
            this.cboFilterColumn.Location = new System.Drawing.Point(348, 6);
            this.cboFilterColumn.Size = new System.Drawing.Size(150, 23);
            this.cboFilterColumn.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboFilterColumn.TabIndex = 2;
            this.pnlSearch.Controls.Add(this.lblSearch);
            this.pnlSearch.Controls.Add(this.txtSearch);
            this.pnlSearch.Controls.Add(this.cboFilterColumn);

            // tabControl
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.TabPages.AddRange(new System.Windows.Forms.TabPage[] {
                tabOHMeters, tabIMMeters, tabOHTransformers, tabIMTransformers });
            this.tabControl.TabIndex = 2;
            this.tabControl.SelectedIndexChanged += new System.EventHandler(this.tabControl_SelectedIndexChanged);

            SetupTab(tabOHMeters,       "OH - Meters",        gridOHMeters);
            SetupTab(tabIMMeters,       "I&M - Meters",       gridIMMeters);
            SetupTab(tabOHTransformers, "OH - Transformers",  gridOHTransformers);
            SetupTab(tabIMTransformers, "I&M - Transformers", gridIMTransformers);

            // statusStrip
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                lblStatusRecords, lblStatusConnection, lblStatusUser });
            this.statusStrip.SizingGrip = false;
            this.lblStatusRecords.Text = "Records: 0";
            this.lblStatusRecords.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.lblStatusConnection.Text = "Connected: (none)";
            this.lblStatusConnection.Spring = true;
            this.lblStatusConnection.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblStatusConnection.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.lblStatusUser.Text = "User: ";

            // MainForm
            this.ClientSize = new System.Drawing.Size(1200, 700);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.pnlSearch);
            this.Controls.Add(this.toolStrip);
            this.Text = "DRED – Device Record Established Database";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.MinimumSize = new System.Drawing.Size(800, 500);
            this.KeyPreview = true;

            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.pnlSearch.ResumeLayout(false);
            this.pnlSearch.PerformLayout();
            this.tabControl.ResumeLayout(false);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
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
                RowHeadersWidth = 30,
            };
            return g;
        }

        private static void SetupTab(System.Windows.Forms.TabPage tab, string text,
            System.Windows.Forms.DataGridView grid)
        {
            tab.Text = text;
            tab.Controls.Add(grid);
        }

        private static void SetToolButton(System.Windows.Forms.ToolStripButton btn,
            string text, string icon, System.EventHandler handler)
        {
            btn.Text = $"{icon} {text}";
            btn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            btn.Click += handler;
        }

        private System.Windows.Forms.ToolStrip toolStrip = null!;
        private System.Windows.Forms.ToolStripButton tsbAdd = null!;
        private System.Windows.Forms.ToolStripButton tsbEdit = null!;
        private System.Windows.Forms.ToolStripButton tsbDelete = null!;
        private System.Windows.Forms.ToolStripSeparator tsSep1 = null!;
        private System.Windows.Forms.ToolStripButton tsbRefresh = null!;
        private System.Windows.Forms.ToolStripSeparator tsSep2 = null!;
        private System.Windows.Forms.ToolStripButton tsbImport = null!;
        private System.Windows.Forms.ToolStripButton tsbExport = null!;
        private System.Windows.Forms.ToolStripButton tsbExportAll = null!;
        private System.Windows.Forms.ToolStripSeparator tsSep3 = null!;
        private System.Windows.Forms.ToolStripButton tsbAdvancedSearch = null!;
        private System.Windows.Forms.ToolStripSeparator tsSep4 = null!;
        private System.Windows.Forms.ToolStripButton tsbSettings = null!;
        private System.Windows.Forms.Panel pnlSearch = null!;
        private System.Windows.Forms.Label lblSearch = null!;
        private System.Windows.Forms.TextBox txtSearch = null!;
        private System.Windows.Forms.ComboBox cboFilterColumn = null!;
        private System.Windows.Forms.TabControl tabControl = null!;
        private System.Windows.Forms.TabPage tabOHMeters = null!;
        private System.Windows.Forms.TabPage tabIMMeters = null!;
        private System.Windows.Forms.TabPage tabOHTransformers = null!;
        private System.Windows.Forms.TabPage tabIMTransformers = null!;
        private System.Windows.Forms.DataGridView gridOHMeters = null!;
        private System.Windows.Forms.DataGridView gridIMMeters = null!;
        private System.Windows.Forms.DataGridView gridOHTransformers = null!;
        private System.Windows.Forms.DataGridView gridIMTransformers = null!;
        private System.Windows.Forms.StatusStrip statusStrip = null!;
        private System.Windows.Forms.ToolStripStatusLabel lblStatusRecords = null!;
        private System.Windows.Forms.ToolStripStatusLabel lblStatusConnection = null!;
        private System.Windows.Forms.ToolStripStatusLabel lblStatusUser = null!;
    }
}

