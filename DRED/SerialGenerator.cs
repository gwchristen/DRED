using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DRED
{
    /// <summary>
    /// Generates fully-formatted 17-character serial numbers for meter and transformer records.
    /// </summary>
    public static class SerialGenerator
    {
        // ── Manufacturer code conversion (2-digit numeric → single char) ─

        private static readonly Dictionary<string, char> MfrMap =
            new Dictionary<string, char>(StringComparer.OrdinalIgnoreCase)
        {
            { "01", 'A' }, { "02", 'B' }, { "03", 'C' }, { "04", 'D' }, { "05", 'E' },
            { "06", 'F' }, { "07", 'G' }, { "08", 'H' }, { "09", 'I' }, { "10", 'J' },
            { "11", 'K' }, { "12", 'L' }, { "13", 'M' }, { "14", 'N' }, { "15", 'O' },
            { "16", 'P' }, { "17", 'Q' }, { "18", 'R' }, { "19", 'S' }, { "20", 'T' },
            { "21", 'U' }, { "22", 'V' }, { "23", 'W' }, { "2C", 'X' }, { "25", 'Y' },
            { "26", 'Z' }, { "27", '0' }, { "28", '1' }, { "29", '2' }, { "30", '3' },
            { "31", '4' }, { "32", '5' }, { "33", '6' }, { "34", '7' }, { "35", '8' },
            { "36", '9' }, { "37", '*' }, { "38", '.' }, { "39", ',' }, { "40", '/' },
            { "41", '+' }, { "42", '%' }, { "50", '$' }, { "24", '-' },
        };

        /// <summary>
        /// Converts a 2-character manufacturer code to a single character.
        /// Returns '?' if the code is not found.
        /// </summary>
        public static char ConvertMfrCode(string mfrCode)
        {
            if (mfrCode == null) return '?';
            return MfrMap.TryGetValue(mfrCode.Trim(), out char c) ? c : '?';
        }

        // ── Main generation entry point ──────────────────────────────────

        /// <summary>
        /// Generates a list of fully-formatted serial numbers for a record.
        /// </summary>
        /// <param name="isMeter">True for meter tabs (0/1); false for transformer tabs (2/3).</param>
        /// <param name="devCode">5-character device code.</param>
        /// <param name="mfrCode">2-digit manufacturer code stored in the record.</param>
        /// <param name="begSer">Beginning serial.</param>
        /// <param name="endSer">Ending serial (may be null/empty for single units).</param>
        /// <param name="qty">Quantity (used for VT040/VT090 range generation).</param>
        /// <param name="oosSerials">OOS serials string (one per line from the MEMO field).</param>
        /// <param name="lookupCode">Lookup code for meters (2-char, already selected by user).</param>
        /// <returns>List of formatted serial strings.</returns>
        public static List<string> Generate(
            bool isMeter,
            string devCode,
            string mfrCode,
            string begSer,
            string? endSer,
            int qty,
            string? oosSerials,
            string lookupCode = "")
        {
            char mfrChar = ConvertMfrCode(mfrCode);
            devCode = (devCode ?? "").Trim().ToUpperInvariant();

            var rangeSerials = ExpandRange(devCode, begSer, endSer, qty);
            var oosList      = ParseOosSerials(oosSerials);

            var result = new List<string>();

            if (isMeter)
            {
                foreach (string ser in rangeSerials)
                    result.Add(FormatMeterSerial(lookupCode, mfrChar, ser, devCode));
                foreach (string ser in oosList)
                    result.Add(FormatMeterSerial(lookupCode, mfrChar, ser, devCode));
            }
            else
            {
                foreach (string ser in rangeSerials)
                    result.AddRange(FormatTransformerSerial(mfrChar, ser, devCode));
                foreach (string ser in oosList)
                    result.AddRange(FormatTransformerSerial(mfrChar, ser, devCode));
            }

            return result;
        }

        // ── Meter format ─────────────────────────────────────────────────

        /// <summary>
        /// Formats a meter serial: [lookup_code(2)][mfr_char(1)][serial(9)][devCode(5)] = 17 chars
        /// </summary>
        private static string FormatMeterSerial(string lookupCode, char mfrChar, string serial, string devCode)
        {
            // Pad/trim serial to 9 digits
            string ser9 = serial.PadLeft(9, '0');
            if (ser9.Length > 9) ser9 = ser9[^9..];

            string lc = (lookupCode ?? "  ").PadRight(2)[..2];
            return $"{lc}{mfrChar}{ser9}{devCode}";
        }

        // ── Transformer format ───────────────────────────────────────────

        /// <summary>
        /// Formats transformer serial(s).
        /// Normal: [mfr_char(1)][zero_fill][serial][devCode(5)] = 17 chars
        /// VT040: generates 3 serials (prefix 1, 2, 3)
        /// VT090: generates 2 serials (prefix 1, 3)
        /// </summary>
        private static List<string> FormatTransformerSerial(char mfrChar, string serial, string devCode)
        {
            string upper = devCode.ToUpperInvariant();

            if (upper == "VT040")
                return FormatVtSerial(mfrChar, serial, devCode, new[] { "1", "2", "3" });

            if (upper == "VT090")
                return FormatVtSerial(mfrChar, serial, devCode, new[] { "1", "3" });

            return new List<string> { BuildTransformerSerial(mfrChar, serial, devCode) };
        }

        private static List<string> FormatVtSerial(char mfrChar, string serial, string devCode, string[] prefixes)
        {
            var result = new List<string>();
            foreach (string prefix in prefixes)
            {
                string ser9 = prefix + serial; // prepend to get 9-digit serial
                result.Add(BuildTransformerSerial(mfrChar, ser9, devCode));
            }
            return result;
        }

        /// <summary>
        /// Builds a single transformer serial string.
        /// Total length must be 17: 1 (mfr) + fill + serial + 5 (devCode) = 17
        /// fill = 17 - 1 - serial.Length - 5 = 11 - serial.Length (minimum 0)
        /// </summary>
        private static string BuildTransformerSerial(char mfrChar, string serial, string devCode)
        {
            int fillCount = Math.Max(0, 11 - serial.Length);
            string fill = new string('0', fillCount);
            string full = $"{mfrChar}{fill}{serial}{devCode}";
            // Truncate to 17 if somehow over
            if (full.Length > 17) full = full[..17];
            return full;
        }

        // ── Serial range expansion ───────────────────────────────────────

        /// <summary>
        /// Expands a serial range from begSer to endSer by incrementing the numeric portion.
        /// If endSer is empty/null, returns just [begSer].
        /// </summary>
        public static List<string> ExpandRange(string devCode, string begSer, string? endSer, int qty)
        {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(begSer))
                return result;

            begSer = begSer.Trim();

            if (string.IsNullOrWhiteSpace(endSer))
            {
                result.Add(begSer);
                return result;
            }

            endSer = endSer.Trim();

            // For VT040/VT090, serials are 8 digits — range still increments normally
            // Parse the numeric portion of begSer and endSer
            string prefix = ExtractPrefix(begSer);
            string suffix = ExtractSuffix(begSer);
            if (!long.TryParse(ExtractNumeric(begSer), out long begNum)) { result.Add(begSer); return result; }
            if (!long.TryParse(ExtractNumeric(endSer),  out long endNum)) { result.Add(begSer); return result; }

            int numericLen = ExtractNumeric(begSer).Length;
            int limit = qty > 0 ? qty : int.MaxValue;
            int count = 0;

            for (long n = begNum; n <= endNum && count < limit; n++, count++)
            {
                string numStr = n.ToString().PadLeft(numericLen, '0');
                result.Add(prefix + numStr + suffix);
            }

            return result;
        }

        // Helpers to split serial into prefix-letters, numeric-middle, suffix-letters
        private static string ExtractNumeric(string serial)
        {
            // Find the last run of digits in the serial
            var m = Regex.Match(serial, @"\d+$");
            if (m.Success) return m.Value;
            // Fall back: find any run of digits
            m = Regex.Match(serial, @"\d+");
            return m.Success ? m.Value : serial;
        }

        private static string ExtractPrefix(string serial)
        {
            var m = Regex.Match(serial, @"^(.*?)(\d+)(\D*)$");
            return m.Success ? m.Groups[1].Value : "";
        }

        private static string ExtractSuffix(string serial)
        {
            var m = Regex.Match(serial, @"^(.*?)(\d+)(\D*)$");
            return m.Success ? m.Groups[3].Value : "";
        }

        // ── OOS serial parsing ───────────────────────────────────────────

        private static List<string> ParseOosSerials(string? oosRaw)
        {
            if (string.IsNullOrWhiteSpace(oosRaw)) return new List<string>();
            var lines = oosRaw.Split(new[] { '\r', '\n', ',' }, StringSplitOptions.RemoveEmptyEntries);
            var result = new List<string>();
            foreach (string line in lines)
            {
                string trimmed = line.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                    result.Add(trimmed);
            }
            return result;
        }
    }
}
