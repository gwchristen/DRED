namespace DRED
{
    partial class PinEntryForm
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
            this.lblPrompt = new System.Windows.Forms.Label();
            this.txtPin = new MaterialSkin.Controls.MaterialTextBox2();
            this.btnOK = new MaterialSkin.Controls.MaterialButton();
            this.btnCancel = new MaterialSkin.Controls.MaterialButton();
            this.SuspendLayout();

            // lblPrompt
            this.lblPrompt.AutoSize = true;
            this.lblPrompt.ForeColor = System.Drawing.Color.FromArgb(204, 204, 204);
            this.lblPrompt.Location = new System.Drawing.Point(12, 20);
            this.lblPrompt.Text = "Enter lock PIN:";

            // txtPin
            this.txtPin.UseTallSize = false;
            this.txtPin.UseSystemPasswordChar = true;
            this.txtPin.Location = new System.Drawing.Point(12, 38);
            this.txtPin.Size = new System.Drawing.Size(260, 48);
            this.txtPin.TabIndex = 0;

            // btnOK
            this.btnOK.AutoSize = true;
            this.btnOK.Text = "OK";
            this.btnOK.Location = new System.Drawing.Point(116, 94);
            this.btnOK.TabIndex = 1;
            this.btnOK.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);

            // btnCancel
            this.btnCancel.AutoSize = true;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.Location = new System.Drawing.Point(186, 94);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);

            // PinEntryForm
            this.AcceptButton = this.btnOK;
            this.CancelButton = this.btnCancel;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(45, 45, 48);
            this.ClientSize = new System.Drawing.Size(284, 140);
            this.Controls.Add(this.lblPrompt);
            this.Controls.Add(this.txtPin);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.ForeColor = System.Drawing.Color.FromArgb(241, 241, 241);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PinEntryForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Unlock";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label lblPrompt = null!;
        private MaterialSkin.Controls.MaterialTextBox2 txtPin = null!;
        private MaterialSkin.Controls.MaterialButton btnOK = null!;
        private MaterialSkin.Controls.MaterialButton btnCancel = null!;
    }
}
