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
            this.txtDatabasePath  = new System.Windows.Forms.TextBox();
            this.btnBrowse        = new System.Windows.Forms.Button();
            this.lblAutoRefresh   = new System.Windows.Forms.Label();
            this.nudAutoRefresh   = new System.Windows.Forms.NumericUpDown();
            this.btnOK            = new System.Windows.Forms.Button();
            this.btnCancel        = new System.Windows.Forms.Button();
            this.lblInfo          = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.nudAutoRefresh)).BeginInit();
            this.SuspendLayout();

            // lblInfo
            this.lblInfo.AutoSize = false;
            this.lblInfo.Location = new System.Drawing.Point(12, 12);
            this.lblInfo.Size = new System.Drawing.Size(560, 40);
            this.lblInfo.Text = "Specify the path to the DRED Access database file (.accdb). For multi-user " +
                                "access, point this to a file on a shared network drive.";

            // lblDatabasePath
            this.lblDatabasePath.AutoSize = true;
            this.lblDatabasePath.Location = new System.Drawing.Point(12, 62);
            this.lblDatabasePath.Text = "Database Path:";

            // txtDatabasePath
            this.txtDatabasePath.Location = new System.Drawing.Point(12, 80);
            this.txtDatabasePath.Size = new System.Drawing.Size(460, 23);
            this.txtDatabasePath.TabIndex = 0;

            // btnBrowse
            this.btnBrowse.Location = new System.Drawing.Point(480, 79);
            this.btnBrowse.Size = new System.Drawing.Size(92, 25);
            this.btnBrowse.Text = "Browse...";
            this.btnBrowse.TabIndex = 1;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);

            // lblAutoRefresh
            this.lblAutoRefresh.AutoSize = true;
            this.lblAutoRefresh.Location = new System.Drawing.Point(12, 118);
            this.lblAutoRefresh.Text = "Auto-Refresh Interval (seconds, 0 = disabled):";

            // nudAutoRefresh
            this.nudAutoRefresh.Location = new System.Drawing.Point(12, 136);
            this.nudAutoRefresh.Size = new System.Drawing.Size(80, 23);
            this.nudAutoRefresh.Minimum = 0;
            this.nudAutoRefresh.Maximum = 600;
            this.nudAutoRefresh.Value = 60;
            this.nudAutoRefresh.TabIndex = 2;

            // btnOK
            this.btnOK.Location = new System.Drawing.Point(400, 174);
            this.btnOK.Size = new System.Drawing.Size(80, 28);
            this.btnOK.Text = "OK";
            this.btnOK.TabIndex = 3;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);

            // btnCancel
            this.btnCancel.Location = new System.Drawing.Point(492, 174);
            this.btnCancel.Size = new System.Drawing.Size(80, 28);
            this.btnCancel.Text = "Cancel";
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);

            // SettingsForm
            this.AcceptButton = this.btnOK;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(584, 216);
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
        private System.Windows.Forms.TextBox txtDatabasePath = null!;
        private System.Windows.Forms.Button btnBrowse = null!;
        private System.Windows.Forms.Label lblAutoRefresh = null!;
        private System.Windows.Forms.NumericUpDown nudAutoRefresh = null!;
        private System.Windows.Forms.Button btnOK = null!;
        private System.Windows.Forms.Button btnCancel = null!;
    }
}

