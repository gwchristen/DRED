using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;

namespace DRED
{
    public partial class MainForm : MaterialForm
    {
        private static readonly string[] TabTableNames =
            { "OH_Meters", "IM_Meters", "OH_Transformers", "IM_Transformers" };

        private static readonly Color[] TabAccentColors = {
            Color.FromArgb(0x90, 0xA4, 0xAE), // Blue Grey - Dashboard
            Color.FromArgb(0x42, 0xA5, 0xF5), // Blue   - OH Meters
            Color.FromArgb(0x26, 0xA6, 0x9A), // Teal   - I&M Meters
            Color.FromArgb(0xFF, 0xA7, 0x26), // Orange - OH Transformers
            Color.FromArgb(0xAB, 0x47, 0xBC), // Purple - I&M Transformers
        };

        private static readonly Color SearchPanelDefaultColor = ThemeManager.SearchPanelColor;
        private static readonly Color SearchPanelFilteredColor = ThemeManager.SearchPanelFilteredColor;

        // Populated by Designer's SetupTabWithSplit; arrays allocated here as field initializers
        private System.Windows.Forms.SplitContainer[] _splitContainers = new System.Windows.Forms.SplitContainer[4];
        private System.Windows.Forms.ListBox[] _listBoxes   = new System.Windows.Forms.ListBox[4];
        private System.Windows.Forms.Panel[]   _detailPanels = new System.Windows.Forms.Panel[4];
        private System.Data.DataTable?[]       _dataTables   = new System.Data.DataTable?[4];
        private int[]                          _hoveredListItem = { -1, -1, -1, -1 };
        private readonly DetailPanelManager _detailPanelManager = new();
        private readonly DashboardManager _dashboardManager;
        private KeyboardShortcutHandler _shortcutHandler = null!;

        private const int  ListItemHeight     = 58;

        private static bool IsDataTab(int tabIndex)
            => tabIndex >= 1 && tabIndex <= TabTableNames.Length;

        private static int ToDataTabIndex(int tabIndex)
            => tabIndex - 1;

        private static int ToUiTabIndex(int dataTabIndex)
            => dataTabIndex + 1;

        private bool IsDashboardSelected => tabControl.SelectedIndex == 0;

        private string CurrentTable =>
            IsDataTab(tabControl.SelectedIndex)
                ? TabTableNames[ToDataTabIndex(tabControl.SelectedIndex)]
                : string.Empty;

        private AdvancedSearchCriteria? _advancedCriteria;
        private bool _dialogOpen  = false;
        private System.Windows.Forms.Timer _refreshTimer = null!;
        private System.Windows.Forms.Timer _backupTimer = null!;
        private bool _initialized = false;
        private bool _isUnlocked = false;
        private System.Windows.Forms.Panel _dashboardHostPanel = null!;

        // ── Nested types ─────────────────────────────────────────────────

        private record ListItem(string DevCode, string Qty, string PONumber, string RecvDate, int RowIndex);

        // ── Constructor ──────────────────────────────────────────────────

