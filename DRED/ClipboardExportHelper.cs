using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace DRED
{
    public static class ClipboardExportHelper
    {
        public static string BuildClipboardText(DataTable table, IEnumerable<int> rowIndexes, IReadOnlyList<string> columns)
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Join('\t', columns));

            foreach (int rowIndex in rowIndexes)
            {
                if (rowIndex < 0 || rowIndex >= table.Rows.Count)
                    continue;

                DataRow row = table.Rows[rowIndex];
                var values = columns.Select(column => EscapeClipboardValue(FormatClipboardValue(row, column)));
                sb.AppendLine(string.Join('\t', values));
            }

            return sb.ToString();
        }

        private static string FormatClipboardValue(DataRow row, string columnName)
        {
            if (!row.Table.Columns.Contains(columnName) || row[columnName] is DBNull)
                return string.Empty;

            object value = row[columnName];
            if (value is DateTime dateValue)
                return dateValue.ToString("MM/dd/yyyy");
            if (value is bool boolValue)
                return boolValue ? "True" : "False";

            return Convert.ToString(value) ?? string.Empty;
        }

        private static string EscapeClipboardValue(string value)
            => value.Replace("\t", " ").Replace("\r", " ").Replace("\n", " ");
    }
}
