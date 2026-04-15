using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MaterialSkin.Controls;

namespace DRED
{
    /// <summary>
    /// Allows the user to view, add, edit, and delete device code → lookup code mappings.
    /// Supports multiple lookup codes per device code.
    /// </summary>
    public class LookupCodeEditorForm : Form
    {
        private ListView   lvMappings = null!;
        private TextBox    txtDevCode  = null!;
        private TextBox    txtLookup   = null!;
        private MaterialButton btnAdd     = null!;
        private MaterialButton btnEdit    = null!;
        private MaterialButton btnDelete  = null!;
        private MaterialButton btnClose   = null!;

        // When non-null, we are in edit mode: this holds the original (dev, lc) being edited
        private (string dev, string lc)? _pendingEdit = null;

        public LookupCodeEditorForm()
        {
            BuildForm();
            LoadMappings();
        }

        private void BuildForm()
        {
            this.Text            = "Lookup Code Editor";
            this.Size            = new Size(480, 540);
            this.MinimumSize     = new Size(380, 400);
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.StartPosition   = FormStartPosition.CenterParent;
            this.BackColor       = ThemeManager.BackgroundColor;
            this.ForeColor       = ThemeManager.TextColor;

            // ── Header instruction label ─────────────────────────────────
            var lblInfo = new Label
            {
                Text      = "Manage device code → lookup code mappings (one row per mapping):",
                Dock      = DockStyle.Top,
                Height    = 36,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(8, 0, 0, 0),
                ForeColor = ThemeManager.SecondaryTextColor,
                BackColor = ThemeManager.SurfaceColor,
                Font      = new Font("Segoe UI", 9F),
            };

            // ── ListView ─────────────────────────────────────────────────
            lvMappings = new ListView
            {
                Dock         = DockStyle.Fill,
                View         = View.Details,
                FullRowSelect = true,
                GridLines    = true,
                MultiSelect  = true,
                BackColor    = ThemeManager.ListBoxDarkColor,
                ForeColor    = ThemeManager.TextColor,
                BorderStyle  = BorderStyle.None,
                Font         = new Font("Consolas", 10F),
            };
            lvMappings.Columns.Add("Device Code", 180, HorizontalAlignment.Left);
            lvMappings.Columns.Add("Lookup Code", 180, HorizontalAlignment.Left);

            // ── Input panel (add/edit mapping) ────────────────────────────
            var pnlInput = new FlowLayoutPanel
            {
                Dock          = DockStyle.Bottom,
                Height        = 52,
                BackColor     = ThemeManager.SurfaceColor,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents  = false,
                Padding       = new Padding(8, 10, 8, 0),
            };

            var lblDev = new Label
            {
                Text      = "Dev Code:",
                AutoSize  = false,
                Width     = 68,
                Height    = 26,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = ThemeManager.SecondaryTextColor,
                Font      = new Font("Segoe UI", 9F),
                Margin    = new Padding(0, 2, 4, 0),
            };
            txtDevCode = new TextBox
            {
                Width       = 72,
                Height      = 26,
                MaxLength   = 5,
                BackColor   = ThemeManager.InputBackColor,
                ForeColor   = ThemeManager.TextColor,
                BorderStyle = BorderStyle.FixedSingle,
                Font        = new Font("Consolas", 10F),
                Margin      = new Padding(0, 2, 12, 0),
            };

            var lblLkp = new Label
            {
                Text      = "Lookup:",
                AutoSize  = false,
                Width     = 54,
                Height    = 26,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = ThemeManager.SecondaryTextColor,
                Font      = new Font("Segoe UI", 9F),
                Margin    = new Padding(0, 2, 4, 0),
            };
            txtLookup = new TextBox
            {
                Width       = 54,
                Height      = 26,
                MaxLength   = 2,
                BackColor   = ThemeManager.InputBackColor,
                ForeColor   = ThemeManager.TextColor,
                BorderStyle = BorderStyle.FixedSingle,
                Font        = new Font("Consolas", 10F),
                Margin      = new Padding(0, 2, 12, 0),
            };

            btnAdd = new MaterialButton
            {
                Text         = "Add",
                Type         = MaterialButton.MaterialButtonType.Contained,
                HighEmphasis = true,
                AutoSize     = true,
                Margin       = new Padding(0, 0, 4, 0),
            };
            btnAdd.Click += BtnAdd_Click;

            pnlInput.Controls.AddRange(new Control[] { lblDev, txtDevCode, lblLkp, txtLookup, btnAdd });

            // ── Bottom button panel ───────────────────────────────────────
            var pnlBtn = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 46,
                BackColor = ThemeManager.ButtonPanelColor,
            };

            btnEdit = new MaterialButton
            {
                Text     = "Edit Selected",
                Location = new Point(8, 8),
                Type     = MaterialButton.MaterialButtonType.Outlined,
                AutoSize = true,
            };
            btnDelete = new MaterialButton
            {
                Text     = "Delete Selected",
                Location = new Point(140, 8),
                Type     = MaterialButton.MaterialButtonType.Outlined,
                AutoSize = true,
            };
            btnClose = new MaterialButton
            {
                Text     = "Close",
                Location = new Point(290, 8),
                Type     = MaterialButton.MaterialButtonType.Text,
                AutoSize = true,
            };

            btnEdit.Click   += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
            btnClose.Click  += (s, e) => this.Close();

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
            foreach (var (dev, lc) in LookupCodeManager.GetAll())
            {
                var item = new ListViewItem(dev);
                item.SubItems.Add(lc);
                lvMappings.Items.Add(item);
            }
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            string dev = txtDevCode.Text.Trim().ToUpperInvariant();
            string lc  = txtLookup.Text.Trim().ToUpperInvariant();

            if (dev.Length == 0 || lc.Length == 0)
            {
                MessageBox.Show("Please enter both a device code and a lookup code.",
                    "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (dev.Length != 5)
            {
                MessageBox.Show("Device code must be exactly 5 characters.",
                    "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (lc.Length != 2)
            {
                MessageBox.Show("Lookup code must be exactly 2 characters.",
                    "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // If in edit mode, remove the original mapping first
            if (_pendingEdit.HasValue)
            {
                LookupCodeManager.RemoveMapping(_pendingEdit.Value.dev, _pendingEdit.Value.lc);
                _pendingEdit = null;
                btnAdd.Text = "Add";
            }

            LookupCodeManager.AddMapping(dev, lc);
            txtDevCode.Text = "";
            txtLookup.Text  = "";
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
            string lc  = item.SubItems[1].Text;

            // Enter edit mode: populate input fields and track what we are editing.
            // The original mapping is only removed when the user clicks "Update".
            _pendingEdit = (dev, lc);
            txtDevCode.Text = dev;
            txtLookup.Text  = lc;
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

            // Collect all (dev, lc) pairs to remove, then remove them
            var toRemove = new List<(string dev, string lc)>();
            foreach (ListViewItem item in lvMappings.SelectedItems)
                toRemove.Add((item.Text, item.SubItems[1].Text));

            foreach (var (dev, lc) in toRemove)
                LookupCodeManager.RemoveMapping(dev, lc);

            LoadMappings();
        }
    }
}
