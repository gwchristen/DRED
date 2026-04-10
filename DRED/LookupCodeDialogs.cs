using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using MaterialSkin.Controls;

namespace DRED
{
    /// <summary>
    /// Prompts the user to manually enter a lookup code when none is configured for a device code.
    /// </summary>
    public class LookupCodeInputDialog : Form
    {
        public string LookupCode { get; private set; } = "";

        private TextBox        txtCode  = null!;
        private MaterialButton btnOK    = null!;
        private MaterialButton btnCancel = null!;

        public LookupCodeInputDialog(string devCode)
        {
            this.Text            = "Lookup Code Required";
            this.Size            = new Size(360, 180);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition   = FormStartPosition.CenterParent;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.BackColor       = Color.FromArgb(0x1E, 0x1E, 0x1E);
            this.ForeColor       = Color.FromArgb(0xF1, 0xF1, 0xF1);

            var lbl = new Label
            {
                Text      = $"No lookup code found for device code '{devCode}'.\nEnter the 2-character lookup code:",
                Location  = new Point(12, 14),
                Size      = new Size(324, 40),
                ForeColor = Color.FromArgb(0xCC, 0xCC, 0xCC),
                Font      = new Font("Segoe UI", 9F),
            };

            txtCode = new TextBox
            {
                Location    = new Point(12, 62),
                Width       = 80,
                MaxLength   = 2,
                BackColor   = Color.FromArgb(0x32, 0x32, 0x32),
                ForeColor   = Color.FromArgb(0xF1, 0xF1, 0xF1),
                BorderStyle = BorderStyle.FixedSingle,
                Font        = new Font("Consolas", 11F),
            };

            btnOK = new MaterialButton
            {
                Text         = "OK",
                Location     = new Point(106, 56),
                Type         = MaterialButton.MaterialButtonType.Contained,
                HighEmphasis = true,
                AutoSize     = true,
            };
            btnCancel = new MaterialButton
            {
                Text     = "Cancel",
                Location = new Point(200, 56),
                Type     = MaterialButton.MaterialButtonType.Outlined,
                AutoSize = true,
            };

            btnOK.Click     += BtnOK_Click;
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            this.Controls.AddRange(new Control[] { lbl, txtCode, btnOK, btnCancel });
            this.AcceptButton = btnOK;
            this.CancelButton = btnCancel;
        }

        private void BtnOK_Click(object? sender, EventArgs e)
        {
            string code = txtCode.Text.Trim().ToUpperInvariant();
            if (code.Length == 0)
            {
                MessageBox.Show("Please enter a lookup code.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            LookupCode   = code;
            DialogResult = DialogResult.OK;
            Close();
        }
    }

    /// <summary>
    /// Prompts the user to pick one lookup code from a list when multiple codes map to the same device code.
    /// </summary>
    public class LookupCodePickerDialog : Form
    {
        public string SelectedCode { get; private set; } = "";

        private ListBox        lstCodes  = null!;
        private MaterialButton btnOK     = null!;
        private MaterialButton btnCancel = null!;

        public LookupCodePickerDialog(string devCode, List<string> codes)
        {
            this.Text            = "Select Lookup Code";
            this.Size            = new Size(320, 260);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition   = FormStartPosition.CenterParent;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.BackColor       = Color.FromArgb(0x1E, 0x1E, 0x1E);
            this.ForeColor       = Color.FromArgb(0xF1, 0xF1, 0xF1);

            var lbl = new Label
            {
                Text      = $"Device code '{devCode}' maps to multiple lookup codes.\nSelect one to use:",
                Location  = new Point(12, 12),
                Size      = new Size(288, 40),
                ForeColor = Color.FromArgb(0xCC, 0xCC, 0xCC),
                Font      = new Font("Segoe UI", 9F),
            };

            lstCodes = new ListBox
            {
                Location    = new Point(12, 58),
                Size        = new Size(288, 120),
                BackColor   = Color.FromArgb(0x1A, 0x1A, 0x1A),
                ForeColor   = Color.FromArgb(0xF1, 0xF1, 0xF1),
                BorderStyle = BorderStyle.FixedSingle,
                Font        = new Font("Consolas", 11F),
            };
            foreach (string c in codes)
                lstCodes.Items.Add(c);
            if (lstCodes.Items.Count > 0)
                lstCodes.SelectedIndex = 0;
            lstCodes.DoubleClick += (s, e) => BtnOK_Click(s, e);

            btnOK = new MaterialButton
            {
                Text         = "OK",
                Location     = new Point(12, 190),
                Type         = MaterialButton.MaterialButtonType.Contained,
                HighEmphasis = true,
                AutoSize     = true,
            };
            btnCancel = new MaterialButton
            {
                Text     = "Cancel",
                Location = new Point(110, 190),
                Type     = MaterialButton.MaterialButtonType.Outlined,
                AutoSize = true,
            };

            btnOK.Click     += BtnOK_Click;
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            this.Controls.AddRange(new Control[] { lbl, lstCodes, btnOK, btnCancel });
            this.AcceptButton = btnOK;
            this.CancelButton = btnCancel;
        }

        private void BtnOK_Click(object? sender, EventArgs e)
        {
            if (lstCodes.SelectedItem is not string code || string.IsNullOrWhiteSpace(code))
            {
                MessageBox.Show("Please select a lookup code.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            SelectedCode = code;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
