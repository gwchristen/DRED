using System;
using System.Windows.Forms;

namespace DRED
{
    public partial class RecordForm : Form
    {
        public RecordData? Result { get; private set; }

        private readonly bool _isEdit;

        public RecordForm(RecordData? existing = null)
        {
            InitializeComponent();
            _isEdit = existing != null;
            this.Text = _isEdit ? "Edit Record" : "Add Record";

            if (_isEdit && existing != null)
                PopulateFromRecord(existing);
        }

        private void PopulateFromRecord(RecordData r)
        {
            txtKey.Text       = r.Key ?? "";
            txtOpCo2.Text     = r.OpCo2 ?? "";
            txtStatus.Text    = r.Status ?? "";
            txtMFR.Text       = r.MFR ?? "";
            txtDevCode.Text   = r.DevCode ?? "";
            txtBegSer.Text    = r.BegSer ?? "";
            txtEndSer.Text    = r.EndSer ?? "";
            nudQty.Value      = r.Qty.HasValue ? (decimal)r.Qty.Value : 0;
            if (r.PODate.HasValue)   dtpPODate.Value   = r.PODate.Value;
            txtVintage.Text   = r.Vintage ?? "";
            txtPONumber.Text  = r.PONumber ?? "";
            if (r.RecvDate.HasValue) dtpRecvDate.Value = r.RecvDate.Value;
            txtUnitCost.Text  = r.UnitCost.HasValue ? r.UnitCost.Value.ToString("F2") : "";
            txtCID.Text       = r.CID ?? "";
            txtMENumber.Text  = r.MENumber ?? "";
            txtPurCode.Text   = r.PurCode ?? "";
            txtEst.Text       = r.Est ?? "";
            txtComments.Text  = r.Comments ?? "";

            chkPODate.Checked   = r.PODate.HasValue;
            chkRecvDate.Checked = r.RecvDate.HasValue;
            UpdateDatePickers();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // Validate UnitCost if provided
            decimal? unitCost = null;
            if (!string.IsNullOrWhiteSpace(txtUnitCost.Text))
            {
                string costText = txtUnitCost.Text.Trim().Replace("$", "").Replace(",", "");
                if (!decimal.TryParse(costText, out decimal cost))
                {
                    MessageBox.Show("Unit Cost must be a valid number.", "Validation",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtUnitCost.Focus();
                    return;
                }
                unitCost = cost;
            }

            Result = new RecordData
            {
                Key      = NullIfEmpty(txtKey.Text),
                OpCo2    = NullIfEmpty(txtOpCo2.Text),
                Status   = NullIfEmpty(txtStatus.Text),
                MFR      = NullIfEmpty(txtMFR.Text),
                DevCode  = NullIfEmpty(txtDevCode.Text),
                BegSer   = NullIfEmpty(txtBegSer.Text),
                EndSer   = NullIfEmpty(txtEndSer.Text),
                Qty      = (int)nudQty.Value == 0 ? (int?)null : (int)nudQty.Value,
                PODate   = chkPODate.Checked ? dtpPODate.Value.Date : (DateTime?)null,
                Vintage  = NullIfEmpty(txtVintage.Text),
                PONumber = NullIfEmpty(txtPONumber.Text),
                RecvDate = chkRecvDate.Checked ? dtpRecvDate.Value.Date : (DateTime?)null,
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

        private void chkPODate_CheckedChanged(object sender, EventArgs e) => UpdateDatePickers();
        private void chkRecvDate_CheckedChanged(object sender, EventArgs e) => UpdateDatePickers();

        private void UpdateDatePickers()
        {
            dtpPODate.Enabled   = chkPODate.Checked;
            dtpRecvDate.Enabled = chkRecvDate.Checked;
        }

        private static string? NullIfEmpty(string? s) =>
            string.IsNullOrWhiteSpace(s) ? null : s.Trim();
    }
}
