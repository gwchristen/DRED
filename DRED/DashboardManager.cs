using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

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
            _dashboardLabels = labels;
        }

        public void Refresh()
        {
            if (_dashboardLabels == null) return;

            var tableData = new Dictionary<string, DataTable>();
            foreach (string table in _tableNames)
                tableData[table] = DatabaseHelper.GetTableData(table);

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
