using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Windows.Forms;

namespace DRED
{
    /// <summary>
    /// Handles all database operations: connection management, table creation, and CRUD.
    /// Uses OleDb for Microsoft Access (.accdb) connectivity.
    /// </summary>
    public static class DatabaseHelper
    {
        public static readonly string[] TableNames = { "OH_Meters", "IM_Meters", "OH_Transformers", "IM_Transformers" };

        public static string GetConnectionString()
        {
            return $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={AppSettings.DatabasePath};Mode=Share Deny None;";
        }

        public static OleDbConnection OpenConnection()
        {
            try
            {
                var conn = new OleDbConnection(GetConnectionString());
                conn.Open();
                return conn;
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to open database connection.", ex);
                throw;
            }
        }

        /// <summary>
        /// Creates the .accdb database file and all tables if they do not exist.
        /// </summary>
        public static void EnsureDatabaseExists()
        {
            string path = AppSettings.DatabasePath;
            if (!System.IO.File.Exists(path))
            {
                SchemaManager.CreateDatabase(path);
            }

            using var conn = OpenConnection();
            foreach (string table in TableNames)
            {
                SchemaManager.EnsureTableExists(conn, table);
            }
            SchemaManager.EnsureRecordLocksTable(conn);
            SchemaManager.EnsureAuditLogTable(conn);
        }

        public static DataTable GetTableData(string tableName, string filter = "",
            string filterColumn = "", AdvancedSearchCriteria? advancedCriteria = null)
        {
            using var conn = OpenConnection();

            var whereParts = new List<string>();
            var paramValues = new List<object>();

            // Simple text filter
            if (!string.IsNullOrWhiteSpace(filter))
            {
                if (!string.IsNullOrEmpty(filterColumn) && filterColumn != "All Columns")
                {
                    whereParts.Add($"[{filterColumn}] LIKE '%' & ? & '%'");
                    paramValues.Add(filter);
                }
                else
                {
                    var cols = new[] { "OpCo2", "Status", "MFR", "DevCode", "BegSer", "EndSer",
                                       "PONumber", "Vintage", "CID", "MENumber", "PurCode", "Comments" };
                    var orParts = new List<string>();
                    foreach (var col in cols)
                    {
                        orParts.Add($"[{col}] LIKE '%' & ? & '%'");
                        paramValues.Add(filter);
                    }
                    whereParts.Add("(" + string.Join(" OR ", orParts) + ")");
                }
            }

            // Advanced criteria
            if (advancedCriteria != null)
            {
                var adv = advancedCriteria;
                if (adv.OpCo2 != null)    { whereParts.Add("[OpCo2] LIKE '%' & ? & '%'");    paramValues.Add(adv.OpCo2); }
                if (adv.Status != null)   { whereParts.Add("[Status] LIKE '%' & ? & '%'");   paramValues.Add(adv.Status); }
                if (adv.MFR != null)      { whereParts.Add("[MFR] LIKE '%' & ? & '%'");      paramValues.Add(adv.MFR); }
                if (adv.DevCode != null)  { whereParts.Add("[DevCode] LIKE '%' & ? & '%'");  paramValues.Add(adv.DevCode); }
                if (adv.BegSer != null)   { whereParts.Add("[BegSer] LIKE '%' & ? & '%'");   paramValues.Add(adv.BegSer); }
                if (adv.EndSer != null)   { whereParts.Add("[EndSer] LIKE '%' & ? & '%'");   paramValues.Add(adv.EndSer); }
                if (adv.PONumber != null) { whereParts.Add("[PONumber] LIKE '%' & ? & '%'"); paramValues.Add(adv.PONumber); }
                if (adv.Vintage != null)  { whereParts.Add("[Vintage] LIKE '%' & ? & '%'");  paramValues.Add(adv.Vintage); }
                if (adv.CID != null)      { whereParts.Add("[CID] LIKE '%' & ? & '%'");      paramValues.Add(adv.CID); }
                if (adv.MENumber != null) { whereParts.Add("[MENumber] LIKE '%' & ? & '%'"); paramValues.Add(adv.MENumber); }
                if (adv.PurCode != null)  { whereParts.Add("[PurCode] LIKE '%' & ? & '%'");  paramValues.Add(adv.PurCode); }
                if (adv.Est.HasValue)      { whereParts.Add("[Est] = ?");      paramValues.Add(adv.Est.Value); }
                if (adv.TextFile.HasValue) { whereParts.Add("[TextFile] = ?"); paramValues.Add(adv.TextFile.Value); }
                if (adv.Comments != null) { whereParts.Add("[Comments] LIKE '%' & ? & '%'"); paramValues.Add(adv.Comments); }
                if (adv.PODateFrom.HasValue)   { whereParts.Add("[PODate] >= ?");   paramValues.Add(adv.PODateFrom.Value); }
                if (adv.PODateTo.HasValue)     { whereParts.Add("[PODate] <= ?");   paramValues.Add(adv.PODateTo.Value); }
                if (adv.RecvDateFrom.HasValue) { whereParts.Add("[RecvDate] >= ?"); paramValues.Add(adv.RecvDateFrom.Value); }
                if (adv.RecvDateTo.HasValue)   { whereParts.Add("[RecvDate] <= ?"); paramValues.Add(adv.RecvDateTo.Value); }
                if (adv.CostMin.HasValue) { whereParts.Add("[UnitCost] >= ?"); paramValues.Add(adv.CostMin.Value); }
                if (adv.CostMax.HasValue) { whereParts.Add("[UnitCost] <= ?"); paramValues.Add(adv.CostMax.Value); }
                if (adv.QtyMin.HasValue)  { whereParts.Add("[Qty] >= ?");      paramValues.Add(adv.QtyMin.Value); }
                if (adv.QtyMax.HasValue)  { whereParts.Add("[Qty] <= ?");      paramValues.Add(adv.QtyMax.Value); }
            }

            string sql = $"SELECT * FROM [{tableName}]";
            if (whereParts.Count > 0)
                sql += " WHERE " + string.Join(" AND ", whereParts);
            sql += " ORDER BY [Id] DESC";

            using var cmd = new OleDbCommand(sql, conn);
            for (int i = 0; i < paramValues.Count; i++)
            {
                object val = paramValues[i] ?? DBNull.Value;
                OleDbParameter p;
                if (val is DateTime dtVal)
                    p = new OleDbParameter { OleDbType = OleDbType.Date, Value = dtVal };
                else if (val is decimal dec)
                    p = new OleDbParameter { OleDbType = OleDbType.Currency, Value = dec };
                else if (val is int iv)
                    p = new OleDbParameter { OleDbType = OleDbType.Integer, Value = iv };
                else if (val is bool bv)
                    p = new OleDbParameter { OleDbType = OleDbType.Boolean, Value = bv };
                else
                    // Text filter values and LIKE pattern strings are always VarWChar
                    p = new OleDbParameter { OleDbType = OleDbType.VarWChar, Size = 255, Value = val };
                cmd.Parameters.Add(p);
            }

            using var adapter = new OleDbDataAdapter(cmd);
            var dt = new DataTable();
            adapter.Fill(dt);
            return dt;
        }

