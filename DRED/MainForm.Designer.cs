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
                _backupTimer?.Dispose();
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
            this.mnuEditUndo         = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuEditAdd          = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuEditEdit         = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuEditDelete       = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuEditSep1         = new System.Windows.Forms.ToolStripSeparator();
            this.mnuEditRefresh      = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSearch           = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSearchFind       = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSearchAdvanced   = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuTools            = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuToolsLookupCodes = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuToolsAuditLog    = new System.Windows.Forms.ToolStripMenuItem();

            // ── Slim toolbar ─────────────────────────────────────────────
            this.pnlToolbar          = new System.Windows.Forms.FlowLayoutPanel();
            this.btnAdd              = new MaterialSkin.Controls.MaterialButton();
            this.btnUndo             = new MaterialSkin.Controls.MaterialButton();
            this.btnRefresh          = new MaterialSkin.Controls.MaterialButton();
            this.btnAdvancedSearch   = new MaterialSkin.Controls.MaterialButton();
            this.pnlToolbarSpacer    = new System.Windows.Forms.Panel();
            this.btnUnlock           = new MaterialSkin.Controls.MaterialButton();

            // ── Hidden buttons kept for event wiring compatibility ────────
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
            this.lblFilterActive     = new System.Windows.Forms.Label();

            // ── Tab control ──────────────────────────────────────────────
            this.tabControl          = new MaterialSkin.Controls.MaterialTabControl();
            this.tabSelector         = new System.Windows.Forms.Panel();
            this.tabDashboard        = new System.Windows.Forms.TabPage();
            this.tabOHMeters         = new System.Windows.Forms.TabPage();
            this.tabIMMeters         = new System.Windows.Forms.TabPage();
            this.tabOHTransformers   = new System.Windows.Forms.TabPage();
            this.tabIMTransformers   = new System.Windows.Forms.TabPage();

            // ── Status bar ───────────────────────────────────────────────
            this.pnlStatus           = new System.Windows.Forms.Panel();
            this.tblStatus           = new System.Windows.Forms.TableLayoutPanel();
            this.lblStatusRecords    = new System.Windows.Forms.Label();
            this.lblStatusConnection = new System.Windows.Forms.Label();
            this.lblStatusUser       = new System.Windows.Forms.Label();

            this.mainMenu.SuspendLayout();
            this.pnlToolbar.SuspendLayout();
            this.pnlSearch.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.pnlStatus.SuspendLayout();
            this.tblStatus.SuspendLayout();
            this.SuspendLayout();

            // ── mainMenu ─────────────────────────────────────────────────
            this.mainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.mnuFile, this.mnuEdit, this.mnuSearch, this.mnuTools });
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
                mnuEditUndo, mnuEditAdd, mnuEditEdit, mnuEditDelete, mnuEditSep1, mnuEditRefresh });

            this.mnuEditUndo.Text = "&Undo";
            this.mnuEditUndo.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z;
            this.mnuEditUndo.Enabled = false;
            this.mnuEditUndo.Click += (s, e) => btnUndo_Click(s, e);

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

            // Tools menu
            this.mnuTools.Text = "&Tools";
            this.mnuTools.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                mnuToolsLookupCodes, mnuToolsAuditLog });

            this.mnuToolsLookupCodes.Text = "&Lookup Code Editor...";
            this.mnuToolsLookupCodes.Click += (s, e) => btnLookupCodeEditor_Click(s, e);

            this.mnuToolsAuditLog.Text = "&Audit Log...";
            this.mnuToolsAuditLog.Click += (s, e) => btnAuditLog_Click(s, e);

            // Apply dark theme to menu
            ThemeManager.ApplyDarkMenuStrip(this.mainMenu);

            // ── pnlToolbar ───────────────────────────────────────────────
            this.pnlToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlToolbar.Height = 48;
            this.pnlToolbar.Padding = new System.Windows.Forms.Padding(8, 8, 8, 6);
            this.pnlToolbar.WrapContents = false;
            this.pnlToolbar.BackColor = ThemeManager.ToolbarColor;

            SetMaterialButton(btnAdd,           "Add",              MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained, true,  this.btnAdd_Click);
            SetMaterialButton(btnUndo,          "Undo",             MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined,  false, this.btnUndo_Click);
            SetMaterialButton(btnRefresh,       "Refresh",          MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined,  false, this.btnRefresh_Click);
            SetMaterialButton(btnAdvancedSearch, "Advanced Search",  MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined,  false, this.btnAdvancedSearch_Click);
            SetMaterialButton(btnUnlock,        "🔒 Unlock",        MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined,  false, this.btnUnlock_Click);
            btnAdd.AccessibleName            = "Add new record";
            btnUndo.AccessibleName           = "Undo last action";
            btnRefresh.AccessibleName        = "Refresh current tab";
            btnAdvancedSearch.AccessibleName = "Open advanced search";
            btnUnlock.AccessibleName         = "Lock or unlock protected actions";
            btnUndo.Enabled                  = false;

            this.pnlToolbarSpacer.Size = new System.Drawing.Size(0, 32);
            this.pnlToolbarSpacer.Margin = new System.Windows.Forms.Padding(0);
            this.pnlToolbarSpacer.BackColor = System.Drawing.Color.Transparent;

            // Hidden buttons kept for programmatic access / keyboard handler compatibility
            SetMaterialButton(btnEdit,     "Edit",        MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined, false, this.btnEdit_Click);
            SetMaterialButton(btnDelete,   "Delete",      MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined, false, this.btnDelete_Click);
            SetMaterialButton(btnImport,   "Import",      MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined, false, this.btnImport_Click);
            SetMaterialButton(btnExport,   "Export",      MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined, false, this.btnExport_Click);
            SetMaterialButton(btnExportAll,"Export All",  MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined, false, this.btnExportAll_Click);
            SetMaterialButton(btnSettings, "Settings",    MaterialSkin.Controls.MaterialButton.MaterialButtonType.Text,     false, this.btnSettings_Click);
            btnEdit.Visible      = false;
            btnDelete.Visible    = false;
            btnImport.Visible    = false;
            btnExport.Visible    = false;
            btnExportAll.Visible = false;
            btnSettings.Visible  = false;

            this.pnlToolbar.Controls.AddRange(new System.Windows.Forms.Control[] {
                btnAdd, btnUndo, btnRefresh, btnAdvancedSearch, pnlToolbarSpacer, btnUnlock });

            this.pnlToolbar.Resize += (s, e) =>
            {
                int reserved = pnlToolbar.Padding.Left + pnlToolbar.Padding.Right;
                foreach (System.Windows.Forms.Control c in pnlToolbar.Controls)
                {
                    if (c == pnlToolbarSpacer) continue;
                    reserved += c.Width + c.Margin.Left + c.Margin.Right;
                }

                int spacer = pnlToolbar.ClientSize.Width - reserved;
                pnlToolbarSpacer.Width = System.Math.Max(0, spacer);
            };

            // ── pnlSearch ────────────────────────────────────────────────
            // 74px accommodates the 42px search row plus the advanced-filter indicator label below it.
            const int searchPanelHeight = 74;
            this.pnlSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlSearch.Height = searchPanelHeight;
            this.pnlSearch.Padding = new System.Windows.Forms.Padding(8, 6, 8, 6);
            this.pnlSearch.BackColor = ThemeManager.SearchPanelColor;

            this.txtSearch.Hint = "Search...";
            this.txtSearch.UseTallSize = false;
            this.txtSearch.Location = new System.Drawing.Point(8, 6);
            this.txtSearch.Size = new System.Drawing.Size(350, 42);
            this.txtSearch.TabIndex = 1;
            this.txtSearch.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Top;
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);

            this.cboFilterColumn.Size = new System.Drawing.Size(200, 36);
            this.cboFilterColumn.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboFilterColumn.TabIndex = 2;
            this.cboFilterColumn.Anchor = System.Windows.Forms.AnchorStyles.None;

            this.btnClearSearch.Text = "✕";
            this.btnClearSearch.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Text;
            this.btnClearSearch.HighEmphasis = false;
            this.btnClearSearch.AutoSize = true;
            this.btnClearSearch.TabIndex = 3;
            this.btnClearSearch.Visible = false;
            this.btnClearSearch.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnClearSearch.AccessibleName = "Clear search";
            this.btnClearSearch.Click += new System.EventHandler(this.btnClearSearch_Click);

            this.lblFilterActive.AutoSize = true;
            this.lblFilterActive.Location = new System.Drawing.Point(12, 48);
            this.lblFilterActive.ForeColor = ThemeManager.FilterLabelColor;
            this.lblFilterActive.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblFilterActive.Text = "🔍 Advanced filter active";
            this.lblFilterActive.Visible = false;

            // Position cboFilterColumn and resize txtSearch whenever the search panel changes size.
            // This replaces the unreliable Anchor=Right approach (whose distances are computed
            // when the parent panel has width=0 during InitializeComponent, causing the control
            // to render off-screen).
            this.pnlSearch.Resize += (s, e) =>
            {
                const int leftMargin = 8;
                const int gap        = 8;
                const int cboWidth   = 200;
                int panelW = pnlSearch.ClientSize.Width;
                if (panelW < cboWidth + leftMargin * 2 + gap) return; // panel too narrow; controls keep prior positions
                int cboLeft = panelW - leftMargin - cboWidth;
                cboFilterColumn.Location  = new System.Drawing.Point(cboLeft, 9);
                txtSearch.Width           = cboLeft - leftMargin - gap;
                btnClearSearch.Location   = new System.Drawing.Point(cboLeft - leftMargin - 36, 10);
                lblFilterActive.MaximumSize = new System.Drawing.Size(cboLeft - leftMargin - gap, 0);
            };

            this.pnlSearch.Controls.Add(this.txtSearch);
            this.pnlSearch.Controls.Add(this.cboFilterColumn);
            this.pnlSearch.Controls.Add(this.btnClearSearch);
            this.pnlSearch.Controls.Add(this.lblFilterActive);

            // ── tabControl ───────────────────────────────────────────────
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.TabPages.AddRange(new System.Windows.Forms.TabPage[] {
                tabDashboard, tabOHMeters, tabIMMeters, tabOHTransformers, tabIMTransformers });
            this.tabControl.TabIndex = 3;
            this.tabControl.SelectedIndexChanged += new System.EventHandler(this.tabControl_SelectedIndexChanged);

            SetupDashboardTab(tabDashboard, "Dashboard");
            SetupTabWithSplit(tabOHMeters,       "OH - Meters",        System.Drawing.Color.FromArgb(0x42, 0xA5, 0xF5), 0);
            SetupTabWithSplit(tabIMMeters,       "I&M - Meters",       System.Drawing.Color.FromArgb(0x26, 0xA6, 0x9A), 1);
            SetupTabWithSplit(tabOHTransformers, "OH - Transformers",  System.Drawing.Color.FromArgb(0xFF, 0xA7, 0x26), 2);
            SetupTabWithSplit(tabIMTransformers, "I&M - Transformers", System.Drawing.Color.FromArgb(0xAB, 0x47, 0xBC), 3);

            // ── tabSelector (custom accent-colored tab strip) ────────────────
            // Replaces MaterialTabSelector so each tab can show its unique accent color.
            this.tabSelector.Dock       = System.Windows.Forms.DockStyle.Top;
            this.tabSelector.Height     = 48;
            this.tabSelector.BackColor  = ThemeManager.ToolbarColor;
            this.tabSelector.TabIndex   = 4;
            this.tabSelector.TabStop    = false;

            this.tabSelector.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                int count = tabControl.TabPages.Count;
                if (count == 0 || tabSelector.Width <= 0) return;
                float tabW  = (float)tabSelector.Width / count;
                int   sel   = tabControl.SelectedIndex;
                int   h     = tabSelector.Height;

                // Tab strip drawing constants
                const int   AccentBlend       = 5;   // divisor for blending accent into background
                const int   BaseBackground    = 28;  // base dark level for selected-tab tint
                const float TabFontSizeSelected   = 9F;
                const float TabFontSizeUnselected  = 8.5F;
                const string TabFontFamily = "Segoe UI";

                for (int i = 0; i < count; i++)
                {
                    bool isSelected = i == sel;
                    var  accent     = TabAccentColors[i];
                    float x = i * tabW;

                    // Background: subtle accent tint for selected tab, flat dark for others
                    using var bgBrush = new System.Drawing.SolidBrush(
                        isSelected
                            ? System.Drawing.Color.FromArgb(
                                Math.Min(255, accent.R / AccentBlend + BaseBackground),
                                Math.Min(255, accent.G / AccentBlend + BaseBackground),
                                Math.Min(255, accent.B / AccentBlend + BaseBackground))
                            : ThemeManager.ToolbarColor);
                    g.FillRectangle(bgBrush, x, 0, tabW, h);

                    // Label
                    string label = tabControl.TabPages[i].Text;
                    using var fgBrush = new System.Drawing.SolidBrush(
                        isSelected ? accent : ThemeManager.TabInactiveTextColor);
                    using var font = new System.Drawing.Font(TabFontFamily,
                        isSelected ? TabFontSizeSelected : TabFontSizeUnselected,
                        isSelected ? System.Drawing.FontStyle.Bold : System.Drawing.FontStyle.Regular);
                    var fmt = new System.Drawing.StringFormat
                    {
                        Alignment     = System.Drawing.StringAlignment.Center,
                        LineAlignment = System.Drawing.StringAlignment.Center,
                        Trimming      = System.Drawing.StringTrimming.EllipsisCharacter,
                        FormatFlags   = System.Drawing.StringFormatFlags.NoWrap,
                    };
                    g.DrawString(label, font, fgBrush,
                        new System.Drawing.RectangleF(x + 2, 0, tabW - 4, h - 3), fmt);

                    // Accent indicator bar at bottom of selected tab
                    if (isSelected)
                    {
                        using var bar = new System.Drawing.SolidBrush(accent);
                        g.FillRectangle(bar, x, h - 3, tabW, 3);
                    }

                    // Divider between tabs
                    if (i < count - 1)
                    {
                        using var sep = new System.Drawing.Pen(ThemeManager.TabDividerColor);
                        g.DrawLine(sep, x + tabW, 4, x + tabW, h - 4);
                    }
                }
            };

            this.tabSelector.MouseClick += (s, e) =>
            {
                int count = tabControl.TabPages.Count;
                if (count == 0 || tabSelector.Width <= 0) return;
                float tabW   = (float)tabSelector.Width / count;
                int clicked  = (int)(e.X / tabW);
                if (clicked >= 0 && clicked < count)
                    tabControl.SelectedIndex = clicked;
            };

            this.tabSelector.Resize += (s, e) => tabSelector.Invalidate();

            // ── pnlStatus ────────────────────────────────────────────────
            this.pnlStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlStatus.Height = 30;
            this.pnlStatus.BackColor = ThemeManager.StatusBarColor;
            this.pnlStatus.Padding = new System.Windows.Forms.Padding(8, 4, 8, 4);

            this.pnlStatus.Paint += (s, e) =>
            {
                using var pen = new System.Drawing.Pen(ThemeManager.StatusBorderColor);
                e.Graphics.DrawLine(pen, 0, 0, pnlStatus.Width, 0);
            };

            this.tblStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblStatus.ColumnCount = 3;
            this.tblStatus.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33F));
            this.tblStatus.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 34F));
            this.tblStatus.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33F));
            this.tblStatus.RowCount = 1;
            this.tblStatus.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblStatus.BackColor = System.Drawing.Color.Transparent;
            this.tblStatus.Margin = System.Windows.Forms.Padding.Empty;

            this.lblStatusRecords.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblStatusRecords.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblStatusRecords.ForeColor = ThemeManager.SecondaryTextColor;
            this.lblStatusRecords.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblStatusRecords.Text = "Records: 0";
            this.tblStatus.Controls.Add(this.lblStatusRecords, 0, 0);

            this.lblStatusConnection.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblStatusConnection.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblStatusConnection.ForeColor = ThemeManager.SecondaryTextColor;
            this.lblStatusConnection.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblStatusConnection.Text = "Connected: (none)";
            this.tblStatus.Controls.Add(this.lblStatusConnection, 1, 0);

            this.lblStatusUser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblStatusUser.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblStatusUser.ForeColor = ThemeManager.SecondaryTextColor;
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

        private void SetupTabWithSplit(System.Windows.Forms.TabPage tab, string text,
            System.Drawing.Color accentColor, int tabIndex)
        {
            tab.Text = text;

            var accentBar = new System.Windows.Forms.Panel
            {
                Dock      = System.Windows.Forms.DockStyle.Top,
                Height    = 4,
                BackColor = accentColor,
            };

            var split = new System.Windows.Forms.SplitContainer
            {
                Dock           = System.Windows.Forms.DockStyle.Fill,
                Orientation    = System.Windows.Forms.Orientation.Vertical,
                FixedPanel     = System.Windows.Forms.FixedPanel.Panel1,
                SplitterWidth  = 1,
                Panel1MinSize  = 0,
                Panel2MinSize  = 0,
                BackColor      = ThemeManager.SplitterColor,
            };
            split.Panel1.BackColor = ThemeManager.BackgroundColor;
            split.Panel2.BackColor = ThemeManager.SurfaceColor;

            split.Resize += (s, e) =>
            {
                if (split.Width <= 0) return;
                // Don't adjust splitter until real min sizes have been applied in Shown handler.
                if (split.Panel1MinSize == 0 && split.Panel2MinSize == 0) return;
                int maxDist = split.Width - split.Panel2MinSize - split.SplitterWidth;
                if (maxDist < split.Panel1MinSize) return;
                try
                {
                    if (split.SplitterDistance > maxDist)
                        split.SplitterDistance = maxDist;
                    else if (split.SplitterDistance < split.Panel1MinSize)
                        split.SplitterDistance = split.Panel1MinSize;
                }
                catch (InvalidOperationException) { }
            };

            var listBox = new System.Windows.Forms.ListBox
            {
                Dock         = System.Windows.Forms.DockStyle.Fill,
                DrawMode     = System.Windows.Forms.DrawMode.OwnerDrawVariable,
                BackColor    = ThemeManager.BackgroundColor,
                ForeColor    = System.Drawing.Color.White,
                BorderStyle  = System.Windows.Forms.BorderStyle.None,
                IntegralHeight = false,
                Tag          = tabIndex,
            };
            listBox.MeasureItem          += new System.Windows.Forms.MeasureItemEventHandler(this.listBox_MeasureItem);
            listBox.DrawItem             += new System.Windows.Forms.DrawItemEventHandler(this.listBox_DrawItem);
            listBox.SelectedIndexChanged += new System.EventHandler(this.listBox_SelectedIndexChanged);
            listBox.DoubleClick          += new System.EventHandler(this.listBox_DoubleClick);
            listBox.MouseMove            += new System.Windows.Forms.MouseEventHandler(this.listBox_MouseMove);
            listBox.MouseLeave           += new System.EventHandler(this.listBox_MouseLeave);

            split.Panel1.Controls.Add(listBox);

            var detailPanel = new System.Windows.Forms.Panel
            {
                Dock        = System.Windows.Forms.DockStyle.Fill,
                AutoScroll  = true,
                BackColor   = ThemeManager.SurfaceColor,
            };

            split.Panel2.Controls.Add(detailPanel);

            tab.Controls.Add(split);
            tab.Controls.Add(accentBar);

            _listBoxes[tabIndex]    = listBox;
            _detailPanels[tabIndex] = detailPanel;
            _splitContainers[tabIndex] = split;
        }

        private void SetupDashboardTab(System.Windows.Forms.TabPage tab, string text)
        {
            tab.Text = text;
            tab.BackColor = ThemeManager.BackgroundColor;

            var dashboardPanel = new System.Windows.Forms.Panel
            {
                Dock = System.Windows.Forms.DockStyle.Fill,
                AutoScroll = true,
                BackColor = ThemeManager.BackgroundColor,
                Padding = new System.Windows.Forms.Padding(10),
            };
            tab.Controls.Add(dashboardPanel);
            _dashboardHostPanel = dashboardPanel;
        }

        private static void SetMaterialButton(MaterialSkin.Controls.MaterialButton btn,
            string text, MaterialSkin.Controls.MaterialButton.MaterialButtonType type,
            bool highEmphasis, System.EventHandler handler)
        {
            btn.Text         = text;
            btn.Type         = type;
            btn.HighEmphasis = highEmphasis;
            btn.AutoSize     = true;
            btn.UseAccentColor = false;
            btn.Click       += handler;
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
        private System.Windows.Forms.ToolStripMenuItem mnuEditUndo = null!;
        private System.Windows.Forms.ToolStripMenuItem mnuEditAdd = null!;
        private System.Windows.Forms.ToolStripMenuItem mnuEditEdit = null!;
        private System.Windows.Forms.ToolStripMenuItem mnuEditDelete = null!;
        private System.Windows.Forms.ToolStripSeparator mnuEditSep1 = null!;
        private System.Windows.Forms.ToolStripMenuItem mnuEditRefresh = null!;
        private System.Windows.Forms.ToolStripMenuItem mnuSearch = null!;
        private System.Windows.Forms.ToolStripMenuItem mnuSearchFind = null!;
        private System.Windows.Forms.ToolStripMenuItem mnuSearchAdvanced = null!;
        private System.Windows.Forms.ToolStripMenuItem mnuTools = null!;
        private System.Windows.Forms.ToolStripMenuItem mnuToolsLookupCodes = null!;
        private System.Windows.Forms.ToolStripMenuItem mnuToolsAuditLog = null!;
        private System.Windows.Forms.FlowLayoutPanel pnlToolbar = null!;
        private MaterialSkin.Controls.MaterialButton btnAdd = null!;
        private MaterialSkin.Controls.MaterialButton btnUndo = null!;
        private System.Windows.Forms.Panel pnlToolbarSpacer = null!;
        private MaterialSkin.Controls.MaterialButton btnUnlock = null!;
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
        private System.Windows.Forms.Label lblFilterActive = null!;
        private MaterialSkin.Controls.MaterialTabControl tabControl = null!;
        private System.Windows.Forms.Panel tabSelector = null!;
        private System.Windows.Forms.TabPage tabDashboard = null!;
        private System.Windows.Forms.TabPage tabOHMeters = null!;
        private System.Windows.Forms.TabPage tabIMMeters = null!;
        private System.Windows.Forms.TabPage tabOHTransformers = null!;
        private System.Windows.Forms.TabPage tabIMTransformers = null!;
        private System.Windows.Forms.Panel pnlStatus = null!;
        private System.Windows.Forms.TableLayoutPanel tblStatus = null!;
        private System.Windows.Forms.Label lblStatusRecords = null!;
        private System.Windows.Forms.Label lblStatusConnection = null!;
        private System.Windows.Forms.Label lblStatusUser = null!;
    }
}
