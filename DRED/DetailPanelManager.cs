using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MaterialSkin.Controls;

namespace DRED
{
    /// <summary>
    /// Manages the creation, population, and theming of record detail panels
    /// in the split-container right pane of each data tab.
    /// </summary>
    internal sealed class DetailPanelManager
    {
        private static readonly Color BooleanTrueColor = Color.FromArgb(0x66, 0xBB, 0x6A); // Material Green 400
        private static readonly Color BooleanFalseColor = Color.FromArgb(0xEF, 0x53, 0x50); // Material Red 400

        private Panel[] _detailPanels = Array.Empty<Panel>();
        private Color[] _accentColors = Array.Empty<Color>();
        private DetailLabels[] _detailLabels = Array.Empty<DetailLabels>();
        private EventHandler? _editHandler;
        private EventHandler? _deleteHandler;
        private EventHandler? _generateHandler;

        public void Initialize(Panel[] detailPanels, Color[] accentColors,
            EventHandler editHandler, EventHandler deleteHandler, EventHandler generateHandler)
        {
            _detailPanels = detailPanels;
            _accentColors = accentColors;
            _editHandler = editHandler;
            _deleteHandler = deleteHandler;
            _generateHandler = generateHandler;
            _detailLabels = new DetailLabels[_detailPanels.Length];

            for (int i = 0; i < _detailPanels.Length; i++)
                BuildDetailForTab(i, _detailPanels[i], _accentColors[i]);
        }

        public void PopulateDetail(int dataTabIndex, DataRow row)
        {
            if (dataTabIndex < 0 || dataTabIndex >= _detailLabels.Length) return;
            var dl = _detailLabels[dataTabIndex];
            if (dl == null) return;

            static string S(object? v) => v is DBNull || v == null ? "—" : v.ToString() ?? "—";
            static string D(object? v) => v is DBNull || v == null ? "—" : Convert.ToDateTime(v).ToString("MM/dd/yyyy");
            static string C(object? v) => v is DBNull || v == null ? "—" : Convert.ToDecimal(v).ToString("$#,##0.00");

            dl.ValOpCo2.Text = S(row["OpCo2"]);
            dl.ValStatus.Text = S(row["Status"]);
            dl.ValMFR.Text = S(row["MFR"]);
            dl.ValDevCode.Text = S(row["DevCode"]);

            dl.ValBegSer.Text = S(row["BegSer"]);
            dl.ValEndSer.Text = S(row["EndSer"]);
            dl.ValQty.Text = S(row["Qty"]);

            string oosRaw = row.Table.Columns.Contains("OOSSerials") && row["OOSSerials"] is not DBNull
                ? row["OOSSerials"] as string ?? "" : "";
            int oosCount = string.IsNullOrEmpty(oosRaw) ? 0
                : oosRaw.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Count(s => !string.IsNullOrWhiteSpace(s));
            dl.ValOOSSerials.Text = oosCount > 0 ? oosCount.ToString() : "—";

            dl.ValPODate.Text = D(row["PODate"]);
            dl.ValPONumber.Text = S(row["PONumber"]);
            dl.ValVintage.Text = S(row["Vintage"]);
            dl.ValRecvDate.Text = D(row["RecvDate"]);
            dl.ValUnitCost.Text = C(row["UnitCost"]);

            dl.ValCID.Text = S(row["CID"]);
            dl.ValMENumber.Text = S(row["MENumber"]);
            dl.ValPurCode.Text = S(row["PurCode"]);
            bool estVal = row.Table.Columns.Contains("Est") && row["Est"] is not DBNull && Convert.ToBoolean(row["Est"]);
            dl.ValEst.Text = estVal ? "✔ Yes" : "✘ No";
            dl.ValEst.ForeColor = estVal ? BooleanTrueColor : BooleanFalseColor;

            bool tfVal = row.Table.Columns.Contains("TextFile") && row["TextFile"] is not DBNull && Convert.ToBoolean(row["TextFile"]);
            dl.ValTextFile.Text = tfVal ? "✔ Yes" : "✘ No";
            dl.ValTextFile.ForeColor = tfVal ? BooleanTrueColor : BooleanFalseColor;

            dl.ValComments.Text = S(row["Comments"]);

            var auditParts = new List<string>();
            if (row.Table.Columns.Contains("CreatedBy") &&
                row["CreatedBy"] is not DBNull &&
                row["CreatedBy"] is string cb && cb.Length > 0)
            {
                string cd = row.Table.Columns.Contains("CreatedDate") && row["CreatedDate"] is not DBNull
                    ? Convert.ToDateTime(row["CreatedDate"]).ToString("MM/dd/yyyy") : "";
                auditParts.Add($"Created by {cb}" + (cd.Length > 0 ? $" on {cd}" : ""));
            }
            if (row.Table.Columns.Contains("ModifiedBy") &&
                row["ModifiedBy"] is not DBNull &&
                row["ModifiedBy"] is string mb && mb.Length > 0)
            {
                string md = row.Table.Columns.Contains("ModifiedDate") && row["ModifiedDate"] is not DBNull
                    ? Convert.ToDateTime(row["ModifiedDate"]).ToString("MM/dd/yyyy") : "";
                auditParts.Add($"Modified by {mb}" + (md.Length > 0 ? $" on {md}" : ""));
            }
            dl.LblAudit.Text = string.Join("   •   ", auditParts);

            dl.EmptyStateLabel.Visible = false;
            dl.ContentPanel.Visible = true;
        }