        public MainForm()
        {
            InitializeComponent();
            MaterialSkinManager.Instance.AddFormToManage(this);
            Logger.Log("Main form initialized.");
            _dashboardManager = new DashboardManager(TabTableNames, TabAccentColors[0], UpdateDashboardStatusBar);

            cboFilterColumn.Items.AddRange(new object[] {
                "All Columns", "OpCo2", "Status", "MFR", "DevCode", "BegSer", "EndSer",
                "PONumber", "Vintage", "CID", "MENumber", "PurCode", "Comments"
            });
            cboFilterColumn.SelectedIndex = 0;
            cboFilterColumn.SelectedIndexChanged += cboFilterColumn_SelectedIndexChanged;

            _detailPanelManager.Initialize(
                _detailPanels,
                new[] { TabAccentColors[1], TabAccentColors[2], TabAccentColors[3], TabAccentColors[4] },
                (s, e) => btnEdit_Click(s!, e),
                (s, e) => btnDelete_Click(s!, e),
                (s, e) => btnGenerate_Click(s!, e));
            _dashboardManager.Initialize(tabDashboard);
            _shortcutHandler = new KeyboardShortcutHandler(new KeyboardActions
            {
                AddRecord = () => btnAdd_Click(this, EventArgs.Empty),
                EditRecord = () => btnEdit_Click(this, EventArgs.Empty),
                DeleteRecord = () => btnDelete_Click(this, EventArgs.Empty),
                Undo = () => btnUndo_Click(this, EventArgs.Empty),
                Refresh = RefreshCurrentTab,
                FocusSearch = () => txtSearch.Focus(),
                ExportCurrentTab = () => btnExport_Click(this, EventArgs.Empty),
                ExportAllTabs = () => btnExportAll_Click(this, EventArgs.Empty),
                ImportFromExcel = () => btnImport_Click(this, EventArgs.Empty),
                OpenSettings = () => btnSettings_Click(this, EventArgs.Empty),
                OpenAdvancedSearch = () => btnAdvancedSearch_Click(this, EventArgs.Empty),
                SelectTab = SelectTabIfAvailable,
                ClearSearchAndFilters = () =>
                {
                    txtSearch.Text = "";
                    _advancedCriteria = null;
                    UpdateFilterIndicator();
                    RefreshCurrentTab();
                },
                IsUnlocked = () => _isUnlocked,
                IsUndoEnabled = () => btnUndo.Enabled,
                IsSearchFocused = () => txtSearch.Focused,
            });
            _isUnlocked = IsCurrentUserAuthorized();
            ApplyLockState();

            _refreshTimer = new System.Windows.Forms.Timer();
            _refreshTimer.Tick += (s, e) =>
            {
                if (!_dialogOpen && !IsDashboardSelected)
                    RefreshCurrentTab();
            };
            UpdateRefreshTimer();

            _backupTimer = new System.Windows.Forms.Timer();
            _backupTimer.Tick += (s, e) =>
            {
                if (_dialogOpen) return;
                try
                {
                    BackupManager.CreateBackup("scheduled");
                    BackupManager.CleanupOldBackups(AppSettings.MaxBackupCount);
                }
                catch (Exception ex)
                {
                    Logger.LogError("Scheduled backup failed.", ex);
                }
            };
            UpdateBackupTimer();
            UpdateFilterIndicator();

            this.Shown += (s, e) =>
            {
                this.BeginInvoke(new Action(() =>
                {
                    Logger.Log("Main form shown; loading initial tab data.");
                    foreach (var sc in _splitContainers)
                    {
                        if (sc == null) continue;

                        // Now that the form is visible with real dimensions,
                        // set the real minimum sizes and splitter position.
                        sc.Panel1MinSize = 200;
                        sc.Panel2MinSize = 200;

                        if (sc.Width > 0)
                        {
                            int maxDist = sc.Width - sc.Panel2MinSize - sc.SplitterWidth;
                            if (maxDist >= sc.Panel1MinSize)
                            {
                                try
                                {
                                    sc.SplitterDistance = Math.Clamp(300, sc.Panel1MinSize, maxDist);
                                }
                                catch (InvalidOperationException) { }
                            }
                        }
                    }

                    RefreshCurrentTab();
                    UpdateUndoState();
                }));
            };

            _initialized = true;
        }

        // ── Timer / refresh ──────────────────────────────────────────────

        private void UpdateRefreshTimer()
        {
            _refreshTimer.Stop();
            int interval = AppSettings.AutoRefreshInterval;
            if (interval > 0)
            {
                _refreshTimer.Interval = interval * 1000;
                _refreshTimer.Start();
            }
        }

        private void UpdateBackupTimer()
        {
            _backupTimer.Stop();
            int hours = AppSettings.BackupIntervalHours;
            if (hours > 0)
            {
                _backupTimer.Interval = (int)TimeSpan.FromHours(hours).TotalMilliseconds;
                _backupTimer.Start();
            }
        }

        private void RefreshCurrentTab()
        {
            if (!_initialized) return;
            try
            {
                int tabIndex = tabControl.SelectedIndex;
                if (tabIndex == 0)
                {
                    _dashboardManager.Refresh();
                    return;
                }

                string filter = txtSearch.Text.Trim();
                string filterColumn = cboFilterColumn.SelectedIndex > 0
                    ? cboFilterColumn.SelectedItem?.ToString() ?? ""
                    : "";
                LoadTable(tabIndex, filter, filterColumn, _advancedCriteria);
            }
            catch (Exception ex)
            {
                ShowDbError(ex);
            }
        }

