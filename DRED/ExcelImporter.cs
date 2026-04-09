using System;
using System.IO;
using ClosedXML.Excel;

namespace DRED
{
    /// <summary>
    /// Imports data from the existing Created Histories.xlsx file into the Access database.
    /// Each sheet maps to its corresponding table.
    /// </summary>
    public static class ExcelImporter
    {
        private static readonly (string SheetName, string TableName)[] SheetTableMap =
        {
            ("OH - Meters",       "OH_Meters"),
            ("I&M - Meters",      "IM_Meters"),
            ("OH - Transformers", "OH_Transformers"),
            ("I&M - Transformers","IM_Transformers"),
        };

        /// <summary>
        /// Imports all sheets from the given Excel file into the database.
        /// </summary>
        /// <param name="excelFilePath">Path to the Created Histories.xlsx file.</param>
        /// <param name="progress">Optional progress reporting callback (message).</param>
        public static void Import(string excelFilePath, Action<string>? progress = null)
        {
            if (!File.Exists(excelFilePath))
                throw new FileNotFoundException("Excel file not found.", excelFilePath);

            using var workbook = new XLWorkbook(excelFilePath);

            foreach (var (sheetName, tableName) in SheetTableMap)
            {
                if (!workbook.TryGetWorksheet(sheetName, out IXLWorksheet? sheet) || sheet == null)
                {
                    progress?.Invoke($"Sheet '{sheetName}' not found in workbook — skipped.");
                    continue;
                }

                int imported = ImportSheet(sheet, tableName);
                progress?.Invoke($"Imported {imported} records from '{sheetName}' → [{tableName}].");
            }
        }

        private static int ImportSheet(IXLWorksheet sheet, string tableName)
        {
            int rowCount = 0;

            // Find the header row (row 1)
            var headerRow = sheet.Row(1);
            int lastCol = sheet.LastColumnUsed()?.ColumnNumber() ?? 0;

            // Build a column-index map based on header names
            var colMap = new System.Collections.Generic.Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int c = 1; c <= lastCol; c++)
            {
                string header = headerRow.Cell(c).GetString().Trim();
                if (!string.IsNullOrEmpty(header))
                    colMap[header] = c;
            }

            int lastRow = sheet.LastRowUsed()?.RowNumber() ?? 0;

            for (int r = 2; r <= lastRow; r++)
            {
                var row = sheet.Row(r);

                // Skip completely empty rows
                bool isEmpty = true;
                for (int c = 1; c <= Math.Min(lastCol, 5); c++)
                {
                    if (!string.IsNullOrWhiteSpace(row.Cell(c).GetString()))
                    { isEmpty = false; break; }
                }
                if (isEmpty) continue;

                var data = new RecordData
                {
                    OpCo2    = GetText(row, colMap, "OpCo2"),
                    Status   = GetText(row, colMap, "Status"),
                    MFR      = GetText(row, colMap, "MFR"),
                    DevCode  = GetText(row, colMap, "Dev Code"),
                    BegSer   = GetText(row, colMap, "Beg Ser"),
                    EndSer   = GetText(row, colMap, "End Ser"),
                    Qty      = GetInt(row, colMap, "Qty"),
                    PODate   = GetDate(row, colMap, "PO Date"),
                    Vintage  = GetText(row, colMap, "Vintage"),
                    PONumber = GetText(row, colMap, "PO Number"),
                    RecvDate = GetDate(row, colMap, "Recv Date"),
                    UnitCost = GetDecimal(row, colMap, "Unit Cost"),
                    CID      = GetText(row, colMap, "CID"),
                    MENumber = GetText(row, colMap, "M.E. #"),
                    PurCode  = GetText(row, colMap, "Pur. Code"),
                    Est      = GetBool(row, colMap, "Est."),
                    Comments = GetText(row, colMap, "Comments"),
                };

                DatabaseHelper.InsertRecord(tableName, data);
                rowCount++;
            }

            return rowCount;
        }

        private static string? GetText(IXLRow row, System.Collections.Generic.Dictionary<string, int> colMap, string colName)
        {
            if (!colMap.TryGetValue(colName, out int col)) return null;
            string val = row.Cell(col).GetString().Trim();
            return string.IsNullOrEmpty(val) ? null : val;
        }

        private static bool GetBool(IXLRow row, System.Collections.Generic.Dictionary<string, int> colMap, string colName)
        {
            if (!colMap.TryGetValue(colName, out int col)) return false;
            var cell = row.Cell(col);
            if (cell.IsEmpty()) return false;
            if (cell.DataType == XLDataType.Boolean) return cell.GetBoolean();
            string s = cell.GetString().Trim();
            if (bool.TryParse(s, out bool bv)) return bv;
            // Accept common truthy representations from Excel: "1", "Yes", "Y", "True"
            return s == "1" || s.ToUpperInvariant() is "YES" or "TRUE" or "Y";
        }

        private static int? GetInt(IXLRow row, System.Collections.Generic.Dictionary<string, int> colMap, string colName)
        {
            if (!colMap.TryGetValue(colName, out int col)) return null;
            var cell = row.Cell(col);
            if (cell.IsEmpty()) return null;

            // Try reading as number first (Excel stores all numbers as doubles)
            if (cell.DataType == XLDataType.Number)
            {
                try { return (int)Math.Round(cell.GetDouble()); }
                catch (OverflowException) { /* value out of int range — fall through */ }
            }

            // Fallback: parse string, handling "5.0" format
            string s = cell.GetString().Trim();
            if (int.TryParse(s, out int val)) return val;
            if (double.TryParse(s, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double dval)) return (int)Math.Round(dval);
            return null;
        }

        private static DateTime? GetDate(IXLRow row, System.Collections.Generic.Dictionary<string, int> colMap, string colName)
        {
            if (!colMap.TryGetValue(colName, out int col)) return null;
            var cell = row.Cell(col);
            if (cell.IsEmpty()) return null;
            try
            {
                if (cell.DataType == XLDataType.DateTime) return cell.GetDateTime();
                if (DateTime.TryParse(cell.GetString().Trim(), out DateTime dt)) return dt;
            }
            catch { }
            return null;
        }

        private static decimal? GetDecimal(IXLRow row, System.Collections.Generic.Dictionary<string, int> colMap, string colName)
        {
            if (!colMap.TryGetValue(colName, out int col)) return null;
            var cell = row.Cell(col);
            if (cell.IsEmpty()) return null;
            try
            {
                if (cell.DataType == XLDataType.Number) return (decimal)cell.GetDouble();
                string s = cell.GetString().Trim().Replace("$", "").Replace(",", "");
                if (decimal.TryParse(s, out decimal val)) return val;
            }
            catch { }
            return null;
        }
    }
}
