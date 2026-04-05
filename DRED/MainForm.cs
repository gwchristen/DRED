using System;
using System.Data;
using System.IO;
using System.Windows.Forms;

namespace DRED
{
    public partial class MainForm : Form
    {
        private static readonly string[] TabTableNames =
            { "OH_Meters", "IM_Meters", "OH_Transformers", "IM_Transformers" };

        private DataGridView CurrentGrid =>
            (DataGridView)tabControl.SelectedTab!.Controls[0];

        private string CurrentTable =>
            TabTableNames[tabControl.SelectedIndex];

        public MainForm()
        {
            InitializeComponent();
            RefreshCurrentTab();
        }

        private void RefreshCurrentTab()
        {
            try
            {
                LoadTable(tabControl.SelectedIndex, txtSearch.Text.Trim());
            }
            catch (Exception ex)
            {
                ShowDbError(ex);
            }
        }

        private void LoadTable(int tabIndex, string filter)
        {
            string table = TabTableNames[tabIndex];
            DataTable dt = DatabaseHelper.GetTableData(table, filter);
            var grid = (DataGridView)tabControl.TabPages[tabIndex].Controls[0];
            grid.DataSource = dt;
            grid.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
        }

        private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshCurrentTab();
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            RefreshCurrentTab();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshCurrentTab();
        }

        private void btnAdd_Click(object sender, EventArgs e)
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

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (CurrentGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a row to edit.", "Edit Record",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DataRow row = ((DataRowView)CurrentGrid.SelectedRows[0].DataBoundItem!).Row;
            int id = Convert.ToInt32(row["Id"]);
            var existing = RowToRecordData(row);

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

        private void btnDelete_Click(object sender, EventArgs e)
        {
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
                RefreshCurrentTab();
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Title = "Select Excel File to Import",
                Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                DefaultExt = "xlsx",
            };

            // Default to repository root if the file is there
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
                ExcelImporter.Import(dlg.FileName, msg =>
                {
                    progress.AppendLine(msg);
                });
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
            if (dlg.ShowDialog(this) != DialogResult.OK)
                return;

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

        private static RecordData RowToRecordData(DataRow row)
        {
            return new RecordData
            {
                Key      = row["Key"] as string,
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
            };
        }

        // OleDb error codes for record/file locking scenarios
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
