using System;
using System.Data.OleDb;
using System.Windows.Forms;

namespace DRED
{
    public static class ErrorHelper
    {
        private const int OleDbErrorFileInUse = -2147217887;
        private const int OleDbErrorRecordLocked = -2147217843;

        public static void ShowDbError(Exception ex)
        {
            Logger.LogError("Database/UI operation failed.", ex);
            string message = ex.Message;
            if (ex is OleDbException oleEx)
            {
                message = oleEx.ErrorCode == OleDbErrorFileInUse || oleEx.ErrorCode == OleDbErrorRecordLocked
                    ? "The database is locked by another user. Please try again.\n\n" + oleEx.Message
                    : oleEx.Message;
            }

            MessageBox.Show(message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
