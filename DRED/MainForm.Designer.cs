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
            // ── Menu strip ──────────────────────────────────────────────
            this.mainMenu            = new System.Windows.Forms.MenuStrip();
            this.mnuFile             = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuFileImport       = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuFileExport       = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuFileExportAll    = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuFileSep1         = new System.Windows.Forms.ToolStripSeparator();
            this.mnuFileSettings     = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuEdit             = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuEditAdd          = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuEditEdit         = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuEditDelete       = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuEditSep1         = new System.Windows.Forms.ToolStripSeparator();
            this.mnuEditRefresh      = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSearch           = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSearchFind       = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSearchAdvanced   = new System.Windows.Forms.ToolStripMenuItem();

            // ── Slim toolbar (3 buttons) ─────────────────────────────────
            this.pnlToolbar          = new System.Windows.Forms.FlowLayoutPanel();
            this.btnAdd              = new MaterialSkin.Controls.MaterialButton();
            this.btnRefresh          = new MaterialSkin.Controls.MaterialButton();
            this.btnAdvancedSearch   = new MaterialSkin.Controls.MaterialButton();

            // ── Hidden buttons kept for event wiring compatibility ───────
            this.btnEdit             = new MaterialSkin.Controls.MaterialButton();
            this.btnDelete           = new MaterialSkin.Controls.MaterialButton();
            this.btnImport           = new MaterialSkin.Controls.MaterialButton();
            this.btnExport           = new MaterialSkin.Controls.MaterialButton();
            this.btnExportAll        = new MaterialSkin.Controls.MaterialButton();
            this.btnSettings         = new MaterialSkin.Controls.MaterialButton();

            // ── Search panel ─────────────────────────────────────────────
            this.pnlSearch           = new System.Windows.Forms.Panel();
            this.txtSearch           = new MaterialSkin.Controls.MaterialTextBox2();
            this.cboFilterColumn     = new MaterialSkin.Controls.MaterialComboBox();
            this.btnClearSearch      = new MaterialSkin.Controls.MaterialButton();

            // ── Tab control ──────────────────────────────────────────────
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

            // ── Status bar ───────────────────────────────────────────────
            this.pnlStatus           = new System.Windows.Forms.Panel();
            this.tblStatus           = new System.Windows.Forms.TableLayoutPanel();
            this.lblStatusRecords    = new System.Windows.Forms.Label();
            this.lblStatusConnection = new System.Windows.Forms.Label();
            this.lblStatusUser       = new System.Windows.Forms.Label();

            // Wire CellDoubleClick for all grids
            this.gridOHMeters.CellDoubleClick       += new System.Windows.Forms.DataGridViewCellEventHandler(this.grid_CellDoubleClick);
            this.gridIMMeters.CellDoubleClick       += new System.Windows.Forms.DataGridViewCellEventHandler(this.grid_CellDoubleClick);
            this.gridOHTransformers.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.grid_CellDoubleClick);
            this.gridIMTransformers.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.grid_CellDoubleClick);

            this.mainMenu.SuspendLayout();
            this.pnlToolbar.SuspendLayout();
            this.pnlSearch.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.pnlStatus.SuspendLayout();
            this.tblStatus.SuspendLayout();
            this.SuspendLayout();

            // ── mainMenu ─────────────────────────────────────────────────
            this.mainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.mnuFile, this.mnuEdit, this.mnuSearch });
            this.mainMenu.Location = new System.Drawing.Point(0, 0);
            this.mainMenu.Name = "mainMenu";
            this.mainMenu.TabIndex = 0;

            // File menu
            this.mnuFile.Text = "&File";
            this.mnuFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                mnuFileImport, mnuFileExport, mnuFileExportAll, mnuFileSep1, mnuFileSettings });

            this.mnuFileImport.Text = "&Import from Excel...";
            this.mnuFileImport.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.I;
            this.mnuFileImport.Click += (s, e) => btnImport_Click(s, e);

            this.mnuFileExport.Text = "E&xport Current Tab...";
            this.mnuFileExport.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S;
            this.mnuFileExport.Click += (s, e) => btnExport_Click(s, e);

            this.mnuFileExportAll.Text = "Export &All Tabs...";
            this.mnuFileExportAll.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.S;
            this.mnuFileExportAll.Click += (s, e) => btnExportAll_Click(s, e);

            this.mnuFileSettings.Text = "&Settings";
            this.mnuFileSettings.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Oemcomma;
            this.mnuFileSettings.Click += (s, e) => btnSettings_Click(s, e);

            // Edit menu
            this.mnuEdit.Text = "&Edit";
            this.mnuEdit.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                mnuEditAdd, mnuEditEdit, mnuEditDelete, mnuEditSep1, mnuEditRefresh });

            this.mnuEditAdd.Text = "&Add Record";
            this.mnuEditAdd.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N;
            this.mnuEditAdd.Click += (s, e) => btnAdd_Click(s, e);

            this.mnuEditEdit.Text = "&Edit Record";
            this.mnuEditEdit.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E;
            this.mnuEditEdit.Click += (s, e) => btnEdit_Click(s, e);

            this.mnuEditDelete.Text = "&Delete Record";
            this.mnuEditDelete.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            this.mnuEditDelete.Click += (s, e) => btnDelete_Click(s, e);

            this.mnuEditRefresh.Text = "&Refresh";
            this.mnuEditRefresh.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.mnuEditRefresh.Click += (s, e) => btnRefresh_Click(s, e);

            // Search menu
            this.mnuSearch.Text = "&Search";
            this.mnuSearch.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                mnuSearchFind, mnuSearchAdvanced });

            this.mnuSearchFind.Text = "&Find...";
            this.mnuSearchFind.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F;
            this.mnuSearchFind.Click += (s, e) => txtSearch.Focus();

            this.mnuSearchAdvanced.Text = "&Advanced Search...";
            this.mnuSearchAdvanced.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.F;
            this.mnuSearchAdvanced.Click += (s, e) => btnAdvancedSearch_Click(s, e);

            // Apply dark theme to menu
            ThemeManager.ApplyDarkMenuStrip(this.mainMenu);

            // ── pnlToolbar ───────────────────────────────────────────────
            this.pnlToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlToolbar.Height = 56;
            this.pnlToolbar.Padding = new System.Windows.Forms.Padding(12, 12, 12, 8);
            this.pnlToolbar.WrapContents = false;
            this.pnlToolbar.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);

            SetMaterialButton(btnAdd,          "Add",              MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained, true,  this.btnAdd_Click);
            SetMaterialButton(btnRefresh,      "Refresh",          MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined,  false, this.btnRefresh_Click);
            SetMaterialButton(btnAdvancedSearch,"Advanced Search",  MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined,  false, this.btnAdvancedSearch_Click);
            btnAdd.AccessibleName           = "Add new record";
            btnRefresh.AccessibleName       = "Refresh current tab";
            btnAdvancedSearch.AccessibleName = "Open advanced search";

            // Hidden buttons kept for programmatic access / keyboard handler compatibility
            SetMaterialButton(btnEdit,    "Edit",       MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined, false, this.btnEdit_Click);
            SetMaterialButton(btnDelete,  "Delete",     MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined, false, this.btnDelete_Click);
            SetMaterialButton(btnImport,  "Import",     MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined, false, this.btnImport_Click);
            SetMaterialButton(btnExport,  "Export",     MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined, false, this.btnExport_Click);
            SetMaterialButton(btnExportAll,"Export All",MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined, false, this.btnExportAll_Click);
            SetMaterialButton(btnSettings, "Settings",  MaterialSkin.Controls.MaterialButton.MaterialButtonType.Text,     false, this.btnSettings_Click);
            // Make hidden buttons invisible so they don't take up space
            btnEdit.Visible = false;
            btnDelete.Visible = false;
            btnImport.Visible = false;
            btnExport.Visible = false;
            btnExportAll.Visible = false;
            btnSettings.Visible = false;

            this.pnlToolbar.Controls.AddRange(new System.Windows.Forms.Control[] {
                btnAdd, btnRefresh, btnAdvancedSearch });

            // ── pnlSearch ────────────────────────────────────────────────
            this.pnlSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlSearch.Height = 68;
            this.pnlSearch.Padding = new System.Windows.Forms.Padding(12, 8, 12, 8);
            this.pnlSearch.BackColor = System.Drawing.Color.FromArgb(37, 37, 40);

            // txtSearch
            this.txtSearch.Hint = "Search...";
            this.txtSearch.UseTallSize = false;
            this.txtSearch.Location = new System.Drawing.Point(12, 8);
            this.txtSearch.Size = new System.Drawing.Size(350, 48);
            this.txtSearch.TabIndex = 1;
            this.txtSearch.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);

            // cboFilterColumn
            this.cboFilterColumn.Location = new System.Drawing.Point(378, 18);
            this.cboFilterColumn.Size = new System.Drawing.Size(200, 36);
            this.cboFilterColumn.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboFilterColumn.TabIndex = 2;

            // btnClearSearch — small "✕" button, only visible when search has text
            this.btnClearSearch.Text = "✕";
            this.btnClearSearch.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Text;
            this.btnClearSearch.HighEmphasis = false;
            this.btnClearSearch.AutoSize = true;
            this.btnClearSearch.TabIndex = 3;
            this.btnClearSearch.Visible = false;
            this.btnClearSearch.Location = new System.Drawing.Point(590, 14);
            this.btnClearSearch.AccessibleName = "Clear search";
            this.btnClearSearch.Click += new System.EventHandler(this.btnClearSearch_Click);

            this.pnlSearch.Controls.Add(this.txtSearch);
            this.pnlSearch.Controls.Add(this.cboFilterColumn);
            this.pnlSearch.Controls.Add(this.btnClearSearch);

            // ── tabControl ───────────────────────────────────────────────
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.TabPages.AddRange(new System.Windows.Forms.TabPage[] {
                tabOHMeters, tabIMMeters, tabOHTransformers, tabIMTransformers });
            this.tabControl.TabIndex = 3;
            this.tabControl.SelectedIndexChanged += new System.EventHandler(this.tabControl_SelectedIndexChanged);

            SetupTabWithAccent(tabOHMeters,       "OH - Meters",        gridOHMeters,        System.Drawing.Color.FromArgb(0x42, 0xA5, 0xF5)); // Blue 400
            SetupTabWithAccent(tabIMMeters,       "I&M - Meters",       gridIMMeters,        System.Drawing.Color.FromArgb(0x26, 0xA6, 0x9A)); // Teal 400
            SetupTabWithAccent(tabOHTransformers, "OH - Transformers",  gridOHTransformers,  System.Drawing.Color.FromArgb(0xFF, 0xA7, 0x26)); // Orange 400
            SetupTabWithAccent(tabIMTransformers, "I&M - Transformers", gridIMTransformers,  System.Drawing.Color.FromArgb(0xAB, 0x47, 0xBC)); // Purple 400

            // ── tabSelector ──────────────────────────────────────────────
            this.tabSelector.BaseTabControl = this.tabControl;
            this.tabSelector.Dock = System.Windows.Forms.DockStyle.Top;
            this.tabSelector.Depth = 0;
            this.tabSelector.MouseState = MaterialSkin.MouseState.HOVER;
            this.tabSelector.TabIndex = 4;

            // ── pnlStatus ────────────────────────────────────────────────
            this.pnlStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlStatus.Height = 30;
            this.pnlStatus.BackColor = System.Drawing.Color.FromArgb(0x2A, 0x2A, 0x2D);
            this.pnlStatus.Padding = new System.Windows.Forms.Padding(12, 6, 12, 6);

            // Draw top border line on status panel
            this.pnlStatus.Paint += (s, e) =>
            {
                using var pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(0x3E, 0x3E, 0x42));
                e.Graphics.DrawLine(pen, 0, 0, pnlStatus.Width, 0);
            };

            // tblStatus — 3-column table layout for status sections
            this.tblStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblStatus.ColumnCount = 3;
            this.tblStatus.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33F));
            this.tblStatus.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 34F));
            this.tblStatus.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33F));
            this.tblStatus.RowCount = 1;
            this.tblStatus.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblStatus.BackColor = System.Drawing.Color.Transparent;
            this.tblStatus.Margin = System.Windows.Forms.Padding.Empty;

            // lblStatusRecords (left)
            this.lblStatusRecords.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblStatusRecords.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblStatusRecords.ForeColor = System.Drawing.Color.FromArgb(204, 204, 204);
            this.lblStatusRecords.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblStatusRecords.Text = "Records: 0";
            this.tblStatus.Controls.Add(this.lblStatusRecords, 0, 0);

            // lblStatusConnection (center)
            this.lblStatusConnection.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblStatusConnection.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblStatusConnection.ForeColor = System.Drawing.Color.FromArgb(204, 204, 204);
            this.lblStatusConnection.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblStatusConnection.Text = "Connected: (none)";
            this.tblStatus.Controls.Add(this.lblStatusConnection, 1, 0);

            // lblStatusUser (right)
            this.lblStatusUser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblStatusUser.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblStatusUser.ForeColor = System.Drawing.Color.FromArgb(204, 204, 204);
            this.lblStatusUser.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblStatusUser.Text = "User: ";
            this.tblStatus.Controls.Add(this.lblStatusUser, 2, 0);

            this.pnlStatus.Controls.Add(this.tblStatus);

            // ── MainForm ─────────────────────────────────────────────────
            this.ClientSize = new System.Drawing.Size(1200, 764);
            this.MainMenuStrip = this.mainMenu;
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.tabSelector);
            this.Controls.Add(this.pnlStatus);
            this.Controls.Add(this.pnlSearch);
            this.Controls.Add(this.pnlToolbar);
            this.Controls.Add(this.mainMenu);
            this.Text = "DRED – Device Record Established Database";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.MinimumSize = new System.Drawing.Size(800, 564);
            this.KeyPreview = true;

            this.mainMenu.ResumeLayout(false);
            this.mainMenu.PerformLayout();
            this.pnlToolbar.ResumeLayout(false);
            this.pnlToolbar.PerformLayout();
            this.pnlSearch.ResumeLayout(false);
            this.pnlSearch.PerformLayout();
            this.tabControl.ResumeLayout(false);
            this.tblStatus.ResumeLayout(false);
            this.tblStatus.PerformLayout();
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
                RowTemplate = { Height = 36 },
                BorderStyle = System.Windows.Forms.BorderStyle.None,
                CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal,
                BackgroundColor = System.Drawing.Color.FromArgb(30, 30, 30),
                GridColor = System.Drawing.Color.FromArgb(60, 60, 60),
                EnableHeadersVisualStyles = false,
                ColumnHeadersHeight = 40,
                ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
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

        private static void SetupTabWithAccent(System.Windows.Forms.TabPage tab, string text,
            System.Windows.Forms.DataGridView grid, System.Drawing.Color accentColor)
        {
            tab.Text = text;

            // Thin colored accent bar at top of tab page
            var accentBar = new System.Windows.Forms.Panel
            {
                Dock = System.Windows.Forms.DockStyle.Top,
                Height = 4,
                BackColor = accentColor,
            };

            // Add grid first, then accent bar (accent bar docks on top)
            tab.Controls.Add(grid);
            tab.Controls.Add(accentBar);
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

        // ── Field declarations ────────────────────────────────────────────
        private System.Windows.Forms.MenuStrip mainMenu = null!;
        private System.Windows.Forms.ToolStripMenuItem mnuFile = null!;
        private System.Windows.Forms.ToolStripMenuItem mnuFileImport = null!;
        private System.Windows.Forms.ToolStripMenuItem mnuFileExport = null!;
        private System.Windows.Forms.ToolStripMenuItem mnuFileExportAll = null!;
        private System.Windows.Forms.ToolStripSeparator mnuFileSep1 = null!;
        private System.Windows.Forms.ToolStripMenuItem mnuFileSettings = null!;
        private System.Windows.Forms.ToolStripMenuItem mnuEdit = null!;
        private System.Windows.Forms.ToolStripMenuItem mnuEditAdd = null!;
        private System.Windows.Forms.ToolStripMenuItem mnuEditEdit = null!;
        private System.Windows.Forms.ToolStripMenuItem mnuEditDelete = null!;
        private System.Windows.Forms.ToolStripSeparator mnuEditSep1 = null!;
        private System.Windows.Forms.ToolStripMenuItem mnuEditRefresh = null!;
        private System.Windows.Forms.ToolStripMenuItem mnuSearch = null!;
        private System.Windows.Forms.ToolStripMenuItem mnuSearchFind = null!;
        private System.Windows.Forms.ToolStripMenuItem mnuSearchAdvanced = null!;
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
        private MaterialSkin.Controls.MaterialButton btnClearSearch = null!;
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
        private System.Windows.Forms.TableLayoutPanel tblStatus = null!;
        private System.Windows.Forms.Label lblStatusRecords = null!;
        private System.Windows.Forms.Label lblStatusConnection = null!;
        private System.Windows.Forms.Label lblStatusUser = null!;
    }
}