        public void ShowEmptyState(int dataTabIndex)
        {
            if (dataTabIndex < 0 || dataTabIndex >= _detailLabels.Length) return;
            var dl = _detailLabels[dataTabIndex];
            if (dl == null) return;
            dl.ContentPanel.Visible = false;
            dl.EmptyStateLabel.Visible = true;
        }

        public void ReapplyColors()
        {
            for (int i = 0; i < _detailLabels.Length; i++)
            {
                var dl = _detailLabels[i];
                if (dl == null) continue;

                var accent = _accentColors[i];
                var fieldNameColor = Color.FromArgb(0x99, 0x99, 0x99);
                var valueColor = Color.FromArgb(0xF1, 0xF1, 0xF1);

                dl.HdrDeviceInfo.ForeColor = accent;
                dl.HdrSerialRange.ForeColor = accent;
                dl.HdrPurchaseInfo.ForeColor = accent;
                dl.HdrIdentifiers.ForeColor = accent;
                dl.HdrComments.ForeColor = accent;

                foreach (var lbl in dl.FieldNameLabels)
                    lbl.ForeColor = fieldNameColor;

                foreach (var lbl in dl.ValueLabels)
                    lbl.ForeColor = valueColor;

                dl.ValEst.ForeColor = dl.ValEst.Text.StartsWith("✔")
                    ? BooleanTrueColor : BooleanFalseColor;
                dl.ValTextFile.ForeColor = dl.ValTextFile.Text.StartsWith("✔")
                    ? BooleanTrueColor : BooleanFalseColor;

                dl.LblAudit.ForeColor = Color.FromArgb(0x88, 0x88, 0x88);
                dl.EmptyStateLabel.ForeColor = Color.FromArgb(0x66, 0x66, 0x66);
                dl.ContentPanel.BackColor = Color.FromArgb(0x2D, 0x2D, 0x30);
            }
        }

        public void SetEditDeleteEnabled(bool enabled)
        {
            for (int i = 0; i < _detailLabels.Length; i++)
            {
                var dl = _detailLabels[i];
                if (dl == null) continue;
                dl.BtnDetailEdit.Enabled = enabled;
                dl.BtnDetailDelete.Enabled = enabled;
            }
        }

        private void BuildDetailForTab(int tabIndex, Panel scrollPanel, Color accentColor)
        {
            var dl = new DetailLabels();

            var emptyLabel = new Label
            {
                Text = "Select a record from the list",
                ForeColor = Color.FromArgb(0x66, 0x66, 0x66),
                Font = new Font("Segoe UI", 12F),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                Visible = true,
                BackColor = Color.Transparent,
            };
            dl.EmptyStateLabel = emptyLabel;
            scrollPanel.Controls.Add(emptyLabel);

            var contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(8),
                BackColor = Color.FromArgb(0x2D, 0x2D, 0x30),
                Visible = false,
            };
            dl.ContentPanel = contentPanel;
            scrollPanel.Controls.Add(contentPanel);

