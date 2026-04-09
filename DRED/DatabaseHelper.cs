using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;

namespace DRED
{
    /// <summary>
    /// Handles all database operations: connection management, table creation, and CRUD.
    /// Uses OleDb for Microsoft Access (.accdb) connectivity.
    /// </summary>
    public static class DatabaseHelper
    {
        public static readonly string[] TableNames = { "OH_Meters", "IM_Meters", "OH_Transformers", "IM_Transformers" };

        private const int RecordLockTimeoutMinutes = 30;

        public static string GetConnectionString()
        {
            return $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={AppSettings.DatabasePath};Mode=Share Deny None;";
        }

        public static OleDbConnection OpenConnection()
        {
            var conn = new OleDbConnection(GetConnectionString());
            conn.Open();
            return conn;
        }

        /// <summary>
        /// Creates the .accdb database file and all tables if they do not exist.
        /// </summary>
        public static void EnsureDatabaseExists()
        {
            string path = AppSettings.DatabasePath;
            if (!System.IO.File.Exists(path))
            {
                CreateDatabase(path);
            }

            using var conn = OpenConnection();
            foreach (string table in TableNames)
            {
                EnsureTableExists(conn, table);
            }
            EnsureRecordLocksTable(conn);
        }

        private static void CreateDatabase(string path)
        {
            try
            {
                Type? catalogType = Type.GetTypeFromProgID("ADOX.Catalog");
                if (catalogType == null)
                    throw new InvalidOperationException("ADOX.Catalog COM class not found.");

                dynamic catalog = Activator.CreateInstance(catalogType)!;
                catalog.Create($"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={path};");
                System.Runtime.InteropServices.Marshal.ReleaseComObject(catalog);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Could not create the Access database file at '{path}'.\n\n" +
                    "Please ensure the Microsoft Access Database Engine 2016 Redistributable is installed.\n\n" +
                    $"Details: {ex.Message}");
            }
        }

        private static void EnsureTableExists(OleDbConnection conn, string tableName)
        {
            DataTable schema = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables,
                new object?[] { null, null, tableName, "TABLE" })!;

            if (schema.Rows.Count == 0)
            {
                string sql = $@"
CREATE TABLE [{tableName}] (
    [Id] AUTOINCREMENT PRIMARY KEY,
    [OpCo2] TEXT(255),
    [Status] TEXT(255),
    [MFR] TEXT(255),
    [DevCode] TEXT(255),
    [BegSer] TEXT(255),
    [EndSer] TEXT(255),
    [Qty] INTEGER,
    [PODate] DATETIME,
    [Vintage] TEXT(255),
    [PONumber] TEXT(255),
    [RecvDate] DATETIME,
    [UnitCost] CURRENCY,
    [CID] TEXT(255),
    [MENumber] TEXT(255),
    [PurCode] TEXT(255),
    [Est] YESNO,
    [TextFile] YESNO,
    [Comments] MEMO
)";
                using var cmd = new OleDbCommand(sql, conn);
                cmd.ExecuteNonQuery();
            }

            // Drop legacy Key column if it exists
            try
            {
                using var dropCmd = new OleDbCommand($"ALTER TABLE [{tableName}] DROP COLUMN [Key]", conn);
                dropCmd.ExecuteNonQuery();
            }
            catch { /* Column may not exist — expected */ }

            EnsureAuditColumns(conn, tableName);
            MigrateEstToBoolean(conn, tableName);
            EnsureTextFileColumn(conn, tableName);
        }

        /// <summary>
        /// Migrates the [Est] column from TEXT(255) to YESNO for existing databases.
        /// If it's already YESNO, this method is a no-op.
        /// </summary>
        private static void MigrateEstToBoolean(OleDbConnection conn, string tableName)
        {
            // Check the actual data type of the Est column
            var schemaTable = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Columns,
                new object?[] { null, null, tableName, "Est" })!;

            if (schemaTable.Rows.Count == 0) return; // Column doesn't exist — CREATE TABLE handles it

            int dataType = Convert.ToInt32(schemaTable.Rows[0]["DATA_TYPE"]);
            if (dataType == 11) return; // 11 = Boolean (YESNO) — already migrated

            // It's TEXT — perform migration via a temp column
            try
            {
                using var a1 = new OleDbCommand($"ALTER TABLE [{tableName}] ADD COLUMN [Est_New] YESNO", conn);
                a1.ExecuteNonQuery();
            }
            catch { return; } // If we can't add the temp column, abort

            // Treat any non-empty text value as True (the original text field stored "1" or similar
            // to indicate established, and null/empty to indicate not established).
            try
            {
                using var a2 = new OleDbCommand(
                    $"UPDATE [{tableName}] SET [Est_New] = IIF([Est] IS NOT NULL AND [Est] <> '', True, False)", conn);
                a2.ExecuteNonQuery();
            }
            catch { /* Best effort */ }

            try
            {
                using var a3 = new OleDbCommand($"ALTER TABLE [{tableName}] DROP COLUMN [Est]", conn);
                a3.ExecuteNonQuery();
            }
            catch { }

            try
            {
                using var a4 = new OleDbCommand($"ALTER TABLE [{tableName}] ADD COLUMN [Est] YESNO", conn);
                a4.ExecuteNonQuery();
            }
            catch { }

            try
            {
                using var a5 = new OleDbCommand($"UPDATE [{tableName}] SET [Est] = [Est_New]", conn);
                a5.ExecuteNonQuery();
            }
            catch { }

            try
            {
                using var a6 = new OleDbCommand($"ALTER TABLE [{tableName}] DROP COLUMN [Est_New]", conn);
                a6.ExecuteNonQuery();
            }
            catch { }
        }

        /// <summary>
        /// Adds the [TextFile] YESNO column if it does not already exist.
        /// </summary>
        private static void EnsureTextFileColumn(OleDbConnection conn, string tableName)
        {
            try
            {
                using var cmd = new OleDbCommand($"ALTER TABLE [{tableName}] ADD COLUMN [TextFile] YESNO", conn);
                cmd.ExecuteNonQuery();
            }
            catch { /* Column likely already exists */ }
        }

        private static void EnsureAuditColumns(OleDbConnection conn, string tableName)
        {
            var auditCols = new[]
            {
                ("CreatedBy",    "TEXT(255)"),
                ("CreatedDate",  "DATETIME"),
                ("ModifiedBy",   "TEXT(255)"),
                ("ModifiedDate", "DATETIME"),
            };
            foreach (var (col, type) in auditCols)
            {
                try
                {
                    using var cmd = new OleDbCommand($"ALTER TABLE [{tableName}] ADD COLUMN [{col}] {type}", conn);
                    cmd.ExecuteNonQuery();
                }
                catch { /* Column likely already exists */ }
            }
        }

        private static void EnsureRecordLocksTable(OleDbConnection conn)
        {
            DataTable schema = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables,
                new object?[] { null, null, "RecordLocks", "TABLE" })!;

            if (schema.Rows.Count > 0) return;

            string sql = @"
