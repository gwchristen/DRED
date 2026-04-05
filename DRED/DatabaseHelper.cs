using System;
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
        /// Creates the .accdb database file and all 4 tables if they do not exist.
        /// </summary>
        public static void EnsureDatabaseExists()
        {
            string path = AppSettings.DatabasePath;
            if (!System.IO.File.Exists(path))
            {
                CreateDatabase(path);
            }

            // Ensure all tables exist
            using var conn = OpenConnection();
            foreach (string table in TableNames)
            {
                EnsureTableExists(conn, table);
            }
        }

        private static void CreateDatabase(string path)
        {
            // Create a new empty .accdb using ADOX via COM interop
            // Since ADOX requires COM which may not be available on all systems,
            // we use a pre-built minimal Access DB byte pattern approach via DAO/ADOX.
            // Fallback: create via OleDb DDL after creating the file with ADOX catalog.
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
            // Check if table exists
            DataTable schema = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables,
                new object?[] { null, null, tableName, "TABLE" })!;

            if (schema.Rows.Count > 0)
                return; // Table already exists

            string sql = $@"
CREATE TABLE [{tableName}] (
    [Id] AUTOINCREMENT PRIMARY KEY,
    [Key] TEXT(255),
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

        public static DataTable GetTableData(string tableName, string filter = "")
        {
            using var conn = OpenConnection();
            string sql = $"SELECT * FROM [{tableName}]";
            OleDbCommand cmd;

            if (!string.IsNullOrWhiteSpace(filter))
            {
                // Use parameterized LIKE to avoid SQL injection.
                // Access supports: field LIKE '%' & ? & '%'
                const string likeExpr = "LIKE '%' & ? & '%'";
                sql += $@" WHERE 
                    [Key] {likeExpr} OR
                    [OpCo2] {likeExpr} OR
                    [Status] {likeExpr} OR
                    [MFR] {likeExpr} OR
                    [DevCode] {likeExpr} OR
                    [BegSer] {likeExpr} OR
                    [EndSer] {likeExpr} OR
                    [PONumber] {likeExpr} OR
                    [Vintage] {likeExpr} OR
                    [CID] {likeExpr} OR
                    [MENumber] {likeExpr} OR
                    [PurCode] {likeExpr} OR
                    [Est] {likeExpr} OR
                    [Comments] {likeExpr}";
                sql += " ORDER BY [Id]";

                cmd = new OleDbCommand(sql, conn);
                // Add one parameter per LIKE placeholder (14 columns)
                for (int i = 0; i < 14; i++)
                    cmd.Parameters.AddWithValue($"@f{i}", filter);
            }
            else
            {
                sql += " ORDER BY [Id]";
                cmd = new OleDbCommand(sql, conn);
            }

            using (cmd)
            {
                using var adapter = new OleDbDataAdapter(cmd);
                var dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
        }

        public static void InsertRecord(string tableName, RecordData data)
        {
            using var conn = OpenConnection();
            string sql = $@"
INSERT INTO [{tableName}]
    ([Key],[OpCo2],[Status],[MFR],[DevCode],[BegSer],[EndSer],[Qty],
     [PODate],[Vintage],[PONumber],[RecvDate],[UnitCost],[CID],[MENumber],
     [PurCode],[Est],[Comments])
VALUES
    (@Key,@OpCo2,@Status,@MFR,@DevCode,@BegSer,@EndSer,@Qty,
     @PODate,@Vintage,@PONumber,@RecvDate,@UnitCost,@CID,@MENumber,
     @PurCode,@Est,@Comments)";

            using var cmd = new OleDbCommand(sql, conn);
            AddParameters(cmd, data);
            cmd.ExecuteNonQuery();
        }

        public static void UpdateRecord(string tableName, int id, RecordData data)
        {
            using var conn = OpenConnection();
            string sql = $@"
UPDATE [{tableName}] SET
    [Key]=@Key, [OpCo2]=@OpCo2, [Status]=@Status, [MFR]=@MFR,
    [DevCode]=@DevCode, [BegSer]=@BegSer, [EndSer]=@EndSer, [Qty]=@Qty,
    [PODate]=@PODate, [Vintage]=@Vintage, [PONumber]=@PONumber,
    [RecvDate]=@RecvDate, [UnitCost]=@UnitCost, [CID]=@CID,
    [MENumber]=@MENumber, [PurCode]=@PurCode, [Est]=@Est, [Comments]=@Comments
WHERE [Id]=@Id";

            using var cmd = new OleDbCommand(sql, conn);
            AddParameters(cmd, data);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.ExecuteNonQuery();
        }

        public static void DeleteRecord(string tableName, int id)
        {
            using var conn = OpenConnection();
            string sql = $"DELETE FROM [{tableName}] WHERE [Id]=@Id";
            using var cmd = new OleDbCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.ExecuteNonQuery();
        }

        private static void AddParameters(OleDbCommand cmd, RecordData data)
        {
            cmd.Parameters.AddWithValue("@Key", (object?)data.Key ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@OpCo2", (object?)data.OpCo2 ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Status", (object?)data.Status ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MFR", (object?)data.MFR ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@DevCode", (object?)data.DevCode ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@BegSer", (object?)data.BegSer ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@EndSer", (object?)data.EndSer ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Qty", (object?)data.Qty ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PODate", (object?)data.PODate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Vintage", (object?)data.Vintage ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PONumber", (object?)data.PONumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@RecvDate", (object?)data.RecvDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@UnitCost", (object?)data.UnitCost ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CID", (object?)data.CID ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MENumber", (object?)data.MENumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PurCode", (object?)data.PurCode ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Est", (object?)data.Est ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Comments", (object?)data.Comments ?? DBNull.Value);
        }

        /// <summary>
        /// Exports table data to an Excel file using ClosedXML.
        /// </summary>
        public static void ExportToExcel(string tableName, string filePath)
        {
            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var dt = GetTableData(tableName);
            workbook.Worksheets.Add(dt, tableName);
            workbook.SaveAs(filePath);
        }
    }

    /// <summary>
    /// Represents a single record matching the shared schema across all 4 tables.
    /// </summary>
    public class RecordData
    {
        public string? Key { get; set; }
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
    }
}
