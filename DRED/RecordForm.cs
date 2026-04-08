using System;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;

namespace DRED
{
    public partial class RecordForm : MaterialForm
    {
        public RecordData? Result { get; private set; }

        private const string CurrencyFormat = "$#,##0.00";
        private readonly bool _isEdit;
        private bool _suppressQtyAutoCalc = false;

        public RecordForm(RecordData? existing = null)
        {
            InitializeComponent();
            _isEdit = existing != null;
            this.Text = _isEdit ? "Edit Record" : "Add Record";
            lblAuditInfo.Visible = false;

            if (_isEdit && existing != null)
                PopulateFromRecord(existing);
        }

        private void PopulateFromRecord(RecordData r)
        {
            // Suppress auto-calc while populating
            _suppressQtyAutoCalc = true;

            txtOpCo2.Text    = r.OpCo2 ?? "";
            txtStatus.Text   = r.Status ?? "";
            txtMFR.Text      = r.MFR ?? "";
            txtDevCode.Text  = r.DevCode ?? "";
            txtBegSer.Text   = r.BegSer ?? "";
            txtEndSer.Text   = r.EndSer ?? "";
            nudQty.Value     = r.Qty.HasValue ? (decimal)r.Qty.Value : 0;
            txtVintage.Text  = r.Vintage ?? "";
            txtPONumber.Text = r.PONumber ?? "";
            txtUnitCost.Text = r.UnitCost.HasValue ? r.UnitCost.Value.ToString(CurrencyFormat) : "";
            txtCID.Text      = r.CID ?? "";
            txtMENumber.Text = r.MENumber ?? "";
            txtPurCode.Text  = r.PurCode ?? "";
            txtEst.Text      = r.Est ?? "";
            txtComments.Text = r.Comments ?? "";

            // PO Date: if value exists, show it; else leave blank
            txtPODate.Text = r.PODate.HasValue ? r.PODate.Value.ToString("MM/dd/yyyy") : "";

            // Recv Date: checkbox only; date is always today when checked
            chkRecvDate.Checked = r.RecvDate.HasValue;

            // Audit info
            string auditText = "";
            if (!string.IsNullOrEmpty(r.CreatedBy))
                auditText += $"Created by {r.CreatedBy} on {r.CreatedDate?.ToString("MM/dd/yyyy") ?? "unknown"}";
            if (!string.IsNullOrEmpty(r.ModifiedBy))
            {
                if (auditText.Length > 0) auditText += "   |   ";
                auditText += $"Last modified by {r.ModifiedBy} on {r.ModifiedDate?.ToString("MM/dd/yyyy") ?? "unknown"}";
            }
            if (!string.IsNullOrEmpty(auditText))
            {
                lblAuditInfo.Text = auditText;
                lblAuditInfo.Visible = true;
            }

            // Disable auto-calc for existing records (manual mode)
            chkAutoQty.Checked = false;
            _suppressQtyAutoCalc = false;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            decimal? unitCost = null;
            if (!string.IsNullOrWhiteSpace(txtUnitCost.Text))
            {
                string costText = StripCurrencyFormatting(txtUnitCost.Text);
                if (!decimal.TryParse(costText, out decimal cost))
                {
                    MessageBox.Show("Unit Cost must be a valid number.", "Validation",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtUnitCost.Focus();
                    return;
                }
                unitCost = cost;
            }

            DateTime? poDate = null;
            if (!string.IsNullOrWhiteSpace(txtPODate.Text))
            {
                if (!DateTime.TryParse(txtPODate.Text, out DateTime parsedDate))
                {
                    MessageBox.Show("PO Date must be a valid date (e.g. MM/dd/yyyy).", "Validation",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPODate.Focus();
                    return;
                }
                poDate = parsedDate.Date;
            }

            Result = new RecordData
            {
                OpCo2    = NullIfEmpty(txtOpCo2.Text),
                Status   = NullIfEmpty(txtStatus.Text),
                MFR      = NullIfEmpty(txtMFR.Text),
                DevCode  = NullIfEmpty(txtDevCode.Text),
                BegSer   = NullIfEmpty(txtBegSer.Text),
                EndSer   = NullIfEmpty(txtEndSer.Text),
                Qty      = (int)nudQty.Value == 0 ? (int?)null : (int)nudQty.Value,
                PODate   = poDate,
                Vintage  = NullIfEmpty(txtVintage.Text),
                PONumber = NullIfEmpty(txtPONumber.Text),
                RecvDate = chkRecvDate.Checked ? DateTime.Today : (DateTime?)null,
                UnitCost = unitCost,
                CID      = NullIfEmpty(txtCID.Text),
                MENumber = NullIfEmpty(txtMENumber.Text),
                PurCode  = NullIfEmpty(txtPurCode.Text),
                Est      = NullIfEmpty(txtEst.Text),
                Comments = NullIfEmpty(txtComments.Text),
            };

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void chkRecvDate_CheckedChanged(object sender, EventArgs e)
        {
            if (chkRecvDate.Checked)
            {
                lblRecvDateDisplay.Text = DateTime.Today.ToShortDateString();
                lblRecvDateDisplay.Visible = true;
            }
            else
            {
                lblRecvDateDisplay.Text = "";
                lblRecvDateDisplay.Visible = false;
            }
        }

        private void txtUnitCost_Enter(object sender, EventArgs e)
        {
            txtUnitCost.Text = StripCurrencyFormatting(txtUnitCost.Text);
        }

        private void txtUnitCost_Leave(object sender, EventArgs e)
        {
            string raw = StripCurrencyFormatting(txtUnitCost.Text);
            if (!string.IsNullOrEmpty(raw) && decimal.TryParse(raw, out decimal val))
                txtUnitCost.Text = val.ToString(CurrencyFormat);
        }

        private static string StripCurrencyFormatting(string text) =>
            text.Replace("$", "").Replace(",", "").Trim();

        private void SerialField_TextChanged(object sender, EventArgs e)
        {
            if (_suppressQtyAutoCalc) return;
            if (chkAutoQty.Checked)
                TryAutoCalcQty();
        }

        private void TryAutoCalcQty()
        {
            if (long.TryParse(txtBegSer.Text.Trim(), out long beg) &&
                long.TryParse(txtEndSer.Text.Trim(), out long end))
            {
                long qty = end - beg + 1;
                if (qty > 0 && qty <= 999999)
                {
                    _suppressQtyAutoCalc = true;
                    nudQty.Value = qty;
                    _suppressQtyAutoCalc = false;
                }
            }
        }

        private void nudQty_ValueChanged(object sender, EventArgs e)
        {
            // If user manually changed qty while not in auto-calc, uncheck auto
            if (!_suppressQtyAutoCalc)
                chkAutoQty.Checked = false;
        }

        private void chkAutoQty_CheckedChanged(object sender, EventArgs e)
        {
            if (chkAutoQty.Checked)
                TryAutoCalcQty();
        }

        private static string? NullIfEmpty(string? s) =>
            string.IsNullOrWhiteSpace(s) ? null : s.Trim();
    }
}

