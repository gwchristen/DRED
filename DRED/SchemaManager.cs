using System;
using System.Data;
using System.Data.OleDb;

namespace DRED
{
    internal static class SchemaManager
    {
        internal static void CreateDatabase(string path)
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

        internal static void EnsureTableExists(OleDbConnection conn, string tableName)
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
    [Comments] MEMO,
    [OOSSerials] MEMO
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
            EnsureOOSSerialsColumn(conn, tableName);
        }

        /// <summary>
        /// Migrates the [Est] column from TEXT(255) to YESNO for existing databases.
        /// If it's already YESNO, this method is a no-op.
        /// </summary>
        internal static void MigrateEstToBoolean(OleDbConnection conn, string tableName)
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
        internal static void EnsureTextFileColumn(OleDbConnection conn, string tableName)
        {
            try
            {
                using var cmd = new OleDbCommand($"ALTER TABLE [{tableName}] ADD COLUMN [TextFile] YESNO", conn);
                cmd.ExecuteNonQuery();
            }
            catch { /* Column likely already exists */ }
        }

        /// <summary>
        /// Adds the [OOSSerials] MEMO column if it does not already exist.
        /// </summary>
        internal static void EnsureOOSSerialsColumn(OleDbConnection conn, string tableName)
        {
            try
            {
                using var cmd = new OleDbCommand($"ALTER TABLE [{tableName}] ADD COLUMN [OOSSerials] MEMO", conn);
                cmd.ExecuteNonQuery();
            }
            catch { /* Column likely already exists */ }
        }

        internal static void EnsureAuditColumns(OleDbConnection conn, string tableName)
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

        internal static void EnsureRecordLocksTable(OleDbConnection conn)
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

        internal static void EnsureAuditLogTable(OleDbConnection conn)
        {
            DataTable schema = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables,
                new object?[] { null, null, "AuditLog", "TABLE" })!;

            if (schema.Rows.Count > 0) return;

            string sql = @"
CREATE TABLE [AuditLog] (
    [Id] AUTOINCREMENT PRIMARY KEY,
    [TableName] TEXT(255),
    [RecordId] INTEGER,
    [Action] TEXT(50),
    [FieldName] TEXT(255),
    [OldValue] MEMO,
    [NewValue] MEMO,
    [UserName] TEXT(255),
    [Timestamp] DATETIME
)";
            using var cmd = new OleDbCommand(sql, conn);
            cmd.ExecuteNonQuery();
            Logger.Log("Created [AuditLog] table.");
        }
    }
}
