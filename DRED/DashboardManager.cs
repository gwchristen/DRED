using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MaterialSkin.Controls;

namespace DRED
{
    /// <summary>
    /// Manages the Dashboard summary tab content and data refresh.
    /// </summary>
    internal sealed class DashboardManager
    {
        private Panel? _dashboardHostPanel;
        private DashboardLabels? _dashboardLabels;
        private readonly string[] _tableNames;
        private readonly Action<int> _updateStatusBar;
        private readonly Color _accentColor;
        private ComboBox? _cboTableFilter;
        private ComboBox? _cboStatusFilter;
        private ComboBox? _cboOpCoFilter;
        private MaterialButton? _btnRefresh;

        public DashboardManager(string[] tableNames, Color accentColor, Action<int> updateStatusBar)
        {
            _tableNames = tableNames;
            _accentColor = accentColor;
            _updateStatusBar = updateStatusBar;
        }

        public void Initialize(TabPage dashboardTab)
        {
            _dashboardHostPanel = dashboardTab.Controls.OfType<Panel>().FirstOrDefault();
            if (_dashboardHostPanel == null)
            {
                _dashboardHostPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = ThemeManager.BackgroundColor,
                    Padding = new Padding(12),
                    AutoScroll = true,
                };
                dashboardTab.Controls.Add(_dashboardHostPanel);
            }

            _dashboardHostPanel.Controls.Clear();
            _dashboardHostPanel.BackColor = ThemeManager.BackgroundColor;

            var filterPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 44,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = ThemeManager.SurfaceColor,
                Padding = new Padding(8, 8, 8, 6),
                Margin = new Padding(0, 0, 0, 8),
            };

            _cboTableFilter = CreateFilterComboBox();
            _cboTableFilter.Items.AddRange(new object[] { "All Tables", "OH_Meters", "IM_Meters", "OH_Transformers", "IM_Transformers" });
            _cboTableFilter.SelectedIndex = 0;

            _cboStatusFilter = CreateFilterComboBox();
            _cboStatusFilter.Items.Add("All Statuses");
            _cboStatusFilter.SelectedIndex = 0;

            _cboOpCoFilter = CreateFilterComboBox();
            _cboOpCoFilter.Items.Add("All OpCo2");
            _cboOpCoFilter.SelectedIndex = 0;

            _btnRefresh = new MaterialButton
            {
                Text = "Refresh",
                Type = MaterialButton.MaterialButtonType.Outlined,
                AutoSize = true,
                Margin = new Padding(8, 2, 0, 0),
            };
            _btnRefresh.Click += (s, e) => Refresh();

            filterPanel.Controls.Add(MakeFilterLabel("Table:"));
            filterPanel.Controls.Add(_cboTableFilter);
            filterPanel.Controls.Add(MakeFilterLabel("Status:"));
            filterPanel.Controls.Add(_cboStatusFilter);
            filterPanel.Controls.Add(MakeFilterLabel("OpCo2:"));
            filterPanel.Controls.Add(_cboOpCoFilter);
            filterPanel.Controls.Add(_btnRefresh);