            var outerGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 5,
                BackColor = Color.Transparent,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None,
                Padding = Padding.Empty,
                Margin = Padding.Empty,
            };
            outerGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            outerGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            outerGrid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            outerGrid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            outerGrid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            outerGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
            outerGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            contentPanel.Controls.Add(outerGrid);

            (TableLayoutPanel card, Label hdr) MakeCard(string title)
            {
                var card = new TableLayoutPanel
                {
                    ColumnCount = 2,
                    RowCount = 1,
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    Dock = DockStyle.Fill,
                    BackColor = Color.FromArgb(0x25, 0x25, 0x28),
                    Margin = new Padding(4),
                    Padding = new Padding(8, 6, 8, 8),
                    CellBorderStyle = TableLayoutPanelCellBorderStyle.None,
                };
                card.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 75));
                card.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                var hdr = new Label
                {
                    Text = title,
                    ForeColor = accentColor,
                    Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold),
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.BottomLeft,
                    BackColor = Color.Transparent,
                    Padding = new Padding(0, 0, 0, 2),
                };
                card.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
                card.Controls.Add(hdr, 0, 0);
                card.SetColumnSpan(hdr, 2);
                return (card, hdr);
            }

            Label AddField(TableLayoutPanel card, string labelText)
            {
                int r = card.RowCount++;
                card.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
                var nm = new Label
                {
                    Text = labelText,
                    ForeColor = Color.FromArgb(0x99, 0x99, 0x99),
                    Font = new Font("Segoe UI", 8F),
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft,
                    BackColor = Color.Transparent,
                };
                var vl = new Label
                {
                    Text = "—",
                    ForeColor = Color.FromArgb(0xF1, 0xF1, 0xF1),
                    Font = new Font("Segoe UI", 9F),
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft,
                    BackColor = Color.Transparent,
                };
                card.Controls.Add(nm, 0, r);
                card.Controls.Add(vl, 1, r);
                dl.FieldNameLabels.Add(nm);
                dl.ValueLabels.Add(vl);
                return vl;
            }

            var (devCard, devHdr) = MakeCard("DEVICE INFORMATION");
            dl.HdrDeviceInfo = devHdr;
            dl.ValOpCo2 = AddField(devCard, "OpCo2:");
            dl.ValStatus = AddField(devCard, "Status:");
            dl.ValMFR = AddField(devCard, "MFR:");
            dl.ValDevCode = AddField(devCard, "Dev Code:");
            outerGrid.Controls.Add(devCard, 0, 0);

            var (serCard, serHdr) = MakeCard("SERIAL RANGE & QUANTITY");
            dl.HdrSerialRange = serHdr;
            dl.ValBegSer = AddField(serCard, "Beg Ser:");
            dl.ValEndSer = AddField(serCard, "End Ser:");
            dl.ValQty = AddField(serCard, "Qty:");
            dl.ValOOSSerials = AddField(serCard, "OOS:");
            outerGrid.Controls.Add(serCard, 1, 0);

            var (purCard, purHdr) = MakeCard("PURCHASE INFORMATION");
            dl.HdrPurchaseInfo = purHdr;
            dl.ValPODate = AddField(purCard, "PO Date:");
            dl.ValPONumber = AddField(purCard, "PO Number:");
            dl.ValVintage = AddField(purCard, "Vintage:");
            dl.ValRecvDate = AddField(purCard, "Est Date:");
            dl.ValUnitCost = AddField(purCard, "Unit Cost:");
            outerGrid.Controls.Add(purCard, 0, 1);

            var (idCard, idHdr) = MakeCard("IDENTIFIERS");
            dl.HdrIdentifiers = idHdr;
            dl.ValCID = AddField(idCard, "CID:");
            dl.ValMENumber = AddField(idCard, "M.E. #:");
            dl.ValPurCode = AddField(idCard, "Pur Code:");
            dl.ValEst = AddField(idCard, "Est.:");
            dl.ValTextFile = AddField(idCard, "Text File:");
            outerGrid.Controls.Add(idCard, 1, 1);

            var (commCard, commHdr) = MakeCard("COMMENTS & NOTES");
            dl.HdrComments = commHdr;
            int commRow = commCard.RowCount++;
            commCard.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
            var commLabel = new Label
            {
                Text = "—",
                ForeColor = Color.FromArgb(0xF1, 0xF1, 0xF1),
                Font = new Font("Segoe UI", 9F),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.TopLeft,
                BackColor = Color.Transparent,
            };
            commCard.Controls.Add(commLabel, 0, commRow);
            commCard.SetColumnSpan(commLabel, 2);
            dl.ValComments = commLabel;
            dl.ValueLabels.Add(commLabel);
            outerGrid.Controls.Add(commCard, 0, 2);
            outerGrid.SetColumnSpan(commCard, 2);

            var auditLabel = new Label
            {
                Text = "",
                ForeColor = Color.FromArgb(0x88, 0x88, 0x88),
                Font = new Font("Segoe UI", 8F, FontStyle.Italic),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent,
                Margin = new Padding(8, 0, 0, 0),
            };
            dl.LblAudit = auditLabel;
            outerGrid.Controls.Add(auditLabel, 0, 3);
            outerGrid.SetColumnSpan(auditLabel, 2);

            var btnDetailEdit = CreateDetailButton("Edit", accentColor, _editHandler!);
            var btnDetailDelete = CreateDetailButton("Delete", Color.FromArgb(0xEF, 0x53, 0x50), _deleteHandler!);
            var btnDetailGenerate = CreateDetailButton("Generate", accentColor, _generateHandler!);
            var btnPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = false,
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Margin = Padding.Empty,
                Padding = Padding.Empty,
            };
            btnPanel.Controls.Add(btnDetailEdit);
            btnPanel.Controls.Add(btnDetailDelete);
            btnPanel.Controls.Add(btnDetailGenerate);
            outerGrid.Controls.Add(btnPanel, 0, 4);
            outerGrid.SetColumnSpan(btnPanel, 2);
            dl.BtnDetailEdit = btnDetailEdit;
            dl.BtnDetailDelete = btnDetailDelete;
            dl.BtnDetailGenerate = btnDetailGenerate;

            _detailLabels[tabIndex] = dl;
        }

        private static MaterialButton CreateDetailButton(string text, Color color, EventHandler handler)
        {
            _ = color;
            var btn = new MaterialButton
            {
                Text = text,
                Type = MaterialButton.MaterialButtonType.Outlined,
                HighEmphasis = false,
                UseAccentColor = false,
                AutoSize = true,
            };
            btn.Click += handler;
            return btn;
        }

        private sealed class DetailLabels
        {
            public Label ValOpCo2 = null!, ValStatus = null!,
                ValMFR = null!, ValDevCode = null!;
            public Label ValBegSer = null!, ValEndSer = null!, ValQty = null!,
                ValOOSSerials = null!;
            public Label ValPODate = null!, ValPONumber = null!,
                ValVintage = null!, ValRecvDate = null!, ValUnitCost = null!;
            public Label ValCID = null!, ValMENumber = null!,
                ValPurCode = null!, ValEst = null!,
                ValTextFile = null!;
            public Label ValComments = null!;
            public Label LblAudit = null!;
            public Label EmptyStateLabel = null!;
            public Panel ContentPanel = null!;
            public MaterialButton BtnDetailEdit = null!, BtnDetailDelete = null!, BtnDetailGenerate = null!;
            public Label HdrDeviceInfo = null!, HdrSerialRange = null!,
                HdrPurchaseInfo = null!, HdrIdentifiers = null!,
                HdrComments = null!;
            public List<Label> FieldNameLabels = new();
            public List<Label> ValueLabels = new();
        }
    }
}
