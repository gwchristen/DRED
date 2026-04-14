using System;
using System.Windows.Forms;

namespace DRED
{
    public partial class PinEntryForm : Form
    {
        public PinEntryForm()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (txtPin.Text == AppSettings.LockPin)
            {
                DialogResult = DialogResult.OK;
                Close();
                return;
            }

            MessageBox.Show(
                "Incorrect PIN. Please try again.",
                "Invalid PIN",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            txtPin.SelectAll();
            txtPin.Focus();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
