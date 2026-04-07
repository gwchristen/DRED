using System;
using System.Data;
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

        private const string CurrencyFormat = "$#,##0.00";

        private DataGridView? CurrentGrid
        {
            get
            {
                var tab = tabControl?.SelectedTab;
                if (tab == null || tab.Controls.Count == 0) return null;
                return tab.Controls[0] as DataGridView;
            }
        }

        private string CurrentTable =>
            TabTableNames[tabControl.SelectedIndex];

        private AdvancedSearchCriteria? _advancedCriteria;
        private bool _dialogOpen = false;
        private System.Windows.Forms.Timer _refreshTimer = null!;
        private bool _initialized = false;

        public MainForm()
        {
            InitializeComponent();

            MaterialSkinManager.Instance.AddFormToManage(this);

            // Populate filter column combo — wire event AFTER setting SelectedIndex
            cboFilterColumn.Items.AddRange(new object[] {
                "All Columns", "OpCo2", "Status", "MFR", "DevCode", "BegSer", "EndSer",
                "PONumber", "Vintage", "CID", "MENumber", "PurCode", "Est", "Comments"
            });
            cboFilterColumn.SelectedIndex = 0;
            cboFilterColumn.SelectedIndexChanged += cboFilterColumn_SelectedIndexChanged;

            _refreshTimer = new System.Windows.Forms.Timer();
            _refreshTimer.Tick += (s, e) => { if (!_dialogOpen) RefreshCurrentTab(); };
            UpdateRefreshTimer();

            _initialized = true;
            RefreshCurrentTab();
        }

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
            if (!_initialized || CurrentGrid == null) return;
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
            var tab = tabControl.TabPages[tabIndex];
            if (tab.Controls.Count == 0) return;
            var grid = tab.Controls[0] as DataGridView;
            if (grid == null) return;
            string table = TabTableNames[tabIndex];
            DataTable dt = DatabaseHelper.GetTableData(table, filter, filterColumn, advancedCriteria);
            grid.DataSource = dt;
            ApplyGridFormatting(grid);
            grid.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
            UpdateStatusBar(dt.Rows.Count);
        }

        private static void ApplyGridFormatting(DataGridView grid)
        {
            if (grid.Columns.Contains("UnitCost"))
                grid.Columns["UnitCost"].DefaultCellStyle.Format = CurrencyFormat;
            if (grid.Columns.Contains("PODate"))
                grid.Columns["PODate"].DefaultCellStyle.Format = "MM/dd/yyyy";
            if (grid.Columns.Contains("RecvDate"))
                grid.Columns["RecvDate"].DefaultCellStyle.Format = "MM/dd/yyyy";
            if (grid.Columns.Contains("CreatedDate"))
                grid.Columns["CreatedDate"].DefaultCellStyle.Format = "MM/dd/yyyy";
            if (grid.Columns.Contains("ModifiedDate"))
                grid.Columns["ModifiedDate"].DefaultCellStyle.Format = "MM/dd/yyyy";
        }

        private void UpdateStatusBar(int recordCount)
        {
            lblStatusRecords.Text = $"Records: {recordCount}";
            lblStatusConnection.Text = $"Connected: {AppSettings.DatabasePath}";
            lblStatusUser.Text = $"User: {Environment.UserName}";
        }

        private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
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
            txtSearch.Text = "";
            _advancedCriteria = null;
            RefreshCurrentTab();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshCurrentTab();
        }

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
            if (CurrentGrid == null) return;
            if (CurrentGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a row to edit.", "Edit Record",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DataRow row = ((DataRowView)CurrentGrid.SelectedRows[0].DataBoundItem!).Row;
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

        private void grid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            btnEdit_Click(sender, e);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (CurrentGrid == null) return;
            if (CurrentGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a row to delete.", "Delete Record",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DataRow row = ((DataRowView)CurrentGrid.SelectedRows[0].DataBoundItem!).Row;
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
                Title = "Select Excel File to Import",
                Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                DefaultExt = "xlsx",
            };

            string defaultPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Created Histories.xlsx");
            if (File.Exists(defaultPath))
                dlg.FileName = defaultPath;

            if (dlg.ShowDialog(this) != DialogResult.OK)
                return;

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
                hasError ? "Import – Errors Occurred" : "Import Complete",
                MessageBoxButtons.OK,
                hasError ? MessageBoxIcon.Warning : MessageBoxIcon.Information);

            RefreshCurrentTab();
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            using var dlg = new SaveFileDialog
            {
                Title = "Export to Excel",
                Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                DefaultExt = "xlsx",
                FileName = $"{CurrentTable}_{DateTime.Now:yyyyMMdd}.xlsx",
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
                Title = "Export All Tabs to Excel",
                Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                DefaultExt = "xlsx",
                FileName = $"DRED_Export_{DateTime.Now:yyyyMMdd}.xlsx",
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
                    txtSearch.Text = "";
                    _advancedCriteria = null;
                    RefreshCurrentTab();
                    return;
            }
        }

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

