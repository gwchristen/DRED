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
            this.btnOK            = new MaterialSkin.Controls.MaterialButton();
            this.btnCancel        = new MaterialSkin.Controls.MaterialButton();
            this.lblInfo          = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.nudAutoRefresh)).BeginInit();
            this.SuspendLayout();

            // lblInfo
            this.lblInfo.AutoSize = false;
            this.lblInfo.Location = new System.Drawing.Point(12, 80);
            this.lblInfo.Size = new System.Drawing.Size(560, 40);
            this.lblInfo.ForeColor = System.Drawing.Color.FromArgb(204, 204, 204);
            this.lblInfo.Text = "Specify the path to the DRED Access database file (.accdb). For multi-user " +
                                "access, point this to a file on a shared network drive.";

            // lblDatabasePath
            this.lblDatabasePath.AutoSize = true;
            this.lblDatabasePath.Location = new System.Drawing.Point(12, 130);
            this.lblDatabasePath.ForeColor = System.Drawing.Color.FromArgb(204, 204, 204);
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
            this.lblAutoRefresh.ForeColor = System.Drawing.Color.FromArgb(204, 204, 204);
            this.lblAutoRefresh.Text = "Auto-Refresh Interval (seconds, 0 = disabled):";

            // nudAutoRefresh
            this.nudAutoRefresh.Location = new System.Drawing.Point(12, 228);
            this.nudAutoRefresh.Size = new System.Drawing.Size(80, 23);
            this.nudAutoRefresh.Minimum = 0;
            this.nudAutoRefresh.Maximum = 600;
            this.nudAutoRefresh.Value = 60;
            this.nudAutoRefresh.TabIndex = 2;
            this.nudAutoRefresh.BackColor = System.Drawing.Color.FromArgb(50, 50, 50);
            this.nudAutoRefresh.ForeColor = System.Drawing.Color.White;

            // btnOK
            this.btnOK.Location = new System.Drawing.Point(400, 270);
            this.btnOK.Text = "OK";
            this.btnOK.TabIndex = 3;
            this.btnOK.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnOK.HighEmphasis = true;
            this.btnOK.AutoSize = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);

            // btnCancel
            this.btnCancel.Location = new System.Drawing.Point(480, 270);
            this.btnCancel.Text = "Cancel";
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined;
            this.btnCancel.AutoSize = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);

            // SettingsForm
            this.AcceptButton = this.btnOK;
            this.CancelButton = this.btnCancel;
            this.BackColor = System.Drawing.Color.FromArgb(45, 45, 48);
            this.ForeColor = System.Drawing.Color.FromArgb(241, 241, 241);
            this.ClientSize = new System.Drawing.Size(584, 316);
            this.Controls.Add(this.lblInfo);
            this.Controls.Add(this.lblDatabasePath);
            this.Controls.Add(this.txtDatabasePath);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.lblAutoRefresh);
            this.Controls.Add(this.nudAutoRefresh);
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
        private MaterialSkin.Controls.MaterialButton btnOK = null!;
        private MaterialSkin.Controls.MaterialButton btnCancel = null!;
    }
}

