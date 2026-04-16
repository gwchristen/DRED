using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ClosedXML.Excel;
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
        private ComboBox cboTableFilter = null!;
        private MaterialButton btnAdd = null!;
        private MaterialButton btnEdit = null!;
        private MaterialButton btnDelete = null!;
        private MaterialButton btnImport = null!;
        private MaterialButton btnClose = null!;
        private Label lblInfo = null!;

        private static readonly string[] TableNames =
            { "OH_Meters", "IM_Meters", "OH_Transformers", "IM_Transformers" };

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

            var pnlTableFilter = new Panel
            {
                Dock = DockStyle.Top,
                Height = 44,
                BackColor = ThemeManager.SurfaceColor,
                Padding = new Padding(8, 8, 8, 6),
            };
            var lblTable = new Label
            {
                Text = "Table:",
                AutoSize = true,
                ForeColor = ThemeManager.SecondaryTextColor,
                BackColor = ThemeManager.SurfaceColor,
                Font = new Font("Segoe UI", 9F),
                Location = new Point(0, 11),
            };
            cboTableFilter = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 180,
                Location = new Point(52, 7),
                BackColor = ThemeManager.InputBackColor,
                ForeColor = ThemeManager.TextColor,
                Font = new Font("Segoe UI", 9F),
            };
            cboTableFilter.Items.AddRange(TableNames.Cast<object>().ToArray());
            cboTableFilter.SelectedIndexChanged += CboTableFilter_SelectedIndexChanged;
            if (cboTableFilter.Items.Count > 0)
                cboTableFilter.SelectedIndex = 0;
            pnlTableFilter.Controls.Add(lblTable);
            pnlTableFilter.Controls.Add(cboTableFilter);

            lblInfo = new Label
            {
                Text = "Managing mappings for:",
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
                Location = new Point(132, 8),
                Type = MaterialButton.MaterialButtonType.Outlined,
                AutoSize = true,
            };
            btnImport = new MaterialButton
            {
                Text = "Import from Excel",
                Location = new Point(272, 8),
                Type = MaterialButton.MaterialButtonType.Outlined,
                AutoSize = true,
            };
            btnClose = new MaterialButton
            {
                Text = "Close",
                Location = new Point(444, 8),
                Type = MaterialButton.MaterialButtonType.Text,
                AutoSize = true,
            };

            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
            btnImport.Click += BtnImport_Click;
            btnClose.Click += (s, e) => this.Close();

            pnlBtn.Controls.Add(btnEdit);
            pnlBtn.Controls.Add(btnDelete);
            pnlBtn.Controls.Add(btnImport);
            pnlBtn.Controls.Add(btnClose);

            this.Controls.Add(lvMappings);
            this.Controls.Add(pnlInput);
            this.Controls.Add(pnlBtn);
            this.Controls.Add(lblInfo);
            this.Controls.Add(pnlTableFilter);

            this.CancelButton = btnClose;
        }

        private void LoadMappings()
        {
            if (lvMappings == null || lblInfo == null) return;

            lvMappings.Items.Clear();
            foreach (var (dev, code) in PurchaseCodeManager.GetAllForTable(GetSelectedTable()))
            {
                var item = new ListViewItem(dev);
                item.SubItems.Add(code);
                lvMappings.Items.Add(item);
            }

            lblInfo.Text = $"Managing mappings for: {GetSelectedTable()}";
        }

        private string GetSelectedTable() =>
            cboTableFilter.SelectedItem?.ToString() ?? TableNames[0];

        private void CboTableFilter_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (btnAdd == null) return;

            _pendingEdit = null;
            btnAdd.Text = "Add";
            txtDevCode.Text = string.Empty;
            txtPurchaseCode.Text = string.Empty;
            LoadMappings();
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            string tableName = GetSelectedTable();
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
                PurchaseCodeManager.RemoveMapping(tableName, _pendingEdit.Value.dev, _pendingEdit.Value.code);
                _pendingEdit = null;
                btnAdd.Text = "Add";
            }

            PurchaseCodeManager.AddMapping(tableName, dev, code);
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
                PurchaseCodeManager.RemoveMapping(GetSelectedTable(), dev, code);

            LoadMappings();
        }

        private void BtnImport_Click(object? sender, EventArgs e)
        {
            string selectedTable = GetSelectedTable();
            using var ofd = new OpenFileDialog
            {
                Title = "Import Purchase Codes from Excel",
                Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                Multiselect = false,
            };

            if (ofd.ShowDialog(this) != DialogResult.OK)
                return;

            List<(string DevCode, string PurchaseCode)> imported;
            try
            {
                imported = ReadMappingsFromExcel(ofd.FileName);
            }
            catch (InvalidDataException ex)
            {
                MessageBox.Show(ex.Message, "Import Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to import purchase codes from '{ofd.FileName}'.", ex);
                MessageBox.Show(
                    $"Failed to read Excel file.\n\n{ex.Message}\n\nMake sure the file is a valid .xlsx workbook and is not open in another program.",
                    "Import Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            var action = MessageBox.Show(
                $"Import {imported.Count} mapping(s) into {selectedTable}?\n\nYes = Replace all mappings for {selectedTable}\nNo = Merge with existing mappings\nCancel = Do nothing",
                "Import Purchase Codes",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (action == DialogResult.Cancel)
                return;

            bool replaced = action == DialogResult.Yes;
            if (replaced &&
                MessageBox.Show(
                    $"This will remove all existing mappings for {selectedTable} before import.\n\nContinue?",
                    "Confirm Replace",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                return;
            }

            if (replaced)
            {
                PurchaseCodeManager.SetAllForTable(selectedTable, imported);
            }
            else
            {
                foreach (var (devCode, purCode) in imported)
                    PurchaseCodeManager.AddMapping(selectedTable, devCode, purCode);
            }

            Logger.Log(
                $"Imported {imported.Count} purchase code mapping(s) into '{selectedTable}' from '{ofd.FileName}' ({(replaced ? "replace" : "merge")}).");

            LoadMappings();
            MessageBox.Show(
                $"Imported {imported.Count} mapping(s) into {selectedTable} ({(replaced ? "Replace" : "Merge")}).",
                "Import Complete",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private static List<(string DevCode, string PurchaseCode)> ReadMappingsFromExcel(string filePath)
        {
            using var workbook = new XLWorkbook(filePath);
            var sheet = workbook.Worksheets.FirstOrDefault();
            if (sheet == null)
                throw new InvalidDataException("The workbook does not contain any worksheets.");

            var headerRow = sheet.Row(1);
            int lastCol = sheet.LastColumnUsed()?.ColumnNumber() ?? 0;
            if (lastCol == 0)
                throw new InvalidDataException("No header row was found in the selected worksheet.");

            int devCol = -1;
            int purCol = -1;
            for (int c = 1; c <= lastCol; c++)
            {
                string normalizedHeader = NormalizeHeader(headerRow.Cell(c).GetString());
                if (IsDevCodeHeader(normalizedHeader))
                    devCol = c;
                if (IsPurchaseCodeHeader(normalizedHeader))
                    purCol = c;
            }

            if (devCol <= 0 || purCol <= 0)
                throw new InvalidDataException(
                    "Could not find required columns.\nExpected headers like Dev Code / DevCode / Device Code and Pur Code / PurCode / Purchase Code.");

            int lastRow = sheet.LastRowUsed()?.RowNumber() ?? 0;
            var mappings = new List<(string DevCode, string PurchaseCode)>();
            for (int r = 2; r <= lastRow; r++)
            {
                var row = sheet.Row(r);
                string devCode = row.Cell(devCol).GetString().Trim().ToUpperInvariant();
                string purCode = row.Cell(purCol).GetString().Trim().ToUpperInvariant();
                if (string.IsNullOrWhiteSpace(devCode) || string.IsNullOrWhiteSpace(purCode))
                    continue;
                mappings.Add((devCode, purCode));
            }

            return mappings
                .Distinct()
                .ToList();
        }

        private static string NormalizeHeader(string value)
        {
            var chars = value
                .Where(char.IsLetterOrDigit)
                .ToArray();
            return new string(chars).ToLowerInvariant();
        }

        private static bool IsDevCodeHeader(string normalizedHeader) =>
            normalizedHeader is "devcode" or "devicecode";

        private static bool IsPurchaseCodeHeader(string normalizedHeader) =>
            normalizedHeader is "purcode" or "purchasecode";
    }
}
