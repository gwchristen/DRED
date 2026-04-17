using System;
using System.Data.OleDb;

namespace DRED
{
    /// <summary>
    /// Provides record-level locking helpers to prevent concurrent edits across users.
    /// </summary>
    public static class RecordLockManager
    {
        private const int RecordLockTimeoutMinutes = 30;

        /// <summary>
        /// Attempts to lock a record for the current user, removing stale locks older than the timeout window.
        /// </summary>
        /// <param name="tableName">The table that contains the record.</param>
        /// <param name="recordId">The Id of the record to lock.</param>
        /// <param name="lockedBy">
        /// When the method returns, contains the user holding the lock when unsuccessful, or the current user when successful.
        /// </param>
        /// <returns><c>true</c> if the lock is acquired; otherwise, <c>false</c>.</returns>
        public static bool TryLockRecord(string tableName, int recordId, out string lockedBy)
        {
            try
            {
                using var conn = DatabaseHelper.OpenConnection();

                // Clean stale locks (>30 min)
                using (var cleanCmd = new OleDbCommand("DELETE FROM [RecordLocks] WHERE [LockedAt] < ?", conn))
                {
                    cleanCmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Date, Value = DateTime.Now.AddMinutes(-RecordLockTimeoutMinutes) });
                    cleanCmd.ExecuteNonQuery();
                }

                // Check for existing lock
                string? existing;
                using (var checkCmd = new OleDbCommand(
                    "SELECT [LockedBy] FROM [RecordLocks] WHERE [TableName]=? AND [RecordId]=?", conn))
                {
                    checkCmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarWChar, Size = 255, Value = tableName });
                    checkCmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = recordId });
                    existing = checkCmd.ExecuteScalar() as string;
                }

                if (existing != null && !string.Equals(existing, Environment.UserName, StringComparison.OrdinalIgnoreCase))
                {
                    lockedBy = existing;
                    return false;
                }

                // Remove any existing lock by same user
                using (var removeCmd = new OleDbCommand(
                    "DELETE FROM [RecordLocks] WHERE [TableName]=? AND [RecordId]=?", conn))
                {
                    removeCmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarWChar, Size = 255, Value = tableName });
                    removeCmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = recordId });
                    removeCmd.ExecuteNonQuery();
                }

                // Insert new lock
                using (var insertCmd = new OleDbCommand(
                    "INSERT INTO [RecordLocks] ([TableName],[RecordId],[LockedBy],[LockedAt]) VALUES (?,?,?,?)", conn))
                {
                    insertCmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarWChar, Size = 255, Value = tableName });
                    insertCmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = recordId });
                    insertCmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarWChar, Size = 255, Value = Environment.UserName });
                    insertCmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Date, Value = DateTime.Now });
                    insertCmd.ExecuteNonQuery();
                }

                lockedBy = Environment.UserName;
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to lock record [{recordId}] in [{tableName}].", ex);
                throw;
            }
        }

        /// <summary>
        /// Releases the current user's lock for the specified record.
        /// </summary>
        /// <param name="tableName">The table that contains the record.</param>
        /// <param name="recordId">The Id of the record to unlock.</param>
        public static void UnlockRecord(string tableName, int recordId)
        {
            try
            {
                using var conn = DatabaseHelper.OpenConnection();
                using var cmd = new OleDbCommand(
                    "DELETE FROM [RecordLocks] WHERE [TableName]=? AND [RecordId]=? AND [LockedBy]=?", conn);
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarWChar, Size = 255, Value = tableName });
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = recordId });
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarWChar, Size = 255, Value = Environment.UserName });
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to unlock record [{recordId}] in [{tableName}].", ex);
            }
        }
    }
}
