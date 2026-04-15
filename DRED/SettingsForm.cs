using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace DRED
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
            txtDatabasePath.Text = AppSettings.DatabasePath;
            nudAutoRefresh.Value = AppSettings.AutoRefreshInterval;
            nudBackupInterval.Value = Math.Clamp(AppSettings.BackupIntervalHours, 0, 168);
            nudMaxBackups.Value = Math.Clamp(AppSettings.MaxBackupCount, 1, 100);
            txtLockPin.Text = AppSettings.LockPin;
            LoadAuthorizedUsers();
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
                catch (Exception ex)
                {
                    Logger.LogError("Failed to initialize settings browse dialog path.", ex);
                }
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

            string pin = txtLockPin.Text.Trim();
            if (string.IsNullOrWhiteSpace(pin))
            {
                MessageBox.Show("Lock PIN cannot be empty.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (pin.Length < 4)
            {
                MessageBox.Show("Lock PIN must be at least 4 characters.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            AppSettings.DatabasePath = path;
            AppSettings.AutoRefreshInterval = (int)nudAutoRefresh.Value;
            AppSettings.BackupIntervalHours = (int)nudBackupInterval.Value;
            AppSettings.MaxBackupCount = (int)nudMaxBackups.Value;
            AppSettings.LockPin = pin;
            AppSettings.AuthorizedUsers = GetAuthorizedUsersFromList();
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

        private void LoadAuthorizedUsers()
        {
            lstAuthorizedUsers.Items.Clear();
            foreach (string user in AppSettings.AuthorizedUsers
                         .Where(u => !string.IsNullOrWhiteSpace(u))
                         .Select(u => u.Trim())
                         .Distinct(StringComparer.OrdinalIgnoreCase))
            {
                lstAuthorizedUsers.Items.Add(user);
            }
        }

        private List<string> GetAuthorizedUsersFromList()
        {
            return lstAuthorizedUsers.Items
                .Cast<object>()
                .Select(item => item.ToString() ?? string.Empty)
                .Where(user => !string.IsNullOrWhiteSpace(user))
                .Select(user => user.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private void btnAddUser_Click(object sender, EventArgs e)
        {
            string user = txtAuthorizedUser.Text.Trim();
            if (string.IsNullOrWhiteSpace(user))
                return;

            bool exists = lstAuthorizedUsers.Items
                .Cast<object>()
                .Any(item => string.Equals(item.ToString(), user, StringComparison.OrdinalIgnoreCase));
            if (!exists)
                lstAuthorizedUsers.Items.Add(user);

            txtAuthorizedUser.Text = string.Empty;
            txtAuthorizedUser.Focus();
        }

        private void btnRemoveUser_Click(object sender, EventArgs e)
        {
            if (lstAuthorizedUsers.SelectedIndex >= 0)
                lstAuthorizedUsers.Items.RemoveAt(lstAuthorizedUsers.SelectedIndex);
        }
    }
}
