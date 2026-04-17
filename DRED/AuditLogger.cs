using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;

namespace DRED
{
    public static class AuditLogger
    {
        public static void LogAuditEntry(
            string tableName, int recordId, string action, string? fieldName, string? oldValue, string? newValue)
        {
            try
            {
                using var conn = DatabaseHelper.OpenConnection();
                LogAuditEntry(conn, tableName, recordId, action, fieldName, oldValue, newValue);
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to write audit log entry.", ex);
            }
        }

        public static DataTable GetAuditLog(int? recordId = null, string? tableName = null, int maxRows = 100)
        {
            using var conn = DatabaseHelper.OpenConnection();
            var whereParts = new List<string>();
            using var cmd = new OleDbCommand();
            cmd.Connection = conn;

            if (recordId.HasValue)
            {
                whereParts.Add("[RecordId] = ?");
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = recordId.Value });
            }

            if (!string.IsNullOrWhiteSpace(tableName))
            {
                whereParts.Add("[TableName] LIKE '%' & ? & '%'");
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarWChar, Size = 255, Value = tableName.Trim() });
            }

            maxRows = Math.Clamp(maxRows, 1, 1000);
            cmd.CommandText = "SELECT TOP " + maxRows + " [Timestamp],[UserName],[TableName],[RecordId],[Action],[FieldName],[OldValue],[NewValue] FROM [AuditLog]";
            if (whereParts.Count > 0)
                cmd.CommandText += " WHERE " + string.Join(" AND ", whereParts);
            cmd.CommandText += " ORDER BY [Timestamp] DESC, [Id] DESC";

            using var adapter = new OleDbDataAdapter(cmd);
            var dt = new DataTable();
            adapter.Fill(dt);
            return dt;
        }

        private static void LogAuditEntry(
            OleDbConnection conn, string tableName, int recordId, string action, string? fieldName, string? oldValue, string? newValue)
        {
            try
            {
                using var cmd = new OleDbCommand(@"
INSERT INTO [AuditLog] ([TableName],[RecordId],[Action],[FieldName],[OldValue],[NewValue],[UserName],[Timestamp])
VALUES (?,?,?,?,?,?,?,?)", conn);
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarWChar, Size = 255, Value = tableName });
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = recordId });
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarWChar, Size = 50, Value = action });
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarWChar, Size = 255, Value = (object?)fieldName ?? DBNull.Value });
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.LongVarWChar, Value = (object?)oldValue ?? DBNull.Value });
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.LongVarWChar, Value = (object?)newValue ?? DBNull.Value });
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarWChar, Size = 255, Value = Environment.UserName });
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Date, Value = DateTime.Now });
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to write audit log entry for [{tableName}] record [{recordId}].", ex);
            }
        }
    }
}