        private void LoadTable(int tabIndex, string filter, string filterColumn = "",
            AdvancedSearchCriteria? advancedCriteria = null)
        {
            if (!IsDataTab(tabIndex)) return;
            int dataTabIndex = ToDataTabIndex(tabIndex);

            // Preserve current selection identity and scroll position before refresh
            var lb = _listBoxes[dataTabIndex];
            object? prevId = null;
            int prevTopIndex = lb.TopIndex;
            if (lb.SelectedItem is ListItem prevItem && _dataTables[dataTabIndex] != null)
            {
                var prevRow = _dataTables[dataTabIndex]!.Rows[prevItem.RowIndex];
                prevId = prevRow["Id"];
            }

            string table = TabTableNames[dataTabIndex];
            DataTable dt = DatabaseHelper.GetTableData(table, filter, filterColumn, advancedCriteria);
            _dataTables[dataTabIndex] = dt;
            PopulateListBox(lb, dt);
            UpdateStatusBar(dt.Rows.Count);

            if (lb.Items.Count > 0)
            {
                // Try to re-select the same record by Id; fall back to index 0
                int restoredIndex = 0;
                if (prevId != null)
                {
                    for (int i = 0; i < lb.Items.Count; i++)
                    {
                        if (lb.Items[i] is ListItem li && li.RowIndex < dt.Rows.Count
                            && dt.Rows[li.RowIndex]["Id"].Equals(prevId))
                        {
                            restoredIndex = i;
                            break;
                        }
                    }
                }
                lb.SelectedIndex = restoredIndex;
                // Restore scroll position (WinForms clamps TopIndex; only restore if Id was found)
                if (prevId != null)
                    lb.TopIndex = prevTopIndex;
            }
            else
            {
                _detailPanelManager.ShowEmptyState(dataTabIndex);
            }
        }

        // ── Status bar ───────────────────────────────────────────────────

        private void UpdateStatusBar(int recordCount)
        {
            lblStatusRecords.Text    = $"Records: {recordCount}";
            lblStatusConnection.Text = $"Connected: {AppSettings.DatabasePath}";
            UpdateUserLockStatusLabel();
        }

        private void UpdateDashboardStatusBar(int totalRecords)
        {
            lblStatusRecords.Text    = $"Dashboard: Total Records {totalRecords}";
            lblStatusConnection.Text = $"Connected: {AppSettings.DatabasePath}";
            UpdateUserLockStatusLabel();
        }

        private void UpdateUserLockStatusLabel()
        {
            lblStatusUser.Text = $"User: {Environment.UserName} {(_isUnlocked ? "🔓" : "🔒")}";
        }

        private bool IsCurrentUserAuthorized()
        {
            string currentUser = Environment.UserName;
            return AppSettings.AuthorizedUsers.Any(u =>
                string.Equals(u, currentUser, StringComparison.OrdinalIgnoreCase));
        }

        private void ApplyLockState()
        {
            bool enabled = _isUnlocked;
            bool dataTabSelected = IsDataTab(tabControl.SelectedIndex);

            btnUnlock.Text = enabled ? "🔓 Lock" : "🔒 Unlock";
            btnAdd.Enabled = enabled && dataTabSelected;
            btnEdit.Enabled = enabled && dataTabSelected;
            btnDelete.Enabled = enabled && dataTabSelected;

            mnuFileSettings.Enabled = enabled;
            mnuFileImport.Enabled = enabled;
            mnuTools.Enabled = enabled;
            mnuEdit.Enabled = enabled && dataTabSelected;
            btnUndo.Enabled = enabled && UndoManager.CanUndo;
            mnuEditUndo.Enabled = enabled && UndoManager.CanUndo;

            _detailPanelManager.SetEditDeleteEnabled(enabled);

            UpdateUserLockStatusLabel();
        }

        private void UpdateUndoState()
        {
            bool canUndo = _isUnlocked && UndoManager.CanUndo;
            btnUndo.Enabled = canUndo;
            mnuEditUndo.Enabled = canUndo;
        }

        // ── ListBox population ───────────────────────────────────────────

        private static void PopulateListBox(System.Windows.Forms.ListBox lb, DataTable dt)
        {
            lb.BeginUpdate();
            lb.Items.Clear();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                var row = dt.Rows[i];
                string devCode  = row["DevCode"] as string ?? "(no code)";
                string qty      = row["Qty"] is DBNull ? "\u2014" : Convert.ToString(row["Qty"]) ?? "\u2014";
                string poNum    = row["PONumber"] as string ?? "(no PO)";
                string recvDate = row["RecvDate"] is DBNull
                    ? "(no date)"
                    : Convert.ToDateTime(row["RecvDate"]).ToString("MM/dd/yyyy");
                lb.Items.Add(new ListItem(devCode, qty, poNum, recvDate, i));
            }
            lb.EndUpdate();
        }

