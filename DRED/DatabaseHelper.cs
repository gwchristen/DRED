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
    [Est] TEXT(255),
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
                                       "PONumber", "Vintage", "CID", "MENumber", "PurCode", "Est", "Comments" };
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
                if (adv.Est != null)      { whereParts.Add("[Est] LIKE '%' & ? & '%'");      paramValues.Add(adv.Est); }
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
                cmd.Parameters.AddWithValue($"@p{i}", paramValues[i] ?? DBNull.Value);

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
     [PurCode],[Est],[Comments],[CreatedBy],[CreatedDate])
VALUES
    (?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)";

            using var cmd = new OleDbCommand(sql, conn);
            AddParameters(cmd, data);
            cmd.Parameters.AddWithValue("@CreatedBy", Environment.UserName);
            cmd.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
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
    [MENumber]=?, [PurCode]=?, [Est]=?, [Comments]=?,
    [ModifiedBy]=?, [ModifiedDate]=?
WHERE [Id]=?";

            using var cmd = new OleDbCommand(sql, conn);
            AddParameters(cmd, data);
            cmd.Parameters.AddWithValue("@ModifiedBy", Environment.UserName);
            cmd.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.ExecuteNonQuery();
        }

        public static void DeleteRecord(string tableName, int id)
        {
            using var conn = OpenConnection();
            string sql = $"DELETE FROM [{tableName}] WHERE [Id]=?";
            using var cmd = new OleDbCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.ExecuteNonQuery();
        }

        private static void AddParameters(OleDbCommand cmd, RecordData data)
        {
            cmd.Parameters.AddWithValue("@OpCo2",    (object?)data.OpCo2    ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Status",   (object?)data.Status   ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MFR",      (object?)data.MFR      ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@DevCode",  (object?)data.DevCode  ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@BegSer",   (object?)data.BegSer   ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@EndSer",   (object?)data.EndSer   ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Qty",      (object?)data.Qty      ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PODate",   (object?)data.PODate   ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Vintage",  (object?)data.Vintage  ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PONumber", (object?)data.PONumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@RecvDate", (object?)data.RecvDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@UnitCost", (object?)data.UnitCost ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CID",      (object?)data.CID      ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MENumber", (object?)data.MENumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PurCode",  (object?)data.PurCode  ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Est",      (object?)data.Est      ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Comments", (object?)data.Comments ?? DBNull.Value);
        }

        public static bool TryLockRecord(string tableName, int recordId, out string lockedBy)
        {
            using var conn = OpenConnection();

            // Clean stale locks (>30 min)
            using (var cleanCmd = new OleDbCommand("DELETE FROM [RecordLocks] WHERE [LockedAt] < ?", conn))
            {
                cleanCmd.Parameters.AddWithValue("@cutoff", DateTime.Now.AddMinutes(-RecordLockTimeoutMinutes));
                cleanCmd.ExecuteNonQuery();
            }

            // Check for existing lock
            string? existing;
            using (var checkCmd = new OleDbCommand(
                "SELECT [LockedBy] FROM [RecordLocks] WHERE [TableName]=? AND [RecordId]=?", conn))
            {
                checkCmd.Parameters.AddWithValue("@t", tableName);
                checkCmd.Parameters.AddWithValue("@id", recordId);
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
                removeCmd.Parameters.AddWithValue("@t", tableName);
                removeCmd.Parameters.AddWithValue("@id", recordId);
                removeCmd.ExecuteNonQuery();
            }

            // Insert new lock
            using (var insertCmd = new OleDbCommand(
                "INSERT INTO [RecordLocks] ([TableName],[RecordId],[LockedBy],[LockedAt]) VALUES (?,?,?,?)", conn))
            {
                insertCmd.Parameters.AddWithValue("@t", tableName);
                insertCmd.Parameters.AddWithValue("@id", recordId);
                insertCmd.Parameters.AddWithValue("@by", Environment.UserName);
                insertCmd.Parameters.AddWithValue("@at", DateTime.Now);
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
                cmd.Parameters.AddWithValue("@t", tableName);
                cmd.Parameters.AddWithValue("@id", recordId);
                cmd.Parameters.AddWithValue("@by", Environment.UserName);
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
        public string? Est { get; set; }
        public string? Comments { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}

