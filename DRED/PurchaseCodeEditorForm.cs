using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using MaterialSkin.Controls;

namespace DRED
{
    /// <summary>
    /// Allows the user to view, add, edit, and delete device code → purchase code mappings.
    /// Supports multiple purchase codes per device code.
    /// </summary>
    public class PurchaseCodeEditorForm : Form
    {
        private ListView lvMappings = null!;
        private TextBox txtDevCode = null!;
        private TextBox txtPurchaseCode = null!;
        private MaterialButton btnAdd = null!;
        private MaterialButton btnEdit = null!;
        private MaterialButton btnDelete = null!;
        private MaterialButton btnClose = null!;

        private (string dev, string code)? _pendingEdit = null;

        public PurchaseCodeEditorForm()
        {
            BuildForm();
            LoadMappings();
        }

        private void BuildForm()
        {
            this.Text = "Purchase Code Editor";
            this.Size = new Size(520, 540);
            this.MinimumSize = new Size(420, 400);
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = ThemeManager.BackgroundColor;
            this.ForeColor = ThemeManager.TextColor;

            var lblInfo = new Label
            {
                Text = "Manage device code → purchase code mappings (one row per mapping):",
                Dock = DockStyle.Top,
                Height = 36,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(8, 0, 0, 0),
                ForeColor = ThemeManager.SecondaryTextColor,
                BackColor = ThemeManager.SurfaceColor,
                Font = new Font("Segoe UI", 9F),
            };

            lvMappings = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                MultiSelect = true,
                BackColor = ThemeManager.ListBoxDarkColor,
                ForeColor = ThemeManager.TextColor,
                BorderStyle = BorderStyle.None,
                Font = new Font("Consolas", 10F),
            };
            lvMappings.Columns.Add("Device Code", 180, HorizontalAlignment.Left);
            lvMappings.Columns.Add("Purchase Code", 240, HorizontalAlignment.Left);

            var pnlInput = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 52,
                BackColor = ThemeManager.SurfaceColor,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(8, 10, 8, 0),
            };

            var lblDev = new Label
            {
                Text = "Dev Code:",
                AutoSize = false,
                Width = 68,
                Height = 26,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = ThemeManager.SecondaryTextColor,
                Font = new Font("Segoe UI", 9F),
                Margin = new Padding(0, 2, 4, 0),
            };
            txtDevCode = new TextBox
            {
                Width = 72,
                Height = 26,
                MaxLength = 5,
                BackColor = ThemeManager.InputBackColor,
                ForeColor = ThemeManager.TextColor,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Consolas", 10F),
                Margin = new Padding(0, 2, 12, 0),
            };

            var lblCode = new Label
            {
                Text = "Pur Code:",
                AutoSize = false,
                Width = 68,
                Height = 26,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = ThemeManager.SecondaryTextColor,
                Font = new Font("Segoe UI", 9F),
                Margin = new Padding(0, 2, 4, 0),
            };
            txtPurchaseCode = new TextBox
            {
                Width = 86,
                Height = 26,
                MaxLength = 20,
                BackColor = ThemeManager.InputBackColor,
                ForeColor = ThemeManager.TextColor,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Consolas", 10F),
                Margin = new Padding(0, 2, 12, 0),
            };

            btnAdd = new MaterialButton
            {
                Text = "Add",
                Type = MaterialButton.MaterialButtonType.Contained,
                HighEmphasis = true,
                AutoSize = true,
                Margin = new Padding(0, 0, 4, 0),
            };
            btnAdd.Click += BtnAdd_Click;

            pnlInput.Controls.AddRange(new Control[] { lblDev, txtDevCode, lblCode, txtPurchaseCode, btnAdd });

            var pnlBtn = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 46,
                BackColor = ThemeManager.ButtonPanelColor,
            };

            btnEdit = new MaterialButton
            {
                Text = "Edit Selected",
                Location = new Point(8, 8),
                Type = MaterialButton.MaterialButtonType.Outlined,
                AutoSize = true,
            };
            btnDelete = new MaterialButton
            {
                Text = "Delete Selected",
                Location = new Point(140, 8),
                Type = MaterialButton.MaterialButtonType.Outlined,
                AutoSize = true,
            };
            btnClose = new MaterialButton
            {
                Text = "Close",
                Location = new Point(290, 8),
                Type = MaterialButton.MaterialButtonType.Text,
                AutoSize = true,
            };

            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
            btnClose.Click += (s, e) => this.Close();

            pnlBtn.Controls.Add(btnEdit);
            pnlBtn.Controls.Add(btnDelete);
            pnlBtn.Controls.Add(btnClose);

            this.Controls.Add(lvMappings);
            this.Controls.Add(pnlInput);
            this.Controls.Add(pnlBtn);
            this.Controls.Add(lblInfo);

            this.CancelButton = btnClose;
        }

        private void LoadMappings()
        {
            lvMappings.Items.Clear();
            foreach (var (dev, code) in PurchaseCodeManager.GetAll())
            {
                var item = new ListViewItem(dev);
                item.SubItems.Add(code);
                lvMappings.Items.Add(item);
            }
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            string dev = txtDevCode.Text.Trim().ToUpperInvariant();
            string code = txtPurchaseCode.Text.Trim().ToUpperInvariant();

            if (dev.Length == 0 || code.Length == 0)
            {
                MessageBox.Show("Please enter both a device code and a purchase code.",
                    "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (dev.Length != 5)
            {
                MessageBox.Show("Device code must be exactly 5 characters.",
                    "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_pendingEdit.HasValue)
            {
                PurchaseCodeManager.RemoveMapping(_pendingEdit.Value.dev, _pendingEdit.Value.code);
                _pendingEdit = null;
                btnAdd.Text = "Add";
            }

            PurchaseCodeManager.AddMapping(dev, code);
            txtDevCode.Text = "";
            txtPurchaseCode.Text = "";
            LoadMappings();
        }

        private void BtnEdit_Click(object? sender, EventArgs e)
        {
            if (lvMappings.SelectedItems.Count != 1)
            {
                MessageBox.Show("Please select exactly one row to edit.",
                    "Edit", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var item = lvMappings.SelectedItems[0];
            string dev = item.Text;
            string code = item.SubItems[1].Text;
            _pendingEdit = (dev, code);
            txtDevCode.Text = dev;
            txtPurchaseCode.Text = code;
            btnAdd.Text = "Update";
            txtDevCode.Focus();
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (lvMappings.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select one or more rows to delete.",
                    "Delete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show(
                    $"Delete {lvMappings.SelectedItems.Count} selected mapping(s)?",
                    "Confirm Delete",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            var toRemove = new List<(string dev, string code)>();
            foreach (ListViewItem item in lvMappings.SelectedItems)
                toRemove.Add((item.Text, item.SubItems[1].Text));

            foreach (var (dev, code) in toRemove)
                PurchaseCodeManager.RemoveMapping(dev, code);

            LoadMappings();
        }
    }
}
