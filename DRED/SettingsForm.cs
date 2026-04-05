using System;
using System.IO;
using System.Windows.Forms;

namespace DRED
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
            txtDatabasePath.Text = AppSettings.DatabasePath;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using var dlg = new SaveFileDialog
            {
                Title = "Select or Create Database File",
                Filter = "Access Database (*.accdb)|*.accdb",
                DefaultExt = "accdb",
                FileName = "DRED.accdb",
                OverwritePrompt = false,
            };

            if (!string.IsNullOrWhiteSpace(txtDatabasePath.Text))
            {
                try
                {
                    dlg.InitialDirectory = Path.GetDirectoryName(txtDatabasePath.Text) ?? "";
                    dlg.FileName = Path.GetFileName(txtDatabasePath.Text);
                }
                catch { }
            }

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                txtDatabasePath.Text = dlg.FileName;
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            string path = txtDatabasePath.Text.Trim();
            if (string.IsNullOrWhiteSpace(path))
            {
                MessageBox.Show("Please specify a database file path.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            AppSettings.DatabasePath = path;
            AppSettings.Save();

            try
            {
                DatabaseHelper.EnsureDatabaseExists();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Could not initialize database:\n{ex.Message}",
                    "Database Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
