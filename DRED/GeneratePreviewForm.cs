using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using MaterialSkin.Controls;

namespace DRED
{
    /// <summary>
    /// Previews the generated serial numbers and allows saving to a .txt file.
    /// </summary>
    public class GeneratePreviewForm : Form
    {
        private readonly List<string> _serials;
        private readonly string _devCode;

        private RichTextBox txtPreview   = null!;
        private Label       lblCount     = null!;
        private MaterialButton btnSave   = null!;
        private MaterialButton btnCopy   = null!;
        private MaterialButton btnClose  = null!;

        public GeneratePreviewForm(List<string> serials, string devCode)
        {
            _serials = serials;
            _devCode = devCode;
            BuildForm();
        }

        private void BuildForm()
        {
            this.Text            = "Serial Number Preview";
            this.Size            = new Size(480, 560);
            this.MinimumSize     = new Size(380, 400);
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.StartPosition   = FormStartPosition.CenterParent;
            this.BackColor       = Color.FromArgb(0x1E, 0x1E, 0x1E);
            this.ForeColor       = Color.FromArgb(0xF1, 0xF1, 0xF1);

            // Count label
            lblCount = new Label
            {
                Text      = $"Total serials: {_serials.Count}   |   Device Code: {_devCode}",
                Dock      = DockStyle.Top,
                Height    = 32,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(8, 0, 0, 0),
                ForeColor = Color.FromArgb(0xCC, 0xCC, 0xCC),
                BackColor = Color.FromArgb(0x2D, 0x2D, 0x30),
                Font      = new Font("Segoe UI", 9.5F),
            };

            // Read-only text area
            txtPreview = new RichTextBox
            {
                Dock      = DockStyle.Fill,
                ReadOnly  = true,
                BackColor = Color.FromArgb(0x1A, 0x1A, 0x1A),
                ForeColor = Color.FromArgb(0xF1, 0xF1, 0xF1),
                Font      = new Font("Consolas", 10F),
                Text      = string.Join(Environment.NewLine, _serials),
                BorderStyle = BorderStyle.None,
                ScrollBars = RichTextBoxScrollBars.Both,
                WordWrap   = false,
            };

            // Button panel
            var pnlButtons = new FlowLayoutPanel
            {
                Dock          = DockStyle.Bottom,
                Height        = 48,
                BackColor     = Color.FromArgb(0x25, 0x25, 0x28),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents  = false,
                Padding       = new Padding(8, 8, 8, 8),
            };

            btnSave = new MaterialButton
            {
                Text         = "Save",
                Type         = MaterialButton.MaterialButtonType.Contained,
                HighEmphasis = true,
                AutoSize     = true,
                Margin       = new Padding(0, 0, 8, 0),
            };
            btnClose = new MaterialButton
            {
                Text     = "Close",
                Type     = MaterialButton.MaterialButtonType.Outlined,
                AutoSize = true,
                Margin   = new Padding(0, 0, 0, 0),
            };
            btnCopy = new MaterialButton
            {
                Text     = "Copy",
                Type     = MaterialButton.MaterialButtonType.Outlined,
                AutoSize = true,
                Margin   = new Padding(0, 0, 8, 0),
            };

            btnSave.Click  += BtnSave_Click;
            btnCopy.Click  += BtnCopy_Click;
            btnClose.Click += (s, e) => this.Close();

            pnlButtons.Controls.Add(btnSave);
            pnlButtons.Controls.Add(btnCopy);
            pnlButtons.Controls.Add(btnClose);

            this.Controls.Add(txtPreview);
            this.Controls.Add(lblCount);
            this.Controls.Add(pnlButtons);

            this.CancelButton = btnClose;
        }

        private void BtnCopy_Click(object? sender, EventArgs e)
        {
            try
            {
                Clipboard.SetText(string.Join(Environment.NewLine, _serials));
                MessageBox.Show(
                    $"Copied {_serials.Count} serial(s) to clipboard.",
                    "Copied",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to copy to clipboard:\n{ex.Message}",
                    "Copy Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            using var dlg = new SaveFileDialog
            {
                Title       = "Save Serial Numbers",
                Filter      = "Text File (*.txt)|*.txt",
                DefaultExt  = "txt",
                FileName    = $"{_devCode}_serials_{DateTime.Now:yyyyMMdd}",
            };

            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            try
            {
                File.WriteAllLines(dlg.FileName, _serials);
                MessageBox.Show(
                    $"Saved {_serials.Count} serial(s) to:\n{dlg.FileName}",
                    "Save Complete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to save file:\n{ex.Message}",
                    "Save Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