        // ── Selected row helper ──────────────────────────────────────────

        private DataRow? GetSelectedRow()
        {
            int tabIndex = tabControl.SelectedIndex;
            if (!IsDataTab(tabIndex)) return null;
            int dataTabIndex = ToDataTabIndex(tabIndex);
            var lb = _listBoxes[dataTabIndex];
            if (lb == null || lb.SelectedIndex < 0 || _dataTables[dataTabIndex] == null) return null;
            if (lb.SelectedItem is not ListItem item) return null;
            int rowIdx = item.RowIndex;
            var dt = _dataTables[dataTabIndex]!;
            if (rowIdx < 0 || rowIdx >= dt.Rows.Count) return null;
            return dt.Rows[rowIdx];
        }

        // ── ListBox owner-draw handlers ───────────────────────────────────

        private void listBox_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            e.ItemHeight = ListItemHeight;
        }

        private void listBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || sender is not System.Windows.Forms.ListBox lb) return;
            if (lb.Items[e.Index] is not ListItem item) return;

            int   tabIndex = (int)(lb.Tag ?? 0);
            Color accent   = TabAccentColors[ToUiTabIndex(tabIndex)];

            bool selected = (e.State & DrawItemState.Selected) != 0;
            bool hovered  = _hoveredListItem[tabIndex] == e.Index;

            Color backColor;
            if (selected)
            {
                backColor = Color.FromArgb(
                    (int)(accent.R * 0.40 + 0x1E * 0.60),
                    (int)(accent.G * 0.40 + 0x1E * 0.60),
                    (int)(accent.B * 0.40 + 0x1E * 0.60));
            }
            else if (hovered)
            {
                backColor = Color.FromArgb(0x3A, 0x3A, 0x3E);
            }
            else
            {
                backColor = Color.FromArgb(0x1E, 0x1E, 0x1E);
            }

            var g = e.Graphics;
            using var backBrush = new SolidBrush(backColor);
            g.FillRectangle(backBrush, e.Bounds);

            int x = e.Bounds.X + 8;
            int y = e.Bounds.Y + 6;

            using var font1  = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
            using var font2  = new Font("Segoe UI", 8.5F);
            using var font3  = new Font("Segoe UI", 8F);
            using var brush1 = new SolidBrush(Color.White);
            using var brush2 = new SolidBrush(Color.FromArgb(0xCC, 0xCC, 0xCC));
            using var brush3 = new SolidBrush(Color.FromArgb(0x99, 0x99, 0x99));

            int maxW = e.Bounds.Width - 16;
            var fmt = System.Drawing.StringFormat.GenericDefault;
            fmt.Trimming    = System.Drawing.StringTrimming.EllipsisCharacter;
            fmt.FormatFlags = System.Drawing.StringFormatFlags.NoWrap;

            var r1 = new System.Drawing.RectangleF(x, y,      maxW, 18);
            var r2 = new System.Drawing.RectangleF(x, y + 18, maxW, 16);
            var r3 = new System.Drawing.RectangleF(x, y + 34, maxW, 14);

            g.DrawString($"{item.DevCode} - {item.PONumber}", font1, brush1, r1, fmt);
            g.DrawString($"Qty: {item.Qty}",                  font2, brush2, r2, fmt);
            g.DrawString(item.RecvDate,                       font3, brush3, r3, fmt);

            using var divPen = new System.Drawing.Pen(Color.FromArgb(0x30, 0x30, 0x33));
            g.DrawLine(divPen, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
        }

        private void listBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (sender is not System.Windows.Forms.ListBox lb) return;
            int tabIndex = (int)(lb.Tag ?? 0);
            int idx = lb.IndexFromPoint(e.Location);
            if (idx != _hoveredListItem[tabIndex])
            {
                int old = _hoveredListItem[tabIndex];
                _hoveredListItem[tabIndex] = idx;
                if (old >= 0 && old < lb.Items.Count)
                    lb.Invalidate(lb.GetItemRectangle(old));
                if (idx >= 0 && idx < lb.Items.Count)
                    lb.Invalidate(lb.GetItemRectangle(idx));
            }
        }

        private void listBox_MouseLeave(object sender, EventArgs e)
        {
            if (sender is not System.Windows.Forms.ListBox lb) return;
            int tabIndex = (int)(lb.Tag ?? 0);
            int old = _hoveredListItem[tabIndex];
            _hoveredListItem[tabIndex] = -1;
            if (old >= 0 && old < lb.Items.Count)
                lb.Invalidate(lb.GetItemRectangle(old));
        }

        private void listBox_DoubleClick(object sender, EventArgs e)
        {
            if (!_isUnlocked) return;
            btnEdit_Click(sender, e);
        }

        private void listBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (sender is not System.Windows.Forms.ListBox lb) return;
            int tabIndex = (int)(lb.Tag ?? 0);
            if (lb.SelectedIndex < 0 || _dataTables[tabIndex] == null)
            {
                _detailPanelManager.ShowEmptyState(tabIndex);
                return;
            }
            if (lb.SelectedItem is not ListItem item) { _detailPanelManager.ShowEmptyState(tabIndex); return; }
            int rowIdx = item.RowIndex;
            var dt = _dataTables[tabIndex]!;
            if (rowIdx < 0 || rowIdx >= dt.Rows.Count) { _detailPanelManager.ShowEmptyState(tabIndex); return; }
            _detailPanelManager.PopulateDetail(tabIndex, dt.Rows[rowIdx]);
        }

        // ── Tab / search event handlers ───────────────────────────────────

        private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            tabSelector?.Invalidate();
            Logger.Log(IsDashboardSelected
                ? "Switched to tab: Dashboard."
                : $"Switched to tab: {CurrentTable}.");
            ApplyLockState();
            RefreshCurrentTab();
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            btnClearSearch.Visible = !string.IsNullOrEmpty(txtSearch.Text);
            RefreshCurrentTab();
        }

        private void cboFilterColumn_SelectedIndexChanged(object? sender, EventArgs e)
        {
            RefreshCurrentTab();
        }

        private void btnClearSearch_Click(object sender, EventArgs e)
        {
            txtSearch.Text    = "";
            _advancedCriteria = null;
            UpdateFilterIndicator();
            RefreshCurrentTab();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshCurrentTab();
        }

        private void btnUndo_Click(object sender, EventArgs e)
        {
            var action = UndoManager.Pop();
            if (action == null)
            {
                UpdateUndoState();
                return;
            }

            try
            {
                switch (action.ActionType)
                {
                    case UndoActionType.Delete:
                        DatabaseHelper.InsertRecord(action.TableName, action.PreviousData);
                        break;
                    case UndoActionType.Edit:
                        DatabaseHelper.UpdateRecord(action.TableName, action.RecordId, action.PreviousData);
                        break;
                }

                Logger.Log($"Undo executed: {action.ActionType} on [{action.TableName}] record [{action.RecordId}] by {Environment.UserName}.");
                RefreshCurrentTab();
                MessageBox.Show("Last action undone.", "Undo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Logger.LogError("Undo action failed.", ex);
                ShowDbError(ex);
            }
            finally
            {
                UpdateUndoState();
            }
        }

        // ── CRUD handlers ────────────────────────────────────────────────

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!EnsureDataTabSelected("add")) return;
            _dialogOpen = true;
            try
            {
                using var form = new RecordForm();
                if (form.ShowDialog(this) == DialogResult.OK && form.Result != null)
                {
                    try
                    {
                        DatabaseHelper.InsertRecord(CurrentTable, form.Result);
                        Logger.Log($"User added a record to [{CurrentTable}].");
                        RefreshCurrentTab();
                        UpdateUndoState();
                    }
                    catch (Exception ex) { ShowDbError(ex); }
                }
            }
            finally
            {
                _dialogOpen = false;
                _detailPanelManager.ReapplyColors();
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (!EnsureDataTabSelected("edit")) return;
            var row = GetSelectedRow();
            if (row == null)
            {
                MessageBox.Show("Please select a record to edit.", "Edit Record",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int id = Convert.ToInt32(row["Id"]);

            if (!DatabaseHelper.TryLockRecord(CurrentTable, id, out string lockedBy))
            {
                MessageBox.Show(
                    $"This record is currently being edited by {lockedBy}. Please try again later.",
                    "Record Locked", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var existing = RowToRecordData(row);
            _dialogOpen = true;
            try
            {
                using var form = new RecordForm(existing);
                if (form.ShowDialog(this) == DialogResult.OK && form.Result != null)
                {
                    var undoAction = new UndoableAction
                    {
                        ActionType = UndoActionType.Edit,
                        TableName = CurrentTable,
                        RecordId = id,
                        PreviousData = existing,
                        Timestamp = DateTime.Now,
                        UserName = Environment.UserName,
                    };
                    try
                    {
                        UndoManager.Push(undoAction);
                        DatabaseHelper.UpdateRecord(CurrentTable, id, form.Result);
                        Logger.Log($"User edited record [{id}] in [{CurrentTable}].");
                        RefreshCurrentTab();
                        UpdateUndoState();
                    }
                    catch (Exception ex)
                    {
                        if (ReferenceEquals(UndoManager.Peek(), undoAction))
                            UndoManager.Pop();
                        ShowDbError(ex);
                    }
                }
            }
            finally
            {
                _dialogOpen = false;
                DatabaseHelper.UnlockRecord(CurrentTable, id);
                _detailPanelManager.ReapplyColors();
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (!EnsureDataTabSelected("delete")) return;
            var row = GetSelectedRow();
            if (row == null)
            {
                MessageBox.Show("Please select a record to delete.", "Delete Record",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int id = Convert.ToInt32(row["Id"]);

            if (MessageBox.Show("Are you sure you want to delete this record?", "Delete",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                var undoAction = new UndoableAction
                {
                    ActionType = UndoActionType.Delete,
                    TableName = CurrentTable,
                    RecordId = id,
                    PreviousData = RowToRecordData(row),
                    Timestamp = DateTime.Now,
                    UserName = Environment.UserName,
                };
                try
                {
                    UndoManager.Push(undoAction);
                    DatabaseHelper.DeleteRecord(CurrentTable, id);
                    Logger.Log($"User deleted record [{id}] from [{CurrentTable}].");
                    RefreshCurrentTab();
                    UpdateUndoState();
                }
                catch (Exception ex)
                {
                    if (ReferenceEquals(UndoManager.Peek(), undoAction))
                        UndoManager.Pop();
                    ShowDbError(ex);
                }
            }
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            try
            {
                using var form = new SettingsForm();
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    UpdateRefreshTimer();
                    UpdateBackupTimer();
                    if (!_isUnlocked && IsCurrentUserAuthorized())
                    {
                        _isUnlocked = true;
                        ApplyLockState();
                    }
                    RefreshCurrentTab();
                }
            }
            finally
            {
                _detailPanelManager.ReapplyColors();
            }
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            if (!EnsureDataTabSelected("generate")) return;
            var row = GetSelectedRow();
            if (row == null)
            {
                MessageBox.Show("Please select a record to generate serials for.", "Generate Serials",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int tabIndex = tabControl.SelectedIndex;
            if (!IsDataTab(tabIndex)) return;
            int dataTabIndex = ToDataTabIndex(tabIndex);
            bool isMeter = dataTabIndex == 0 || dataTabIndex == 1;

            string devCode = row["DevCode"] as string ?? "";
            string mfrCode = row["MFR"]     as string ?? "";
            string begSer  = row["BegSer"]  as string ?? "";
            string endSer  = row["EndSer"]  as string ?? "";
            int    qty     = row["Qty"] is DBNull ? 0 : Convert.ToInt32(row["Qty"]);
            string? oosRaw = row.Table.Columns.Contains("OOSSerials")
                ? row["OOSSerials"] as string : null;

            string lookupCode = "";
            if (isMeter)
            {
                var codes = LookupCodeManager.GetLookupCodes(devCode);
                if (codes.Count == 0)
                {
                    // Prompt user to enter manually
                    using var dlgInput = new LookupCodeInputDialog(devCode);
                    if (dlgInput.ShowDialog(this) != DialogResult.OK) return;
                    lookupCode = dlgInput.LookupCode;
                }
                else if (codes.Count == 1)
                {
                    lookupCode = codes[0];
                }
                else
                {
                    // Multiple lookup codes — ask user to pick one
                    using var dlgPick = new LookupCodePickerDialog(devCode, codes);
                    if (dlgPick.ShowDialog(this) != DialogResult.OK) return;
                    lookupCode = dlgPick.SelectedCode;
                }
            }

            List<string> serials;
            try
            {
                serials = SerialGenerator.Generate(isMeter, devCode, mfrCode, begSer, endSer, qty, oosRaw, lookupCode);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating serials:\n{ex.Message}", "Generate Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (serials.Count == 0)
            {
                MessageBox.Show("No serials could be generated for this record.", "Generate Serials",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            _dialogOpen = true;
            try
            {
                using var preview = new GeneratePreviewForm(serials, devCode);
                preview.ShowDialog(this);
            }
            finally
            {
                _dialogOpen = false;
                _detailPanelManager.ReapplyColors();
            }
        }

        private void btnLookupCodeEditor_Click(object sender, EventArgs e)
        {
            _dialogOpen = true;
            try
            {
                using var form = new LookupCodeEditorForm();
                form.ShowDialog(this);
            }
            finally
            {
                _dialogOpen = false;
                _detailPanelManager.ReapplyColors();
            }
        }

        private void btnAuditLog_Click(object sender, EventArgs e)
        {
            _dialogOpen = true;
            try
            {
                using var form = new AuditLogForm();
                form.ShowDialog(this);
            }
            finally
            {
                _dialogOpen = false;
                _detailPanelManager.ReapplyColors();
            }
        }

        private void btnAdvancedSearch_Click(object sender, EventArgs e)
        {
            _dialogOpen = true;
            try
            {
                using var form = new AdvancedSearchForm();
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    _advancedCriteria = form.Criteria;
                    UpdateFilterIndicator();
                    RefreshCurrentTab();
                }
            }
            finally
            {
                _dialogOpen = false;
                _detailPanelManager.ReapplyColors();
            }
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Title      = "Select Excel File to Import",
                Filter     = "Excel Workbook (*.xlsx)|*.xlsx",
                DefaultExt = "xlsx",
            };

            string defaultPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Created Histories.xlsx");
            if (File.Exists(defaultPath))
                dlg.FileName = defaultPath;

            if (dlg.ShowDialog(this) != DialogResult.OK) return;
            Logger.Log($"Starting import from '{dlg.FileName}'.");

            try
            {
                BackupManager.CreateBackup("pre-import");
                BackupManager.CleanupOldBackups(AppSettings.MaxBackupCount);
            }
            catch (Exception ex)
            {
                Logger.LogError("Pre-import backup failed.", ex);
                if (MessageBox.Show(
                        $"Unable to create backup before import:\n{ex.Message}\n\nContinue import anyway?",
                        "Backup Failed",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning) != DialogResult.Yes)
                {
                    return;
                }
            }

            var progress = new System.Text.StringBuilder();
            bool hasError = false;
            try
            {
                Cursor = Cursors.WaitCursor;
                ExcelImporter.Import(dlg.FileName, msg => progress.AppendLine(msg));
            }
            catch (Exception ex)
            {
                hasError = true;
                progress.AppendLine($"ERROR: {ex.Message}");
                Logger.LogError("Import failed.", ex);
            }
            finally
            {
                Cursor = Cursors.Default;
            }

            MessageBox.Show(progress.ToString(),
                hasError ? "Import \u2013 Errors Occurred" : "Import Complete",
                MessageBoxButtons.OK,
                hasError ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
            Logger.Log(hasError ? "Import completed with errors." : "Import completed successfully.");

            RefreshCurrentTab();
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            if (!EnsureDataTabSelected("export")) return;
            using var dlg = new SaveFileDialog
            {
                Title      = "Export to Excel",
                Filter     = "Excel Workbook (*.xlsx)|*.xlsx",
                DefaultExt = "xlsx",
                FileName   = $"{CurrentTable}_{DateTime.Now:yyyyMMdd}.xlsx",
            };
            if (dlg.ShowDialog(this) != DialogResult.OK) return;
            Logger.Log($"Export current tab [{CurrentTable}] to '{dlg.FileName}'.");

            try
            {
                Cursor = Cursors.WaitCursor;
                DatabaseHelper.ExportToExcel(CurrentTable, dlg.FileName);
                Cursor = Cursors.Default;
                MessageBox.Show($"Data exported to:\n{dlg.FileName}", "Export Complete",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                Logger.Log($"Export complete for [{CurrentTable}] to '{dlg.FileName}'.");
            }
            catch (Exception ex)
            {
                Cursor = Cursors.Default;
                ShowDbError(ex);
            }
        }

        private void btnExportAll_Click(object sender, EventArgs e)
        {
            using var dlg = new SaveFileDialog
            {
                Title      = "Export All Tabs to Excel",
                Filter     = "Excel Workbook (*.xlsx)|*.xlsx",
                DefaultExt = "xlsx",
                FileName   = $"DRED_Export_{DateTime.Now:yyyyMMdd}.xlsx",
            };
            if (dlg.ShowDialog(this) != DialogResult.OK) return;
            Logger.Log($"Export all tabs to '{dlg.FileName}'.");

            try
            {
                Cursor = Cursors.WaitCursor;
                DatabaseHelper.ExportAllToExcel(dlg.FileName);
                Cursor = Cursors.Default;
                MessageBox.Show($"All data exported to:\n{dlg.FileName}", "Export Complete",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                Logger.Log($"Export all tabs complete to '{dlg.FileName}'.");
            }
            catch (Exception ex)
            {
                Cursor = Cursors.Default;
                ShowDbError(ex);
            }
        }

        // ── Keyboard shortcuts ───────────────────────────────────────────

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (_shortcutHandler.HandleKeyDown(e)) return;
        }

        private void UpdateFilterIndicator()
        {
            bool isFiltered = _advancedCriteria != null;
            lblFilterActive.Visible = isFiltered;
            pnlSearch.BackColor = isFiltered ? SearchPanelFilteredColor : SearchPanelDefaultColor;
        }

        private void SelectTabIfAvailable(int tabIndex)
        {
            if (tabIndex >= 0 && tabIndex < tabControl.TabCount)
                tabControl.SelectedIndex = tabIndex;
        }

        private bool EnsureDataTabSelected(string action)
        {
            if (IsDataTab(tabControl.SelectedIndex)) return true;
            MessageBox.Show(
                $"Please switch to a data tab to {action} records.",
                "Dashboard",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return false;
        }

        private void btnUnlock_Click(object sender, EventArgs e)
        {
            if (_isUnlocked)
            {
                _isUnlocked = false;
                ApplyLockState();
                Logger.Log("Application locked by user.");
                return;
            }

            using var pinForm = new PinEntryForm();
            if (pinForm.ShowDialog(this) == DialogResult.OK)
            {
                _isUnlocked = true;
                ApplyLockState();
                Logger.Log("Application unlocked by user.");
            }
        }

        // ── Utilities ────────────────────────────────────────────────────

        private static RecordData RowToRecordData(DataRow row)
        {
            return new RecordData
            {
                OpCo2    = row["OpCo2"] as string,
                Status   = row["Status"] as string,
                MFR      = row["MFR"] as string,
                DevCode  = row["DevCode"] as string,
                BegSer   = row["BegSer"] as string,
                EndSer   = row["EndSer"] as string,
                Qty      = row["Qty"] is DBNull ? null : Convert.ToInt32(row["Qty"]),
                PODate   = row["PODate"] is DBNull ? null : Convert.ToDateTime(row["PODate"]),
                Vintage  = row["Vintage"] as string,
                PONumber = row["PONumber"] as string,
                RecvDate = row["RecvDate"] is DBNull ? null : Convert.ToDateTime(row["RecvDate"]),
                UnitCost = row["UnitCost"] is DBNull ? null : Convert.ToDecimal(row["UnitCost"]),
                CID      = row["CID"] as string,
                MENumber = row["MENumber"] as string,
                PurCode  = row["PurCode"] as string,
                Est      = row.Table.Columns.Contains("Est") && row["Est"] is not DBNull && Convert.ToBoolean(row["Est"]),
                TextFile = row.Table.Columns.Contains("TextFile") && row["TextFile"] is not DBNull && Convert.ToBoolean(row["TextFile"]),
                Comments = row["Comments"] as string,
                OOSSerials = row.Table.Columns.Contains("OOSSerials") ? row["OOSSerials"] as string : null,
                CreatedBy    = row.Table.Columns.Contains("CreatedBy") ? row["CreatedBy"] as string : null,
                CreatedDate  = row.Table.Columns.Contains("CreatedDate") && !(row["CreatedDate"] is DBNull)
                               ? Convert.ToDateTime(row["CreatedDate"]) : (DateTime?)null,
                ModifiedBy   = row.Table.Columns.Contains("ModifiedBy") ? row["ModifiedBy"] as string : null,
                ModifiedDate = row.Table.Columns.Contains("ModifiedDate") && !(row["ModifiedDate"] is DBNull)
                               ? Convert.ToDateTime(row["ModifiedDate"]) : (DateTime?)null,
            };
        }

        private const int OleDbErrorFileInUse    = -2147217887;
        private const int OleDbErrorRecordLocked = -2147217843;

        private static void ShowDbError(Exception ex)
        {
            Logger.LogError("Database/UI operation failed.", ex);
            string msg = ex.Message;
            if (ex is System.Data.OleDb.OleDbException oleEx)
            {
                msg = oleEx.ErrorCode == OleDbErrorFileInUse || oleEx.ErrorCode == OleDbErrorRecordLocked
                    ? "The database is locked by another user. Please try again.\n\n" + oleEx.Message
                    : oleEx.Message;
            }
            MessageBox.Show(msg, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
