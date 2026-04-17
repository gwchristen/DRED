using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;

namespace DRED
{
    public sealed class AuditLogForm : MaterialForm
    {
        private readonly MaterialTextBox2 _txtFilter;
        private readonly DateTimePicker _dtFrom;
        private readonly DateTimePicker _dtTo;
        private readonly MaterialButton _btnRefresh;
        private readonly DataGridView _grid;

        public AuditLogForm()
        {
            Text = "Audit Log";
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(900, 500);
            Size = new Size(1100, 650);

            MaterialSkinManager.Instance.AddFormToManage(this);

            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 54,
                BackColor = ThemeManager.ToolbarColor,
                Padding = new Padding(8, 8, 8, 6)
            };

            _txtFilter = new MaterialTextBox2
            {
                Hint = "Table name or record ID",
                UseTallSize = false,
                Size = new Size(240, 36),
                Location = new Point(8, 8)
            };
            _txtFilter.TextChanged += (_, _) => LoadAuditLog();

            _dtFrom = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                ShowCheckBox = true,
                Width = 110,
                Location = new Point(260, 14)
            };
            _dtFrom.ValueChanged += (_, _) => LoadAuditLog();
            _dtFrom.Checked = false;

            _dtTo = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                ShowCheckBox = true,
                Width = 110,
                Location = new Point(380, 14)
            };
            _dtTo.ValueChanged += (_, _) => LoadAuditLog();
            _dtTo.Checked = false;

            _btnRefresh = new MaterialButton
            {
                Text = "Refresh",
                Type = MaterialButton.MaterialButtonType.Outlined,
                AutoSize = true,
                Location = new Point(500, 10)
            };
            _btnRefresh.Click += (_, _) => LoadAuditLog();

            topPanel.Controls.Add(_txtFilter);
            topPanel.Controls.Add(_dtFrom);
            topPanel.Controls.Add(_dtTo);
            topPanel.Controls.Add(_btnRefresh);

            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            ThemeManager.ApplyToDataGridView(_grid);

            Controls.Add(_grid);
            Controls.Add(topPanel);
            ThemeManager.StyleNonMaterialControls(this);

            Shown += (_, _) => LoadAuditLog();
        }

        private void LoadAuditLog()
        {
            try
            {
                string filter = _txtFilter.Text.Trim();
                int? recordId = int.TryParse(filter, out int parsedId) ? parsedId : null;
                string? tableName = recordId.HasValue || string.IsNullOrWhiteSpace(filter) ? null : filter;

                DataTable dt = AuditLogger.GetAuditLog(recordId, tableName, 500);
                var rows = dt.AsEnumerable();

                if (_dtFrom.Checked)
                {
                    DateTime from = _dtFrom.Value.Date;
                    rows = rows.Where(r => r.Field<DateTime?>("Timestamp") >= from);
                }
                if (_dtTo.Checked)
                {
                    DateTime to = _dtTo.Value.Date.AddDays(1).AddTicks(-1);
                    rows = rows.Where(r => r.Field<DateTime?>("Timestamp") <= to);
                }

                DataTable filtered = rows.Any() ? rows.CopyToDataTable() : dt.Clone();
                _grid.DataSource = filtered;

                SetHeader("Timestamp", "Timestamp");
                SetHeader("UserName", "User");
                SetHeader("TableName", "Table");
                SetHeader("RecordId", "Record ID");
                SetHeader("Action", "Action");
                SetHeader("FieldName", "Field");
                SetHeader("OldValue", "Old Value");
                SetHeader("NewValue", "New Value");
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to load audit log.", ex);
                MessageBox.Show(
                    $"Failed to load audit log:\n{ex.Message}",
                    "Audit Log Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void SetHeader(string column, string text)
        {
            if (_grid.Columns.Contains(column))
                _grid.Columns[column].HeaderText = text;
        }
    }
}