CREATE TABLE [RecordLocks] (
    [Id] AUTOINCREMENT PRIMARY KEY,
    [TableName] TEXT(255),
    [RecordId] INTEGER,
    [LockedBy] TEXT(255),
    [LockedAt] DATETIME
)";
            using var cmd = new OleDbCommand(sql, conn);
            cmd.ExecuteNonQuery();
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
            sql += " ORDER BY [Id]";

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

        public static void InsertRecord(string tableName, RecordData data)
        {
            using var conn = OpenConnection();
            string sql = $@"
INSERT INTO [{tableName}]
    ([OpCo2],[Status],[MFR],[DevCode],[BegSer],[EndSer],[Qty],
     [PODate],[Vintage],[PONumber],[RecvDate],[UnitCost],[CID],[MENumber],
     [PurCode],[Est],[TextFile],[Comments],[CreatedBy],[CreatedDate])
VALUES
    (?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)";

            using var cmd = new OleDbCommand(sql, conn);
            AddParameters(cmd, data);
            cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarWChar, Size = 255, Value = Environment.UserName });
            cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Date, Value = DateTime.Now });
            cmd.ExecuteNonQuery();
        }

        public static void UpdateRecord(string tableName, int id, RecordData data)
        {
            using var conn = OpenConnection();
            string sql = $@"
UPDATE [{tableName}] SET
    [OpCo2]=?, [Status]=?, [MFR]=?, [DevCode]=?,
    [BegSer]=?, [EndSer]=?, [Qty]=?,
    [PODate]=?, [Vintage]=?, [PONumber]=?,
    [RecvDate]=?, [UnitCost]=?, [CID]=?,
    [MENumber]=?, [PurCode]=?, [Est]=?, [TextFile]=?, [Comments]=?,
    [ModifiedBy]=?, [ModifiedDate]=?
WHERE [Id]=?";

            using var cmd = new OleDbCommand(sql, conn);
            AddParameters(cmd, data);
            cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarWChar, Size = 255, Value = Environment.UserName });
            cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Date, Value = DateTime.Now });
            cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = id });
            cmd.ExecuteNonQuery();
        }

        public static void DeleteRecord(string tableName, int id)
        {
            using var conn = OpenConnection();
            string sql = $"DELETE FROM [{tableName}] WHERE [Id]=?";
            using var cmd = new OleDbCommand(sql, conn);
            cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = id });
            cmd.ExecuteNonQuery();
        }

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

        public static bool TryLockRecord(string tableName, int recordId, out string lockedBy)
        {
            using var conn = OpenConnection();

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

        public static void UnlockRecord(string tableName, int recordId)
        {
            try
            {
                using var conn = OpenConnection();
                using var cmd = new OleDbCommand(
                    "DELETE FROM [RecordLocks] WHERE [TableName]=? AND [RecordId]=? AND [LockedBy]=?", conn);
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarWChar, Size = 255, Value = tableName });
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.Integer, Value = recordId });
                cmd.Parameters.Add(new OleDbParameter { OleDbType = OleDbType.VarWChar, Size = 255, Value = Environment.UserName });
                cmd.ExecuteNonQuery();
            }
            catch { /* Best effort */ }
        }

        /// <summary>
        /// Exports a single table to an Excel file using ClosedXML.
        /// </summary>
        public static void ExportToExcel(string tableName, string filePath)
        {
            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var dt = GetTableData(tableName);
            workbook.Worksheets.Add(dt, tableName);
            workbook.SaveAs(filePath);
        }

        /// <summary>
        /// Exports all four tables to separate sheets in a single Excel workbook.
        /// </summary>
        public static void ExportAllToExcel(string filePath)
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
    }

    /// <summary>
    /// Represents a single record matching the shared schema across all 4 tables.
    /// </summary>
    public class RecordData
    {
        public string? OpCo2 { get; set; }
        public string? Status { get; set; }
        public string? MFR { get; set; }
        public string? DevCode { get; set; }
        public string? BegSer { get; set; }
        public string? EndSer { get; set; }
        public int? Qty { get; set; }
        public DateTime? PODate { get; set; }
        public string? Vintage { get; set; }
        public string? PONumber { get; set; }
        public DateTime? RecvDate { get; set; }
        public decimal? UnitCost { get; set; }
        public string? CID { get; set; }
        public string? MENumber { get; set; }
        public string? PurCode { get; set; }
        public bool Est { get; set; }
        public bool TextFile { get; set; }
        public string? Comments { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}

