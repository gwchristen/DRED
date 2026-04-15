namespace DRED
{
    partial class SettingsForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblDatabasePath  = new System.Windows.Forms.Label();
            this.txtDatabasePath  = new MaterialSkin.Controls.MaterialTextBox2();
            this.btnBrowse        = new MaterialSkin.Controls.MaterialButton();
            this.lblAutoRefresh   = new System.Windows.Forms.Label();
            this.nudAutoRefresh   = new System.Windows.Forms.NumericUpDown();
            this.lblLockPin       = new System.Windows.Forms.Label();
            this.txtLockPin       = new MaterialSkin.Controls.MaterialTextBox2();
            this.lblAuthorizedUsers = new System.Windows.Forms.Label();
            this.lstAuthorizedUsers = new System.Windows.Forms.ListBox();
            this.txtAuthorizedUser  = new MaterialSkin.Controls.MaterialTextBox2();
            this.btnAddUser         = new MaterialSkin.Controls.MaterialButton();
            this.btnRemoveUser      = new MaterialSkin.Controls.MaterialButton();
            this.btnOK            = new MaterialSkin.Controls.MaterialButton();
            this.btnCancel        = new MaterialSkin.Controls.MaterialButton();
            this.lblInfo          = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.nudAutoRefresh)).BeginInit();
            this.SuspendLayout();

            // lblInfo
            this.lblInfo.AutoSize = false;
            this.lblInfo.Location = new System.Drawing.Point(12, 80);
            this.lblInfo.Size = new System.Drawing.Size(560, 40);
            this.lblInfo.ForeColor = ThemeManager.SecondaryTextColor;
            this.lblInfo.Text = "Specify the path to the DRED Access database file (.accdb). For multi-user " +
                                "access, point this to a file on a shared network drive.";

            // lblDatabasePath
            this.lblDatabasePath.AutoSize = true;
            this.lblDatabasePath.Location = new System.Drawing.Point(12, 130);
            this.lblDatabasePath.ForeColor = ThemeManager.SecondaryTextColor;
            this.lblDatabasePath.Text = "Database Path:";

            // txtDatabasePath
            this.txtDatabasePath.UseTallSize = false;
            this.txtDatabasePath.Location = new System.Drawing.Point(12, 148);
            this.txtDatabasePath.Size = new System.Drawing.Size(460, 48);
            this.txtDatabasePath.TabIndex = 0;

            // btnBrowse
            this.btnBrowse.Location = new System.Drawing.Point(480, 148);
            this.btnBrowse.Text = "Browse...";
            this.btnBrowse.TabIndex = 1;
            this.btnBrowse.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined;
            this.btnBrowse.AutoSize = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);

            // lblAutoRefresh
            this.lblAutoRefresh.AutoSize = true;
            this.lblAutoRefresh.Location = new System.Drawing.Point(12, 210);
            this.lblAutoRefresh.ForeColor = ThemeManager.SecondaryTextColor;
            this.lblAutoRefresh.Text = "Auto-Refresh Interval (seconds, 0 = disabled):";

            // nudAutoRefresh
            this.nudAutoRefresh.Location = new System.Drawing.Point(12, 228);
            this.nudAutoRefresh.Size = new System.Drawing.Size(80, 23);
            this.nudAutoRefresh.Minimum = 0;
            this.nudAutoRefresh.Maximum = 600;
            this.nudAutoRefresh.Value = 60;
            this.nudAutoRefresh.TabIndex = 2;
            this.nudAutoRefresh.BackColor = ThemeManager.InputBackColor;
            this.nudAutoRefresh.ForeColor = System.Drawing.Color.White;

            // lblLockPin
            this.lblLockPin.AutoSize = true;
            this.lblLockPin.Location = new System.Drawing.Point(12, 266);
            this.lblLockPin.ForeColor = ThemeManager.SecondaryTextColor;
            this.lblLockPin.Text = "Lock PIN:";

            // txtLockPin
            this.txtLockPin.UseTallSize = false;
            this.txtLockPin.Location = new System.Drawing.Point(12, 284);
            this.txtLockPin.Size = new System.Drawing.Size(180, 48);
            this.txtLockPin.TabIndex = 3;
            this.txtLockPin.UseSystemPasswordChar = true;

            // lblAuthorizedUsers
            this.lblAuthorizedUsers.AutoSize = true;
            this.lblAuthorizedUsers.Location = new System.Drawing.Point(12, 338);
            this.lblAuthorizedUsers.ForeColor = ThemeManager.SecondaryTextColor;
            this.lblAuthorizedUsers.Text = "Authorized Users (Windows usernames):";

            // lstAuthorizedUsers
            this.lstAuthorizedUsers.Location = new System.Drawing.Point(12, 356);
            this.lstAuthorizedUsers.Size = new System.Drawing.Size(460, 94);
            this.lstAuthorizedUsers.TabIndex = 4;
            this.lstAuthorizedUsers.BackColor = ThemeManager.InputBackColor;
            this.lstAuthorizedUsers.ForeColor = System.Drawing.Color.White;

            // txtAuthorizedUser
            this.txtAuthorizedUser.UseTallSize = false;
            this.txtAuthorizedUser.Location = new System.Drawing.Point(12, 456);
            this.txtAuthorizedUser.Size = new System.Drawing.Size(300, 48);
            this.txtAuthorizedUser.TabIndex = 5;
            this.txtAuthorizedUser.Hint = "Username";

            // btnAddUser
            this.btnAddUser.Location = new System.Drawing.Point(320, 463);
            this.btnAddUser.Text = "Add";
            this.btnAddUser.TabIndex = 6;
            this.btnAddUser.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined;
            this.btnAddUser.AutoSize = true;
            this.btnAddUser.Click += new System.EventHandler(this.btnAddUser_Click);

            // btnRemoveUser
            this.btnRemoveUser.Location = new System.Drawing.Point(390, 463);
            this.btnRemoveUser.Text = "Remove";
            this.btnRemoveUser.TabIndex = 7;
            this.btnRemoveUser.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined;
            this.btnRemoveUser.AutoSize = true;
            this.btnRemoveUser.Click += new System.EventHandler(this.btnRemoveUser_Click);

            // btnOK
            this.btnOK.Location = new System.Drawing.Point(400, 512);
            this.btnOK.Text = "OK";
            this.btnOK.TabIndex = 8;
            this.btnOK.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnOK.HighEmphasis = true;
            this.btnOK.AutoSize = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);

            // btnCancel
            this.btnCancel.Location = new System.Drawing.Point(480, 512);
            this.btnCancel.Text = "Cancel";
            this.btnCancel.TabIndex = 9;
            this.btnCancel.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined;
            this.btnCancel.AutoSize = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);

            // SettingsForm
            this.AcceptButton = this.btnOK;
            this.CancelButton = this.btnCancel;
            this.BackColor = ThemeManager.FormBackColor;
            this.ForeColor = ThemeManager.TextColor;
            this.ClientSize = new System.Drawing.Size(584, 560);
            this.Controls.Add(this.lblInfo);
            this.Controls.Add(this.lblDatabasePath);
            this.Controls.Add(this.txtDatabasePath);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.lblAutoRefresh);
            this.Controls.Add(this.nudAutoRefresh);
            this.Controls.Add(this.lblLockPin);
            this.Controls.Add(this.txtLockPin);
            this.Controls.Add(this.lblAuthorizedUsers);
            this.Controls.Add(this.lstAuthorizedUsers);
            this.Controls.Add(this.txtAuthorizedUser);
            this.Controls.Add(this.btnAddUser);
            this.Controls.Add(this.btnRemoveUser);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "DRED – Settings";
            ((System.ComponentModel.ISupportInitialize)(this.nudAutoRefresh)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label lblInfo = null!;
        private System.Windows.Forms.Label lblDatabasePath = null!;
        private MaterialSkin.Controls.MaterialTextBox2 txtDatabasePath = null!;
        private MaterialSkin.Controls.MaterialButton btnBrowse = null!;
        private System.Windows.Forms.Label lblAutoRefresh = null!;
        private System.Windows.Forms.NumericUpDown nudAutoRefresh = null!;
        private System.Windows.Forms.Label lblLockPin = null!;
        private MaterialSkin.Controls.MaterialTextBox2 txtLockPin = null!;
        private System.Windows.Forms.Label lblAuthorizedUsers = null!;
        private System.Windows.Forms.ListBox lstAuthorizedUsers = null!;
        private MaterialSkin.Controls.MaterialTextBox2 txtAuthorizedUser = null!;
        private MaterialSkin.Controls.MaterialButton btnAddUser = null!;
        private MaterialSkin.Controls.MaterialButton btnRemoveUser = null!;
        private MaterialSkin.Controls.MaterialButton btnOK = null!;
        private MaterialSkin.Controls.MaterialButton btnCancel = null!;
    }
}