            var grid = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 2,
                RowCount = 2,
                BackColor = Color.Transparent,
                Padding = new Padding(0),
                Margin = new Padding(0),
            };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var labels = new DashboardLabels();

            var totalCard = CreateDashboardCard("TOTAL RECORDS", _accentColor, out labels.TotalRecords);
            labels.TotalRecords.Font = new Font("Segoe UI Semibold", 18F, FontStyle.Bold);
            labels.PerTable = AddDashboardSecondaryLabel(totalCard);
            grid.Controls.Add(totalCard, 0, 0);

            var recentCard = CreateDashboardCard("RECENT ACTIVITY", _accentColor, out labels.RecentActivity);
            grid.Controls.Add(recentCard, 1, 0);

            var statusCard = CreateDashboardCard("RECORDS BY STATUS", _accentColor, out labels.StatusCounts);
            grid.Controls.Add(statusCard, 0, 1);

            var opcoCard = CreateDashboardCard("RECORDS BY OPCO2", _accentColor, out labels.OpCoCounts);
            grid.Controls.Add(opcoCard, 1, 1);

            _dashboardHostPanel.Controls.Add(grid);
            _dashboardHostPanel.Controls.Add(filterPanel);
            _dashboardLabels = labels;
        }

        public void Refresh()
        {
            if (_dashboardLabels == null || _cboTableFilter == null || _cboStatusFilter == null || _cboOpCoFilter == null) return;

            var tableData = new Dictionary<string, DataTable>();
            string selectedTable = _cboTableFilter.SelectedItem?.ToString() ?? "All Tables";
            IEnumerable<string> selectedTables = _tableNames.Where(table =>
                selectedTable == "All Tables" ||
                string.Equals(table, selectedTable, StringComparison.OrdinalIgnoreCase));
            foreach (string table in selectedTables)
                tableData[table] = DatabaseHelper.GetTableData(table);

            UpdateFilterOptions(tableData.Values, _cboStatusFilter, "All Statuses", "Status");
            UpdateFilterOptions(tableData.Values, _cboOpCoFilter, "All OpCo2", "OpCo2");

            string statusFilter = _cboStatusFilter.SelectedItem?.ToString() ?? "All Statuses";
            string opcoFilter = _cboOpCoFilter.SelectedItem?.ToString() ?? "All OpCo2";
            bool hasStatusFilter = !string.Equals(statusFilter, "All Statuses", StringComparison.OrdinalIgnoreCase);
            bool hasOpcoFilter = !string.Equals(opcoFilter, "All OpCo2", StringComparison.OrdinalIgnoreCase);

            if (hasStatusFilter || hasOpcoFilter)
            {
                foreach (string key in tableData.Keys.ToList())
                    tableData[key] = ApplyFilters(tableData[key], hasStatusFilter ? statusFilter : null, hasOpcoFilter ? opcoFilter : null);
            }

            int total = tableData.Values.Sum(t => t.Rows.Count);
            _dashboardLabels.TotalRecords.Text = total.ToString("N0");
            _dashboardLabels.PerTable.Text = string.Join(Environment.NewLine,
                tableData.Select(kvp => $"{kvp.Key}: {kvp.Value.Rows.Count:N0}"));

            var recent = tableData
                .SelectMany(kvp =>
                    kvp.Value.AsEnumerable()
                    .Select(row => new
                    {
                        Table = kvp.Key,
                        DevCode = row["DevCode"] as string ?? "(blank)",
                        Date = GetRecentDate(row)
                    }))
                .Where(x => x.Date.HasValue)
                .OrderByDescending(x => x.Date)
                .Take(5)
                .Select(x => $"{x.DevCode} • {x.Table} • {x.Date:MM/dd/yyyy HH:mm}")
                .ToList();
            _dashboardLabels.RecentActivity.Text = recent.Count > 0
                ? string.Join(Environment.NewLine, recent)
                : "No recent activity.";

            _dashboardLabels.StatusCounts.Text = BuildGroupedCounts(tableData.Values, "Status");
            _dashboardLabels.OpCoCounts.Text = BuildGroupedCounts(tableData.Values, "OpCo2");
            _updateStatusBar(total);
            Logger.Log("Dashboard refreshed.");
        }

        private static ComboBox CreateFilterComboBox()
        {
            return new ComboBox
            {
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = ThemeManager.InputBackColor,
                ForeColor = ThemeManager.TextColor,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(4, 2, 6, 0),
            };
        }

        private static Label MakeFilterLabel(string text)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                ForeColor = ThemeManager.SecondaryTextColor,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 6, 0, 0),
            };
        }

        private static void UpdateFilterOptions(IEnumerable<DataTable> tables, ComboBox comboBox, string allText, string columnName)
        {
            string previous = comboBox.SelectedItem?.ToString() ?? allText;
            var distinctValues = tables
                .SelectMany(t => t.AsEnumerable())
                .Where(r => r.Table.Columns.Contains(columnName) && r[columnName] != DBNull.Value)
                .Select(r => (r[columnName] as string ?? string.Empty).Trim())
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(v => v, StringComparer.OrdinalIgnoreCase)
                .ToList();

            comboBox.BeginUpdate();
            comboBox.Items.Clear();
            comboBox.Items.Add(allText);
            foreach (string value in distinctValues)
                comboBox.Items.Add(value);
            comboBox.EndUpdate();

            if (comboBox.Items.Cast<object>().Any(item =>
                    string.Equals(item?.ToString(), previous, StringComparison.OrdinalIgnoreCase)))
            {
                comboBox.SelectedItem = comboBox.Items.Cast<object>()
                    .First(item => string.Equals(item?.ToString(), previous, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                comboBox.SelectedIndex = 0;
            }
        }

        private static DataTable ApplyFilters(DataTable source, string? statusFilter, string? opcoFilter)
        {
            if (string.IsNullOrWhiteSpace(statusFilter) && string.IsNullOrWhiteSpace(opcoFilter))
                return source;

            var rows = source.AsEnumerable().Where(row =>
            {
                bool statusMatches = string.IsNullOrWhiteSpace(statusFilter) ||
                    string.Equals((row["Status"] as string ?? string.Empty).Trim(), statusFilter, StringComparison.OrdinalIgnoreCase);
                bool opcoMatches = string.IsNullOrWhiteSpace(opcoFilter) ||
                    string.Equals((row["OpCo2"] as string ?? string.Empty).Trim(), opcoFilter, StringComparison.OrdinalIgnoreCase);
                return statusMatches && opcoMatches;
            });

            return rows.Any() ? rows.CopyToDataTable() : source.Clone();
        }

        private static TableLayoutPanel CreateDashboardCard(string title, Color accent, out Label valueLabel)
        {
            var card = new TableLayoutPanel
            {
                ColumnCount = 1,
                RowCount = 2,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Top,
                Margin = new Padding(6),
                Padding = new Padding(12, 10, 12, 12),
                BackColor = ThemeManager.SurfaceColor,
            };
            card.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            card.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var header = new Label
            {
                Text = title,
                ForeColor = accent,
                Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold),
                Dock = DockStyle.Top,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 6),
            };

            valueLabel = new Label
            {
                Text = "—",
                ForeColor = ThemeManager.TextColor,
                Font = new Font("Segoe UI", 11F),
                Dock = DockStyle.Top,
                AutoSize = true,
                MaximumSize = new Size(480, 0),
            };

            card.Controls.Add(header, 0, 0);
            card.Controls.Add(valueLabel, 0, 1);
            return card;
        }

        private static Label AddDashboardSecondaryLabel(TableLayoutPanel card)
        {
            card.RowCount += 1;
            card.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            var lbl = new Label
            {
                Text = "—",
                ForeColor = ThemeManager.SecondaryTextColor,
                Font = new Font("Segoe UI", 9F),
                Dock = DockStyle.Top,
                AutoSize = true,
                MaximumSize = new Size(480, 0),
                Margin = new Padding(0, 8, 0, 0),
            };
            card.Controls.Add(lbl, 0, card.RowCount - 1);
            return lbl;
        }

        private static DateTime? GetRecentDate(DataRow row)
        {
            if (row.Table.Columns.Contains("ModifiedDate") && row["ModifiedDate"] is not DBNull)
                return Convert.ToDateTime(row["ModifiedDate"]);
            if (row.Table.Columns.Contains("CreatedDate") && row["CreatedDate"] is not DBNull)
                return Convert.ToDateTime(row["CreatedDate"]);
            return null;
        }

        private static string BuildGroupedCounts(IEnumerable<DataTable> tables, string columnName)
        {
            var groups = tables
                .SelectMany(t => t.AsEnumerable())
                .Select(r => r.Table.Columns.Contains(columnName) && r[columnName] is not DBNull
                    ? (r[columnName] as string ?? string.Empty).Trim()
                    : string.Empty)
                .Select(v => string.IsNullOrWhiteSpace(v) ? "(blank)" : v)
                .GroupBy(v => v, StringComparer.OrdinalIgnoreCase)
                .OrderByDescending(g => g.Count())
                .ThenBy(g => g.Key, StringComparer.OrdinalIgnoreCase)
                .Select(g => $"{g.Key}: {g.Count():N0}")
                .ToList();

            return groups.Count > 0 ? string.Join(Environment.NewLine, groups) : "No records.";
        }

        private sealed class DashboardLabels
        {
            public Label TotalRecords = null!;
            public Label PerTable = null!;
            public Label RecentActivity = null!;
            public Label StatusCounts = null!;
            public Label OpCoCounts = null!;
        }
    }
}