        public static AutoCompleteStringCollection GetDistinctValues(string tableName, string columnName)
        {
            EnsureValidTableName(tableName);
            EnsureValidColumnName(columnName);

            var values = new AutoCompleteStringCollection();

            using var conn = OpenConnection();
            string sql = $@"
SELECT DISTINCT [{columnName}]
FROM [{tableName}]
WHERE [{columnName}] IS NOT NULL AND [{columnName}] <> ''
ORDER BY [{columnName}]";

            using var cmd = new OleDbCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string? value = reader[0] as string;
                if (!string.IsNullOrWhiteSpace(value))
                    values.Add(value);
            }

            return values;
        }

        public static bool RecordExists(string tableName, string devCode, string begSer, string endSer, int? excludeId = null)
        {
            EnsureValidTableName(tableName);

            using var conn = OpenConnection();
            string trimmedEndSer = endSer?.Trim() ?? string.Empty;

            string sql;
            using var cmd = new OleDbCommand();
            cmd.Connection = conn;

            if (string.IsNullOrEmpty(trimmedEndSer))
            {
                sql = $@"
SELECT COUNT(*)
FROM [{tableName}]
WHERE [DevCode] = ? AND [BegSer] = ? AND ([EndSer] IS NULL OR [EndSer] = '')";
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarWChar, Size = 255, Value = devCode.Trim() });
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarWChar, Size = 255, Value = begSer.Trim() });
            }
            else
            {
                sql = $@"
SELECT COUNT(*)
FROM [{tableName}]
WHERE [DevCode] = ? AND [BegSer] = ? AND [EndSer] = ?";
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarWChar, Size = 255, Value = devCode.Trim() });
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarWChar, Size = 255, Value = begSer.Trim() });
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarWChar, Size = 255, Value = trimmedEndSer });
            }

            if (excludeId.HasValue)
            {
                sql += " AND [Id] <> ?";
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = excludeId.Value });
            }

            cmd.CommandText = sql;
            object? result = cmd.ExecuteScalar();
            return Convert.ToInt32(result) > 0;
        }

        public static void InsertRecord(string tableName, RecordData data)
        {
            try
            {
                using var conn = OpenConnection();
                string sql = $@"
INSERT INTO [{tableName}]
    ([OpCo2],[Status],[MFR],[DevCode],[BegSer],[EndSer],[Qty],
     [PODate],[Vintage],[PONumber],[RecvDate],[UnitCost],[CID],[MENumber],
     [PurCode],[Est],[TextFile],[Comments],[OOSSerials],[CreatedBy],[CreatedDate])
VALUES
    (?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)";

                using var cmd = new OleDbCommand(sql, conn);
                AddParameters(cmd, data);
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarWChar, Size = 255, Value = Environment.UserName });
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Date, Value = DateTime.Now });
                cmd.ExecuteNonQuery();
                using var idCmd = new OleDbCommand("SELECT @@IDENTITY", conn);
                int recordId = Convert.ToInt32(idCmd.ExecuteScalar());
                string summary = BuildRecordSummary(data);
                AuditLogger.LogAuditEntry(tableName, recordId, "INSERT", null, null, summary);
                Logger.Log($"Inserted record into [{tableName}].");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to insert record into [{tableName}].", ex);
                throw;
            }
        }

        public static void UpdateRecord(string tableName, int id, RecordData data)
        {
            try
            {
                using var conn = OpenConnection();
                var oldData = GetRecordDataById(conn, tableName, id);
                string sql = $@"
UPDATE [{tableName}] SET
    [OpCo2]=?, [Status]=?, [MFR]=?, [DevCode]=?,
    [BegSer]=?, [EndSer]=?, [Qty]=?,
    [PODate]=?, [Vintage]=?, [PONumber]=?,
    [RecvDate]=?, [UnitCost]=?, [CID]=?,
    [MENumber]=?, [PurCode]=?, [Est]=?, [TextFile]=?, [Comments]=?, [OOSSerials]=?,
    [ModifiedBy]=?, [ModifiedDate]=?
WHERE [Id]=?";

                using var cmd = new OleDbCommand(sql, conn);
                AddParameters(cmd, data);
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarWChar, Size = 255, Value = Environment.UserName });
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Date, Value = DateTime.Now });
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = id });
                cmd.ExecuteNonQuery();

                if (oldData != null)
                {
                    foreach (var change in GetChangedFields(oldData, data))
                    {
                        AuditLogger.LogAuditEntry(tableName, id, "UPDATE", change.FieldName, change.OldValue, change.NewValue);
                    }
                }
                Logger.Log($"Updated record [{id}] in [{tableName}].");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to update record [{id}] in [{tableName}].", ex);
                throw;
            }
        }

        public static void DeleteRecord(string tableName, int id)
        {
            try
            {
                using var conn = OpenConnection();
                var oldData = GetRecordDataById(conn, tableName, id);
                string sql = $"DELETE FROM [{tableName}] WHERE [Id]=?";
                using var cmd = new OleDbCommand(sql, conn);
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = id });
                cmd.ExecuteNonQuery();
                string summary = oldData != null ? BuildRecordSummary(oldData) : "(record data unavailable)";
                AuditLogger.LogAuditEntry(tableName, id, "DELETE", null, summary, null);
                Logger.Log($"Deleted record [{id}] from [{tableName}].");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to delete record [{id}] from [{tableName}].", ex);
                throw;
            }
        }

        private static RecordData? GetRecordDataById(OleDbConnection conn, string tableName, int id)
        {
            using var cmd = new OleDbCommand($"SELECT * FROM [{tableName}] WHERE [Id]=?", conn);
            cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = id });
            using var reader = cmd.ExecuteReader();
            if (reader == null || !reader.Read()) return null;

            return new RecordData
            {
                OpCo2 = GetNullableString(reader, "OpCo2"),
                Status = GetNullableString(reader, "Status"),
                MFR = GetNullableString(reader, "MFR"),
                DevCode = GetNullableString(reader, "DevCode"),
                BegSer = GetNullableString(reader, "BegSer"),
                EndSer = GetNullableString(reader, "EndSer"),
                Qty = GetNullableInt(reader, "Qty"),
                PODate = GetNullableDate(reader, "PODate"),
                Vintage = GetNullableString(reader, "Vintage"),
                PONumber = GetNullableString(reader, "PONumber"),
                RecvDate = GetNullableDate(reader, "RecvDate"),
                UnitCost = GetNullableDecimal(reader, "UnitCost"),
                CID = GetNullableString(reader, "CID"),
                MENumber = GetNullableString(reader, "MENumber"),
                PurCode = GetNullableString(reader, "PurCode"),
                Est = GetBool(reader, "Est"),
                TextFile = GetBool(reader, "TextFile"),
                Comments = GetNullableString(reader, "Comments"),
                OOSSerials = GetNullableString(reader, "OOSSerials"),
            };
        }

        private static List<(string FieldName, string? OldValue, string? NewValue)> GetChangedFields(RecordData oldData, RecordData newData)
        {
            var oldMap = GetAuditableFieldMap(oldData);
            var newMap = GetAuditableFieldMap(newData);
            var changes = new List<(string FieldName, string? OldValue, string? NewValue)>();
            foreach (var kvp in oldMap)
            {
                string field = kvp.Key;
                string? oldValue = kvp.Value;
                string? newValue = newMap[field];
                if (!string.Equals(oldValue, newValue, StringComparison.Ordinal))
                    changes.Add((field, oldValue, newValue));
            }

            return changes;
        }

        private static Dictionary<string, string?> GetAuditableFieldMap(RecordData data)
        {
            return new Dictionary<string, string?>
            {
                ["OpCo2"] = data.OpCo2,
                ["Status"] = data.Status,
                ["MFR"] = data.MFR,
                ["DevCode"] = data.DevCode,
                ["BegSer"] = data.BegSer,
                ["EndSer"] = data.EndSer,
                ["Qty"] = data.Qty?.ToString(),
                ["PODate"] = FormatAuditDate(data.PODate),
                ["Vintage"] = data.Vintage,
                ["PONumber"] = data.PONumber,
                ["RecvDate"] = FormatAuditDate(data.RecvDate),
                ["UnitCost"] = data.UnitCost?.ToString("0.00"),
                ["CID"] = data.CID,
                ["MENumber"] = data.MENumber,
                ["PurCode"] = data.PurCode,
                ["Est"] = data.Est ? "True" : "False",
                ["TextFile"] = data.TextFile ? "True" : "False",
                ["Comments"] = data.Comments,
                ["OOSSerials"] = data.OOSSerials,
            };
        }

        private static string BuildRecordSummary(RecordData data)
        {
            string devCode = string.IsNullOrWhiteSpace(data.DevCode) ? "(blank)" : data.DevCode!;
            string poNumber = string.IsNullOrWhiteSpace(data.PONumber) ? "(blank)" : data.PONumber!;
            return $"DevCode={devCode}, PONumber={poNumber}";
        }

        private static string? GetNullableString(OleDbDataReader reader, string column)
        {
            int idx = reader.GetOrdinal(column);
            return reader.IsDBNull(idx) ? null : reader.GetString(idx);
        }

        private static int? GetNullableInt(OleDbDataReader reader, string column)
        {
            int idx = reader.GetOrdinal(column);
            return reader.IsDBNull(idx) ? null : Convert.ToInt32(reader.GetValue(idx));
        }

        private static DateTime? GetNullableDate(OleDbDataReader reader, string column)
        {
            int idx = reader.GetOrdinal(column);
            return reader.IsDBNull(idx) ? null : Convert.ToDateTime(reader.GetValue(idx));
        }

        private static decimal? GetNullableDecimal(OleDbDataReader reader, string column)
        {
            int idx = reader.GetOrdinal(column);
            return reader.IsDBNull(idx) ? null : Convert.ToDecimal(reader.GetValue(idx));
        }

        private static bool GetBool(OleDbDataReader reader, string column)
        {
            int idx = reader.GetOrdinal(column);
            return !reader.IsDBNull(idx) && Convert.ToBoolean(reader.GetValue(idx));
        }

        private static string? FormatAuditDate(DateTime? value)
            => value?.ToString("yyyy-MM-dd HH:mm:ss");

        private static void AddParameters(OleDbCommand cmd, RecordData data)
        {
            AddTextParam(cmd, data.OpCo2);
            AddTextParam(cmd, data.Status);
            AddTextParam(cmd, data.MFR);
            AddTextParam(cmd, data.DevCode);
            AddTextParam(cmd, data.BegSer);
            AddTextParam(cmd, data.EndSer);
            AddIntParam(cmd, data.Qty);
            AddDateParam(cmd, data.PODate);
            AddTextParam(cmd, data.Vintage);
            AddTextParam(cmd, data.PONumber);
            AddDateParam(cmd, data.RecvDate);
            AddCurrencyParam(cmd, data.UnitCost);
            AddTextParam(cmd, data.CID);
            AddTextParam(cmd, data.MENumber);
            AddTextParam(cmd, data.PurCode);
            AddBoolParam(cmd, data.Est);
            AddBoolParam(cmd, data.TextFile);
            AddMemoParam(cmd, data.Comments);
            AddMemoParam(cmd, data.OOSSerials);
        }

        private static void AddBoolParam(OleDbCommand cmd, bool value)
        {
            var p = new OleDbParameter { OleDbType = OleDbType.Boolean, Value = value };
            cmd.Parameters.Add(p);
        }

        private static void AddTextParam(OleDbCommand cmd, string? value)
        {
            var p = new OleDbParameter { OleDbType = OleDbType.VarWChar, Size = 255 };
            p.Value = (object?)value ?? DBNull.Value;
            cmd.Parameters.Add(p);
        }

        private static void AddMemoParam(OleDbCommand cmd, string? value)
        {
            var p = new OleDbParameter { OleDbType = OleDbType.LongVarWChar };
            p.Value = (object?)value ?? DBNull.Value;
            cmd.Parameters.Add(p);
        }

        private static void AddIntParam(OleDbCommand cmd, int? value)
        {
            var p = new OleDbParameter { OleDbType = OleDbType.Integer };
            p.Value = value.HasValue ? (object)value.Value : DBNull.Value;
            cmd.Parameters.Add(p);
        }

        private static void AddDateParam(OleDbCommand cmd, DateTime? value)
        {
            var p = new OleDbParameter { OleDbType = OleDbType.Date };
            p.Value = value.HasValue ? (object)value.Value : DBNull.Value;
            cmd.Parameters.Add(p);
        }

        private static void AddCurrencyParam(OleDbCommand cmd, decimal? value)
        {
            var p = new OleDbParameter { OleDbType = OleDbType.Currency };
            p.Value = value.HasValue ? (object)value.Value : DBNull.Value;
            cmd.Parameters.Add(p);
        }

        private static void EnsureValidTableName(string tableName)
        {
            if (!TableNames.Contains(tableName))
                throw new ArgumentException(
                    $"Invalid table name '{tableName}'. Valid tables: {string.Join(", ", TableNames)}.",
                    nameof(tableName));
        }

        private static void EnsureValidColumnName(string columnName)
        {
            string[] allowedColumns =
            {
                "OpCo2", "Status", "MFR", "DevCode", "BegSer", "EndSer", "Qty",
                "PODate", "Vintage", "PONumber", "RecvDate", "UnitCost", "CID",
                "MENumber", "PurCode", "Est", "TextFile", "Comments", "OOSSerials"
            };

            if (!allowedColumns.Contains(columnName))
                throw new ArgumentException("Invalid column name.", nameof(columnName));
        }

        /// <summary>
        /// Exports a single table to an Excel file using ClosedXML.
        /// </summary>
        public static void ExportToExcel(string tableName, string filePath)
        {
            try
            {
                using var workbook = new ClosedXML.Excel.XLWorkbook();
                var dt = GetTableData(tableName);
                workbook.Worksheets.Add(dt, tableName);
                workbook.SaveAs(filePath);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to export table [{tableName}] to '{filePath}'.", ex);
                throw;
            }
        }

        /// <summary>
        /// Exports all four tables to separate sheets in a single Excel workbook.
        /// </summary>
        public static void ExportAllToExcel(string filePath)
        {
            try
            {
                using var workbook = new ClosedXML.Excel.XLWorkbook();
                var sheetNames = new[] { "OH - Meters", "I&M - Meters", "OH - Transformers", "I&M - Transformers" };
                for (int i = 0; i < TableNames.Length; i++)
                {
                    var dt = GetTableData(TableNames[i]);
                    workbook.Worksheets.Add(dt, sheetNames[i]);
                }
                workbook.SaveAs(filePath);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to export all tables to '{filePath}'.", ex);
                throw;
            }
        }
    }

}
