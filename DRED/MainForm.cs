using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
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

        // Populated by Designer's SetupTabWithSplit; arrays allocated here as field initializers
        private System.Windows.Forms.SplitContainer[] _splitContainers = new System.Windows.Forms.SplitContainer[4];
        private System.Windows.Forms.ListBox[] _listBoxes   = new System.Windows.Forms.ListBox[4];
        private System.Windows.Forms.Panel[]   _detailPanels = new System.Windows.Forms.Panel[4];
        private System.Data.DataTable?[]       _dataTables   = new System.Data.DataTable?[4];
        private DetailLabels[]                 _detailLabels = new DetailLabels[4];
        private int[]                          _hoveredListItem = { -1, -1, -1, -1 };

        private const int  ListItemHeight     = 72;
        private const string CardTag          = "card";

        private string CurrentTable =>
            TabTableNames[tabControl.SelectedIndex];

        private AdvancedSearchCriteria? _advancedCriteria;
        private bool _dialogOpen  = false;
        private System.Windows.Forms.Timer _refreshTimer = null!;
        private bool _initialized = false;

        // ── Nested types ─────────────────────────────────────────────────

        private record ListItem(string DevCode, string Qty, string PONumber, string RecvDate, int RowIndex);

        private sealed class DetailLabels
        {
            public System.Windows.Forms.Label ValOpCo2   = null!, ValStatus   = null!,
                                              ValMFR      = null!, ValDevCode  = null!;
            public System.Windows.Forms.Label ValBegSer  = null!, ValEndSer   = null!, ValQty = null!;
            public System.Windows.Forms.Label ValPODate  = null!, ValPONumber = null!,
                                              ValVintage  = null!, ValRecvDate = null!, ValUnitCost = null!;
            public System.Windows.Forms.Label ValCID     = null!, ValMENumber = null!,
                                              ValPurCode  = null!, ValEst      = null!;
            public System.Windows.Forms.Label ValComments = null!;
            public System.Windows.Forms.Label LblAudit   = null!;
            public System.Windows.Forms.Label EmptyStateLabel = null!;
            public System.Windows.Forms.Panel ContentPanel    = null!;
            public MaterialSkin.Controls.MaterialButton BtnDetailEdit = null!, BtnDetailDelete = null!;
        }

        // ── Constructor ──────────────────────────────────────────────────

        public MainForm()
        {
            InitializeComponent();
            MaterialSkinManager.Instance.AddFormToManage(this);

            cboFilterColumn.Items.AddRange(new object[] {
                "All Columns", "OpCo2", "Status", "MFR", "DevCode", "BegSer", "EndSer",
                "PONumber", "Vintage", "CID", "MENumber", "PurCode", "Est", "Comments"
            });
            cboFilterColumn.SelectedIndex = 0;
            cboFilterColumn.SelectedIndexChanged += cboFilterColumn_SelectedIndexChanged;

            InitializeDetailPanels();

            _refreshTimer = new System.Windows.Forms.Timer();
            _refreshTimer.Tick += (s, e) => { if (!_dialogOpen) RefreshCurrentTab(); };
            UpdateRefreshTimer();

            this.Shown += (s, e) =>
            {
                this.BeginInvoke(new Action(() =>
                {
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
                }));
            };

            _initialized = true;
            RefreshCurrentTab();
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
            string table = TabTableNames[tabIndex];
            DataTable dt = DatabaseHelper.GetTableData(table, filter, filterColumn, advancedCriteria);
            _dataTables[tabIndex] = dt;
            PopulateListBox(_listBoxes[tabIndex], dt);
            UpdateStatusBar(dt.Rows.Count);

            if (_listBoxes[tabIndex].Items.Count > 0)
                _listBoxes[tabIndex].SelectedIndex = 0;
            else
                ShowEmptyDetailState(tabIndex);
        }

        // ── Status bar ───────────────────────────────────────────────────

        private void UpdateStatusBar(int recordCount)
        {
            lblStatusRecords.Text    = $"Records: {recordCount}";
            lblStatusConnection.Text = $"Connected: {AppSettings.DatabasePath}";
            lblStatusUser.Text       = $"User: {Environment.UserName}";
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
            var r2 = new System.Drawing.RectangleF(x, y + 17, maxW, 16);
            var r3 = new System.Drawing.RectangleF(x, y + 32, maxW, 16);
            var r4 = new System.Drawing.RectangleF(x, y + 48, maxW, 14);

            g.DrawString(item.DevCode,               font1, brush1, r1, fmt);
            g.DrawString($"Qty: {item.Qty}",         font2, brush2, r2, fmt);
            g.DrawString(item.PONumber,              font2, brush2, r3, fmt);
            g.DrawString(item.RecvDate,              font3, brush3, r4, fmt);

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

            // Content panel — fills the detail panel, no scrolling needed
            var contentPanel = new System.Windows.Forms.Panel
            {
                Dock      = DockStyle.Fill,
                Padding   = new Padding(10, 8, 10, 8),
                BackColor = Color.FromArgb(0x2D, 0x2D, 0x30),
                Visible   = false,
            };
            dl.ContentPanel = contentPanel;
            scrollPanel.Controls.Add(contentPanel);

            // Main 4-column TableLayoutPanel: [label 80px] [value 50%] [label 80px] [value 50%]
            var grid = new System.Windows.Forms.TableLayoutPanel
            {
                Dock            = DockStyle.Fill,
                ColumnCount     = 4,
                BackColor       = Color.Transparent,
                CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.None,
                Padding         = Padding.Empty,
                Margin          = Padding.Empty,
            };
            grid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80));
            grid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50));
            grid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80));
            grid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50));
            contentPanel.Controls.Add(grid);

            int row = 0;

            // ── Helper closures ──────────────────────────────────────────
            void AddSectionHeader(string title)
            {
                grid.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26));
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
                grid.Controls.Add(hdr, 0, row);
                grid.SetColumnSpan(hdr, 4);
                row++;
            }

            System.Windows.Forms.Label AddFieldPair(string labelText, int col)
            {
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
                grid.Controls.Add(nm, col, row);
                grid.Controls.Add(vl, col + 1, row);
                return vl;
            }

            void AddSeparator()
            {
                grid.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 1));
                var sep = new System.Windows.Forms.Panel
                {
                    Dock      = DockStyle.Fill,
                    BackColor = Color.FromArgb(0x3E, 0x3E, 0x42),
                    Margin    = Padding.Empty,
                };
                grid.Controls.Add(sep, 0, row);
                grid.SetColumnSpan(sep, 4);
                row++;
            }

            // ── DEVICE INFORMATION ───────────────────────────────────────
            AddSectionHeader("DEVICE INFORMATION");
            grid.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22));
            dl.ValOpCo2  = AddFieldPair("OpCo2:",  0);
            dl.ValStatus = AddFieldPair("Status:", 2);
            row++;
            grid.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22));
            dl.ValMFR     = AddFieldPair("MFR:",      0);
            dl.ValDevCode = AddFieldPair("Dev Code:", 2);
            row++;

            AddSeparator();

            // ── SERIAL RANGE & QUANTITY ──────────────────────────────────
            AddSectionHeader("SERIAL RANGE & QUANTITY");
            grid.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22));
            dl.ValBegSer = AddFieldPair("Beg Ser:", 0);
            dl.ValEndSer = AddFieldPair("End Ser:", 2);
            row++;
            grid.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22));
            dl.ValQty = AddFieldPair("Qty:", 0);
            row++;

            AddSeparator();

            // ── PURCHASE INFORMATION ─────────────────────────────────────
            AddSectionHeader("PURCHASE INFORMATION");
            grid.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22));
            dl.ValPODate   = AddFieldPair("PO Date:",  0);
            dl.ValPONumber = AddFieldPair("PO Number:", 2);
            row++;
            grid.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22));
            dl.ValVintage  = AddFieldPair("Vintage:",  0);
            dl.ValRecvDate = AddFieldPair("Recv Date:", 2);
            row++;
            grid.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22));
            dl.ValUnitCost = AddFieldPair("Unit Cost:", 0);
            row++;

            AddSeparator();

            // ── IDENTIFIERS ──────────────────────────────────────────────
            AddSectionHeader("IDENTIFIERS");
            grid.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22));
            dl.ValCID      = AddFieldPair("CID:",      0);
            dl.ValMENumber = AddFieldPair("M.E. #:",   2);
            row++;
            grid.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22));
            dl.ValPurCode = AddFieldPair("Pur Code:", 0);
            dl.ValEst     = AddFieldPair("Est.:",     2);
            row++;

            AddSeparator();

            // ── COMMENTS & NOTES ─────────────────────────────────────────
            AddSectionHeader("COMMENTS & NOTES");
            grid.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 54));
            var commLabel = new System.Windows.Forms.Label
            {
                Text      = "\u2014",
                ForeColor = Color.FromArgb(0xF1, 0xF1, 0xF1),
                Font      = new Font("Segoe UI", 9F),
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.TopLeft,
                BackColor = Color.Transparent,
            };
            grid.Controls.Add(commLabel, 0, row);
            grid.SetColumnSpan(commLabel, 4);
            dl.ValComments = commLabel;
            row++;

            AddSeparator();

            // ── Audit label ──────────────────────────────────────────────
            grid.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22));
            var auditLabel = new System.Windows.Forms.Label
            {
                Text      = "",
                ForeColor = Color.FromArgb(0x88, 0x88, 0x88),
                Font      = new Font("Segoe UI", 8F, FontStyle.Italic),
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent,
            };
            dl.LblAudit = auditLabel;
            grid.Controls.Add(auditLabel, 0, row);
            grid.SetColumnSpan(auditLabel, 4);
            row++;

            // ── Action buttons ───────────────────────────────────────────
            grid.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40));
            var btnDetailEdit   = CreateDetailButton("Edit",   accentColor,                      (s, ev) => btnEdit_Click(s!, ev));
            var btnDetailDelete = CreateDetailButton("Delete", Color.FromArgb(0xEF, 0x53, 0x50), (s, ev) => btnDelete_Click(s!, ev));
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
            grid.Controls.Add(btnPanel, 0, row);
            grid.SetColumnSpan(btnPanel, 4);
            dl.BtnDetailEdit   = btnDetailEdit;
            dl.BtnDetailDelete = btnDetailDelete;

            grid.RowCount = row + 1;

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

            dl.ValPODate.Text   = D(row["PODate"]);
            dl.ValPONumber.Text = S(row["PONumber"]);
            dl.ValVintage.Text  = S(row["Vintage"]);
            dl.ValRecvDate.Text = D(row["RecvDate"]);
            dl.ValUnitCost.Text = C(row["UnitCost"]);

            dl.ValCID.Text      = S(row["CID"]);
            dl.ValMENumber.Text = S(row["MENumber"]);
            dl.ValPurCode.Text  = S(row["PurCode"]);
            dl.ValEst.Text      = S(row["Est"]);

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
            RefreshCurrentTab();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshCurrentTab();
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
                        RefreshCurrentTab();
                    }
                    catch (Exception ex) { ShowDbError(ex); }
                }
            }
            finally
            {
                _dialogOpen = false;
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
                    try
                    {
                        DatabaseHelper.UpdateRecord(CurrentTable, id, form.Result);
                        RefreshCurrentTab();
                    }
                    catch (Exception ex) { ShowDbError(ex); }
                }
            }
            finally
            {
                _dialogOpen = false;
                DatabaseHelper.UnlockRecord(CurrentTable, id);
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
                try
                {
                    DatabaseHelper.DeleteRecord(CurrentTable, id);
                    RefreshCurrentTab();
                }
                catch (Exception ex) { ShowDbError(ex); }
            }
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            using var form = new SettingsForm();
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                UpdateRefreshTimer();
                RefreshCurrentTab();
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
                    RefreshCurrentTab();
                }
            }
            finally
            {
                _dialogOpen = false;
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
            }
            finally
            {
                Cursor = Cursors.Default;
            }

            MessageBox.Show(progress.ToString(),
                hasError ? "Import \u2013 Errors Occurred" : "Import Complete",
                MessageBoxButtons.OK,
                hasError ? MessageBoxIcon.Warning : MessageBoxIcon.Information);

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

            try
            {
                Cursor = Cursors.WaitCursor;
                DatabaseHelper.ExportToExcel(CurrentTable, dlg.FileName);
                Cursor = Cursors.Default;
                MessageBox.Show($"Data exported to:\n{dlg.FileName}", "Export Complete",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
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

            try
            {
                Cursor = Cursors.WaitCursor;
                DatabaseHelper.ExportAllToExcel(dlg.FileName);
                Cursor = Cursors.Default;
                MessageBox.Show($"All data exported to:\n{dlg.FileName}", "Export Complete",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                        btnAdd_Click(this, EventArgs.Empty);
                        return;
                    case Keys.E:
                        e.Handled = true;
                        btnEdit_Click(this, EventArgs.Empty);
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
                        btnImport_Click(this, EventArgs.Empty);
                        return;
                    case Keys.Oemcomma:
                        e.Handled = true;
                        btnSettings_Click(this, EventArgs.Empty);
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
                        btnDelete_Click(this, EventArgs.Empty);
                    }
                    return;
                case Keys.Escape:
                    e.Handled = true;
                    txtSearch.Text    = "";
                    _advancedCriteria = null;
                    RefreshCurrentTab();
                    return;
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
                Est      = row["Est"] as string,
                Comments = row["Comments"] as string,
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