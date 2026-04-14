using System;
using System.Security.Cryptography;
using System.Text;
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
            if (IsPinMatch(txtPin.Text, AppSettings.LockPin))
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

        private static bool IsPinMatch(string enteredPin, string configuredPin)
        {
            byte[] enteredBytes = Encoding.UTF8.GetBytes(enteredPin ?? string.Empty);
            byte[] configuredBytes = Encoding.UTF8.GetBytes(configuredPin ?? string.Empty);
            return CryptographicOperations.FixedTimeEquals(enteredBytes, configuredBytes);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
