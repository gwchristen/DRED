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
            Color.FromArgb(0x42, 0xA5, 0xF5), // Blue   - OH Meters
            Color.FromArgb(0x26, 0xA6, 0x9A), // Teal   - I&M Meters
            Color.FromArgb(0xFF, 0xA7, 0x26), // Orange - OH Transformers
            Color.FromArgb(0xAB, 0x47, 0xBC), // Purple - I&M Transformers
        };

        private static readonly Color BooleanTrueColor  = Color.FromArgb(0x66, 0xBB, 0x6A); // Material Green 400
        private static readonly Color BooleanFalseColor = Color.FromArgb(0xEF, 0x53, 0x50); // Material Red 400
        private static readonly Color SearchPanelDefaultColor = ThemeManager.SearchPanelColor;
        private static readonly Color SearchPanelFilteredColor = ThemeManager.SearchPanelFilteredColor;

        // Populated by Designer's SetupTabWithSplit; arrays allocated here as field initializers
        private System.Windows.Forms.SplitContainer[] _splitContainers = new System.Windows.Forms.SplitContainer[4];
        private System.Windows.Forms.ListBox[] _listBoxes   = new System.Windows.Forms.ListBox[4];
        private System.Windows.Forms.Panel[]   _detailPanels = new System.Windows.Forms.Panel[4];
        private System.Data.DataTable?[]       _dataTables   = new System.Data.DataTable?[4];
        private DetailLabels[]                 _detailLabels = new DetailLabels[4];
        private int[]                          _hoveredListItem = { -1, -1, -1, -1 };

        private const int  ListItemHeight     = 58;

        private string CurrentTable =>
            TabTableNames[tabControl.SelectedIndex];

        private AdvancedSearchCriteria? _advancedCriteria;
        private bool _dialogOpen  = false;
        private System.Windows.Forms.Timer _refreshTimer = null!;
        private bool _initialized = false;
        private bool _isUnlocked = false;

        // ── Nested types ─────────────────────────────────────────────────

        private record ListItem(string DevCode, string Qty, string PONumber, string RecvDate, int RowIndex);

        private sealed class DetailLabels
        {
            public System.Windows.Forms.Label ValOpCo2   = null!, ValStatus   = null!,
                                              ValMFR      = null!, ValDevCode  = null!;
            public System.Windows.Forms.Label ValBegSer  = null!, ValEndSer   = null!, ValQty = null!,
                                              ValOOSSerials = null!;
            public System.Windows.Forms.Label ValPODate  = null!, ValPONumber = null!,
                                              ValVintage  = null!, ValRecvDate = null!, ValUnitCost = null!;
            public System.Windows.Forms.Label ValCID     = null!, ValMENumber = null!,
                                              ValPurCode  = null!, ValEst      = null!,
                                              ValTextFile = null!;
            public System.Windows.Forms.Label ValComments = null!;
            public System.Windows.Forms.Label LblAudit   = null!;
            public System.Windows.Forms.Label EmptyStateLabel = null!;
            public System.Windows.Forms.Panel ContentPanel    = null!;
            public MaterialSkin.Controls.MaterialButton BtnDetailEdit = null!, BtnDetailDelete = null!, BtnDetailGenerate = null!;
            // Section header labels (re-colored after MaterialSkinManager theme resets)
            public System.Windows.Forms.Label HdrDeviceInfo   = null!, HdrSerialRange  = null!,
                                              HdrPurchaseInfo = null!, HdrIdentifiers  = null!,
                                              HdrComments     = null!;
            // Field name labels ("OpCo2:", "Status:", etc.) and value labels — tracked so
            // ReapplyDetailPanelColors() can restore them after MaterialSkinManager resets them.
            public List<System.Windows.Forms.Label> FieldNameLabels = new();
            public List<System.Windows.Forms.Label> ValueLabels     = new();
        }

        // ── Constructor ──────────────────────────────────────────────────

        public MainForm()
        {
            InitializeComponent();
            MaterialSkinManager.Instance.AddFormToManage(this);
            Logger.Log("Main form initialized.");

            cboFilterColumn.Items.AddRange(new object[] {
                "All Columns", "OpCo2", "Status", "MFR", "DevCode", "BegSer", "EndSer",
                "PONumber", "Vintage", "CID", "MENumber", "PurCode", "Comments"
            });
            cboFilterColumn.SelectedIndex = 0;
            cboFilterColumn.SelectedIndexChanged += cboFilterColumn_SelectedIndexChanged;

            InitializeDetailPanels();
            _isUnlocked = IsCurrentUserAuthorized();
            ApplyLockState();

            _refreshTimer = new System.Windows.Forms.Timer();
            _refreshTimer.Tick += (s, e) => { if (!_dialogOpen) RefreshCurrentTab(); };
            UpdateRefreshTimer();
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

        private void RefreshCurrentTab()
        {
            if (!_initialized) return;
            try
            {
                int tabIndex = tabControl.SelectedIndex;
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
            if (tabIndex < 0 || tabIndex >= tabControl.TabPages.Count) return;

            // Preserve current selection identity and scroll position before refresh
            var lb = _listBoxes[tabIndex];
            object? prevId = null;
            int prevTopIndex = lb.TopIndex;
            if (lb.SelectedItem is ListItem prevItem && _dataTables[tabIndex] != null)
            {
                var prevRow = _dataTables[tabIndex]!.Rows[prevItem.RowIndex];
                prevId = prevRow["Id"];
            }

            string table = TabTableNames[tabIndex];
            DataTable dt = DatabaseHelper.GetTableData(table, filter, filterColumn, advancedCriteria);
            _dataTables[tabIndex] = dt;
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
                ShowEmptyDetailState(tabIndex);
            }
        }

        // ── Status bar ───────────────────────────────────────────────────

        private void UpdateStatusBar(int recordCount)
        {
            lblStatusRecords.Text    = $"Records: {recordCount}";
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

            btnUnlock.Text = enabled ? "🔓 Lock" : "🔒 Unlock";
            btnAdd.Enabled = enabled;
            btnEdit.Enabled = enabled;
            btnDelete.Enabled = enabled;

            mnuFileSettings.Enabled = enabled;
            mnuFileImport.Enabled = enabled;
            mnuTools.Enabled = enabled;
            mnuEdit.Enabled = enabled;
            btnUndo.Enabled = enabled && UndoManager.CanUndo;
            mnuEditUndo.Enabled = enabled && UndoManager.CanUndo;

            for (int i = 0; i < _detailLabels.Length; i++)
            {
                var detailLabels = _detailLabels[i];
                if (detailLabels == null) continue;

                detailLabels.BtnDetailEdit.Enabled = enabled;
                detailLabels.BtnDetailDelete.Enabled = enabled;
            }

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
            if (tabIndex < 0 || tabIndex >= 4) return null;
            var lb = _listBoxes[tabIndex];
            if (lb == null || lb.SelectedIndex < 0 || _dataTables[tabIndex] == null) return null;
            if (lb.SelectedItem is not ListItem item) return null;
            int rowIdx = item.RowIndex;
            var dt = _dataTables[tabIndex]!;
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
            Color accent   = TabAccentColors[tabIndex];

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
            if (lb.SelectedIndex < 0 || _dataTables[tabIndex] == null || _detailLabels == null)
            {
                ShowEmptyDetailState(tabIndex);
                return;
            }
            if (lb.SelectedItem is not ListItem item) { ShowEmptyDetailState(tabIndex); return; }
            int rowIdx = item.RowIndex;
            var dt = _dataTables[tabIndex]!;
            if (rowIdx < 0 || rowIdx >= dt.Rows.Count) { ShowEmptyDetailState(tabIndex); return; }
            PopulateDetailPanel(tabIndex, dt.Rows[rowIdx]);
        }

        // ── Detail panel initialisation ───────────────────────────────────

        private void InitializeDetailPanels()
        {
            _detailLabels = new DetailLabels[4];
            for (int i = 0; i < 4; i++)
                BuildDetailForTab(i, _detailPanels[i], TabAccentColors[i]);
        }

        private void ReapplyDetailPanelColors()
        {
            for (int i = 0; i < 4; i++)
            {
                var dl = _detailLabels[i];
                if (dl == null) continue;

                var accent         = TabAccentColors[i];
                var fieldNameColor = Color.FromArgb(0x99, 0x99, 0x99);
                var valueColor     = Color.FromArgb(0xF1, 0xF1, 0xF1);

                // Section headers
                dl.HdrDeviceInfo.ForeColor   = accent;
                dl.HdrSerialRange.ForeColor  = accent;
                dl.HdrPurchaseInfo.ForeColor = accent;
                dl.HdrIdentifiers.ForeColor  = accent;
                dl.HdrComments.ForeColor     = accent;

                // Field name labels
                foreach (var lbl in dl.FieldNameLabels)
                    lbl.ForeColor = fieldNameColor;

                // Value labels
                foreach (var lbl in dl.ValueLabels)
                    lbl.ForeColor = valueColor;

                // Re-apply boolean-specific colors that the bulk reset just overwrote
                dl.ValEst.ForeColor = dl.ValEst.Text.StartsWith("✔")
                    ? BooleanTrueColor : BooleanFalseColor;
                dl.ValTextFile.ForeColor = dl.ValTextFile.Text.StartsWith("✔")
                    ? BooleanTrueColor : BooleanFalseColor;

                // Audit label
                dl.LblAudit.ForeColor = Color.FromArgb(0x88, 0x88, 0x88);

                // Empty-state label
                dl.EmptyStateLabel.ForeColor = Color.FromArgb(0x66, 0x66, 0x66);

                // Panel backgrounds
                dl.ContentPanel.BackColor = Color.FromArgb(0x2D, 0x2D, 0x30);
            }
        }

        private void BuildDetailForTab(int tabIndex, System.Windows.Forms.Panel scrollPanel, Color accentColor)
        {
            var dl = new DetailLabels();

            // Empty state label
            var emptyLabel = new System.Windows.Forms.Label
            {
                Text      = "Select a record from the list",
                ForeColor = Color.FromArgb(0x66, 0x66, 0x66),
                Font      = new Font("Segoe UI", 12F),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock      = DockStyle.Fill,
                Visible   = true,
                BackColor = Color.Transparent,
            };
            dl.EmptyStateLabel = emptyLabel;
            scrollPanel.Controls.Add(emptyLabel);

            // Content panel — fills the detail panel
            var contentPanel = new System.Windows.Forms.Panel
            {
                Dock      = DockStyle.Fill,
                Padding   = new Padding(8),
                BackColor = Color.FromArgb(0x2D, 0x2D, 0x30),
                Visible   = false,
            };
            dl.ContentPanel = contentPanel;
            scrollPanel.Controls.Add(contentPanel);

            // Outer 2-column grid: [50%] [50%], 5 rows
            var outerGrid = new System.Windows.Forms.TableLayoutPanel
            {
                Dock            = DockStyle.Fill,
                ColumnCount     = 2,
                RowCount        = 5,
                BackColor       = Color.Transparent,
                CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.None,
                Padding         = Padding.Empty,
                Margin          = Padding.Empty,
            };
            outerGrid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50));
            outerGrid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50));
            outerGrid.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize)); // Row 0: devCard | serCard
            outerGrid.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize)); // Row 1: purCard | idCard
            outerGrid.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize)); // Row 2: commCard (full width)
            outerGrid.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24)); // Row 3: audit label
            outerGrid.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44)); // Row 4: buttons
            contentPanel.Controls.Add(outerGrid);

            // ── Helper: create a section card ────────────────────────────
            (System.Windows.Forms.TableLayoutPanel card, System.Windows.Forms.Label hdr) MakeCard(string title)
            {
                var card = new System.Windows.Forms.TableLayoutPanel
                {
                    ColumnCount  = 2,
                    RowCount     = 1,
                    AutoSize     = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    Dock         = DockStyle.Fill,
                    BackColor    = Color.FromArgb(0x25, 0x25, 0x28),
                    Margin       = new Padding(4),
                    Padding      = new Padding(8, 6, 8, 8),
                    CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.None,
                };
                card.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 75));
                card.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100));
                // Header
                var hdr = new System.Windows.Forms.Label
                {
                    Text      = title,
                    ForeColor = accentColor,
                    Font      = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold),
                    Dock      = DockStyle.Fill,
                    TextAlign = ContentAlignment.BottomLeft,
                    BackColor = Color.Transparent,
                    Padding   = new Padding(0, 0, 0, 2),
                };
                card.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26));
                card.Controls.Add(hdr, 0, 0);
                card.SetColumnSpan(hdr, 2);
                return (card, hdr);
            }

            // ── Helper: add a field row to a card ────────────────────────
            System.Windows.Forms.Label AddField(System.Windows.Forms.TableLayoutPanel card, string labelText)
            {
                int r = card.RowCount++;
                card.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22));
                var nm = new System.Windows.Forms.Label
                {
                    Text      = labelText,
                    ForeColor = Color.FromArgb(0x99, 0x99, 0x99),
                    Font      = new Font("Segoe UI", 8F),
                    Dock      = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft,
                    BackColor = Color.Transparent,
                };
                var vl = new System.Windows.Forms.Label
                {
                    Text      = "\u2014",
                    ForeColor = Color.FromArgb(0xF1, 0xF1, 0xF1),
                    Font      = new Font("Segoe UI", 9F),
                    Dock      = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft,
                    BackColor = Color.Transparent,
                };
                card.Controls.Add(nm, 0, r);
                card.Controls.Add(vl, 1, r);
                dl.FieldNameLabels.Add(nm);
                dl.ValueLabels.Add(vl);
                return vl;
            }

            // ── DEVICE INFORMATION (col 0, row 0) ────────────────────────
            var (devCard, devHdr) = MakeCard("DEVICE INFORMATION");
            dl.HdrDeviceInfo = devHdr;
            dl.ValOpCo2   = AddField(devCard, "OpCo2:");
            dl.ValStatus  = AddField(devCard, "Status:");
            dl.ValMFR     = AddField(devCard, "MFR:");
            dl.ValDevCode = AddField(devCard, "Dev Code:");
            outerGrid.Controls.Add(devCard, 0, 0);

            // ── SERIAL RANGE & QUANTITY (col 1, row 0) ───────────────────
            var (serCard, serHdr) = MakeCard("SERIAL RANGE & QUANTITY");
            dl.HdrSerialRange = serHdr;
            dl.ValBegSer     = AddField(serCard, "Beg Ser:");
            dl.ValEndSer     = AddField(serCard, "End Ser:");
            dl.ValQty        = AddField(serCard, "Qty:");
            dl.ValOOSSerials = AddField(serCard, "OOS:");
            outerGrid.Controls.Add(serCard, 1, 0);

            // ── PURCHASE INFORMATION (col 0, row 1) ──────────────────────
            var (purCard, purHdr) = MakeCard("PURCHASE INFORMATION");
            dl.HdrPurchaseInfo = purHdr;
            dl.ValPODate   = AddField(purCard, "PO Date:");
            dl.ValPONumber = AddField(purCard, "PO Number:");
            dl.ValVintage  = AddField(purCard, "Vintage:");
            dl.ValRecvDate = AddField(purCard, "Est Date:");
            dl.ValUnitCost = AddField(purCard, "Unit Cost:");
            outerGrid.Controls.Add(purCard, 0, 1);

            // ── IDENTIFIERS (col 1, row 1) ───────────────────────────────
            var (idCard, idHdr) = MakeCard("IDENTIFIERS");
            dl.HdrIdentifiers = idHdr;
            dl.ValCID      = AddField(idCard, "CID:");
            dl.ValMENumber = AddField(idCard, "M.E. #:");
            dl.ValPurCode  = AddField(idCard, "Pur Code:");
            dl.ValEst      = AddField(idCard, "Est.:");
            dl.ValTextFile = AddField(idCard, "Text File:");
            outerGrid.Controls.Add(idCard, 1, 1);

            // ── COMMENTS & NOTES (full width, row 2) ─────────────────────
            var (commCard, commHdr) = MakeCard("COMMENTS & NOTES");
            dl.HdrComments = commHdr;
            int commRow = commCard.RowCount++;
            commCard.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 54));
            var commLabel = new System.Windows.Forms.Label
            {
                Text      = "\u2014",
                ForeColor = Color.FromArgb(0xF1, 0xF1, 0xF1),
                Font      = new Font("Segoe UI", 9F),
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.TopLeft,
                BackColor = Color.Transparent,
            };
            commCard.Controls.Add(commLabel, 0, commRow);
            commCard.SetColumnSpan(commLabel, 2);
            dl.ValComments = commLabel;
            dl.ValueLabels.Add(commLabel);
            outerGrid.Controls.Add(commCard, 0, 2);
            outerGrid.SetColumnSpan(commCard, 2);

            // ── Audit label (full width, row 3) ──────────────────────────
            var auditLabel = new System.Windows.Forms.Label
            {
                Text      = "",
                ForeColor = Color.FromArgb(0x88, 0x88, 0x88),
                Font      = new Font("Segoe UI", 8F, FontStyle.Italic),
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent,
                Margin    = new Padding(8, 0, 0, 0),
            };
            dl.LblAudit = auditLabel;
            outerGrid.Controls.Add(auditLabel, 0, 3);
            outerGrid.SetColumnSpan(auditLabel, 2);

            // ── Action buttons (full width, row 4) ───────────────────────
            var btnDetailEdit     = CreateDetailButton("Edit",     accentColor,                      (s, ev) => btnEdit_Click(s!, ev));
            var btnDetailDelete   = CreateDetailButton("Delete",   Color.FromArgb(0xEF, 0x53, 0x50), (s, ev) => btnDelete_Click(s!, ev));
            var btnDetailGenerate = CreateDetailButton("Generate", accentColor,                      (s, ev) => btnGenerate_Click(s!, ev));
            var btnPanel = new System.Windows.Forms.FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents  = false,
                AutoSize      = false,
                Dock          = DockStyle.Fill,
                BackColor     = Color.Transparent,
                Margin        = Padding.Empty,
                Padding       = Padding.Empty,
            };
            btnPanel.Controls.Add(btnDetailEdit);
            btnPanel.Controls.Add(btnDetailDelete);
            btnPanel.Controls.Add(btnDetailGenerate);
            outerGrid.Controls.Add(btnPanel, 0, 4);
            outerGrid.SetColumnSpan(btnPanel, 2);
            dl.BtnDetailEdit     = btnDetailEdit;
            dl.BtnDetailDelete   = btnDetailDelete;
            dl.BtnDetailGenerate = btnDetailGenerate;

            _detailLabels[tabIndex] = dl;
        }

        private MaterialSkin.Controls.MaterialButton CreateDetailButton(
            string text, Color color, EventHandler handler)
        {
            var btn = new MaterialSkin.Controls.MaterialButton
            {
                Text           = text,
                Type           = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined,
                HighEmphasis   = false,
                UseAccentColor = false,
                AutoSize       = true,
            };
            btn.Click += handler;
            return btn;
        }

        // ── Detail panel population ──────────────────────────────────────

        private void PopulateDetailPanel(int tabIndex, DataRow row)
        {
            var dl = _detailLabels[tabIndex];
            if (dl == null) return;

            static string S(object? v) => v is DBNull || v == null ? "\u2014" : v.ToString() ?? "\u2014";
            static string D(object? v) => v is DBNull || v == null ? "\u2014" : Convert.ToDateTime(v).ToString("MM/dd/yyyy");
            static string C(object? v) => v is DBNull || v == null ? "\u2014" : Convert.ToDecimal(v).ToString("$#,##0.00");

            dl.ValOpCo2.Text   = S(row["OpCo2"]);
            dl.ValStatus.Text  = S(row["Status"]);
            dl.ValMFR.Text     = S(row["MFR"]);
            dl.ValDevCode.Text = S(row["DevCode"]);

            dl.ValBegSer.Text = S(row["BegSer"]);
            dl.ValEndSer.Text = S(row["EndSer"]);
            dl.ValQty.Text    = S(row["Qty"]);

            string oosRaw = row.Table.Columns.Contains("OOSSerials") && !(row["OOSSerials"] is DBNull)
                ? row["OOSSerials"] as string ?? "" : "";
            int oosCount = string.IsNullOrEmpty(oosRaw) ? 0
                : oosRaw.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                         .Count(s => !string.IsNullOrWhiteSpace(s));
            dl.ValOOSSerials.Text = oosCount > 0 ? oosCount.ToString() : "\u2014";

            dl.ValPODate.Text   = D(row["PODate"]);
            dl.ValPONumber.Text = S(row["PONumber"]);
            dl.ValVintage.Text  = S(row["Vintage"]);
            dl.ValRecvDate.Text = D(row["RecvDate"]);
            dl.ValUnitCost.Text = C(row["UnitCost"]);

            dl.ValCID.Text      = S(row["CID"]);
            dl.ValMENumber.Text = S(row["MENumber"]);
            dl.ValPurCode.Text  = S(row["PurCode"]);
            bool estVal = row.Table.Columns.Contains("Est") && row["Est"] is not DBNull && Convert.ToBoolean(row["Est"]);
            dl.ValEst.Text = estVal ? "✔ Yes" : "✘ No";
            dl.ValEst.ForeColor = estVal ? BooleanTrueColor : BooleanFalseColor;

            bool tfVal = row.Table.Columns.Contains("TextFile") && row["TextFile"] is not DBNull && Convert.ToBoolean(row["TextFile"]);
            dl.ValTextFile.Text = tfVal ? "✔ Yes" : "✘ No";
            dl.ValTextFile.ForeColor = tfVal ? BooleanTrueColor : BooleanFalseColor;

            dl.ValComments.Text = S(row["Comments"]);

            var auditParts = new List<string>();
            if (row.Table.Columns.Contains("CreatedBy") &&
                !(row["CreatedBy"] is DBNull) &&
                row["CreatedBy"] is string cb && cb.Length > 0)
            {
                string cd = row.Table.Columns.Contains("CreatedDate") && !(row["CreatedDate"] is DBNull)
                    ? Convert.ToDateTime(row["CreatedDate"]).ToString("MM/dd/yyyy") : "";
                auditParts.Add($"Created by {cb}" + (cd.Length > 0 ? $" on {cd}" : ""));
            }
            if (row.Table.Columns.Contains("ModifiedBy") &&
                !(row["ModifiedBy"] is DBNull) &&
                row["ModifiedBy"] is string mb && mb.Length > 0)
            {
                string md = row.Table.Columns.Contains("ModifiedDate") && !(row["ModifiedDate"] is DBNull)
                    ? Convert.ToDateTime(row["ModifiedDate"]).ToString("MM/dd/yyyy") : "";
                auditParts.Add($"Modified by {mb}" + (md.Length > 0 ? $" on {md}" : ""));
            }
            dl.LblAudit.Text = string.Join("   \u2022   ", auditParts);

            dl.EmptyStateLabel.Visible = false;
            dl.ContentPanel.Visible    = true;
        }

        private void ShowEmptyDetailState(int tabIndex)
        {
            if (_detailLabels == null || tabIndex < 0 || tabIndex >= _detailLabels.Length) return;
            var dl = _detailLabels[tabIndex];
            if (dl == null) return;
            dl.ContentPanel.Visible    = false;
            dl.EmptyStateLabel.Visible = true;
        }

        // ── Tab / search event handlers ───────────────────────────────────

        private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            tabSelector?.Invalidate();
            Logger.Log($"Switched to tab: {CurrentTable}.");
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
                ReapplyDetailPanelColors();
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
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
                ReapplyDetailPanelColors();
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
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
                ReapplyDetailPanelColors();
            }
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            var row = GetSelectedRow();
            if (row == null)
            {
                MessageBox.Show("Please select a record to generate serials for.", "Generate Serials",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int tabIndex = tabControl.SelectedIndex;
            bool isMeter = tabIndex == 0 || tabIndex == 1;

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
                ReapplyDetailPanelColors();
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
                ReapplyDetailPanelColors();
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
                ReapplyDetailPanelColors();
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

            if (e.Control && e.Shift)
            {
                switch (e.KeyCode)
                {
                    case Keys.F:
                        e.Handled = true;
                        btnAdvancedSearch_Click(this, EventArgs.Empty);
                        return;
                    case Keys.S:
                        e.Handled = true;
                        btnExportAll_Click(this, EventArgs.Empty);
                        return;
                }
            }

            if (e.Control)
            {
                switch (e.KeyCode)
                {
                    case Keys.N:
                        e.Handled = true;
                        if (_isUnlocked)
                            btnAdd_Click(this, EventArgs.Empty);
                        return;
                    case Keys.E:
                        e.Handled = true;
                        if (_isUnlocked)
                            btnEdit_Click(this, EventArgs.Empty);
                        return;
                    case Keys.Z:
                        e.Handled = true;
                        if (btnUndo.Enabled)
                            btnUndo_Click(this, EventArgs.Empty);
                        return;
                    case Keys.R:
                        e.Handled = true;
                        RefreshCurrentTab();
                        return;
                    case Keys.F:
                        e.Handled = true;
                        txtSearch.Focus();
                        return;
                    case Keys.S:
                        e.Handled = true;
                        btnExport_Click(this, EventArgs.Empty);
                        return;
                    case Keys.I:
                        e.Handled = true;
                        if (_isUnlocked)
                            btnImport_Click(this, EventArgs.Empty);
                        return;
                    case Keys.Oemcomma:
                        e.Handled = true;
                        if (_isUnlocked)
                            btnSettings_Click(this, EventArgs.Empty);
                        return;
                    case Keys.D1:
                        e.Handled = true;
                        SelectTabIfAvailable(0);
                        return;
                    case Keys.D2:
                        e.Handled = true;
                        SelectTabIfAvailable(1);
                        return;
                    case Keys.D3:
                        e.Handled = true;
                        SelectTabIfAvailable(2);
                        return;
                    case Keys.D4:
                        e.Handled = true;
                        SelectTabIfAvailable(3);
                        return;
                }
            }

            switch (e.KeyCode)
            {
                case Keys.F5:
                    e.Handled = true;
                    RefreshCurrentTab();
                    return;
                case Keys.Delete:
                    if (!txtSearch.Focused)
                    {
                        e.Handled = true;
                        if (_isUnlocked)
                            btnDelete_Click(this, EventArgs.Empty);
                    }
                    return;
                case Keys.Escape:
                    e.Handled = true;
                    txtSearch.Text    = "";
                    _advancedCriteria = null;
                    UpdateFilterIndicator();
                    RefreshCurrentTab();
                    return;
            }
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
