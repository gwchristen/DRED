using System;
using System.Data.OleDb;
using System.Windows.Forms;

namespace DRED
{
    /// <summary>
    /// Provides common UI error handling for database-related failures.
    /// </summary>
    public static class ErrorHelper
    {
        private const int OleDbErrorFileInUse = -2147217887;
        private const int OleDbErrorRecordLocked = -2147217843;

        /// <summary>
        /// Logs an exception and displays a user-friendly database error message.
        /// </summary>
        /// <param name="ex">The exception to report.</param>
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
