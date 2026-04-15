using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace DRED
{
    /// <summary>
    /// Manages the device code → lookup code mapping table, persisted as JSON.
    /// Supports multiple lookup codes per device code.
    /// </summary>
    public static class LookupCodeManager
    {
        private static readonly string FilePath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lookup_codes.json");

        // key = DevCode (5-char), value = list of lookup codes (2-char each)
        private static Dictionary<string, List<string>> _table = new(StringComparer.OrdinalIgnoreCase);

        private static bool _loaded = false;

        /// <summary>
        /// Returns all current mappings as a list of (DevCode, LookupCode) pairs.
        /// </summary>
        public static List<(string DevCode, string LookupCode)> GetAll()
        {
            EnsureLoaded();
            var result = new List<(string, string)>();
            foreach (var kv in _table.OrderBy(x => x.Key))
                foreach (var lc in kv.Value)
                    result.Add((kv.Key, lc));
            return result;
        }

        /// <summary>
        /// Returns the lookup codes for a given device code.
        /// Returns an empty list if not found.
        /// </summary>
        public static List<string> GetLookupCodes(string devCode)
        {
            EnsureLoaded();
            return _table.TryGetValue(devCode, out var codes)
                ? new List<string>(codes)
                : new List<string>();
        }

        /// <summary>
        /// Adds or replaces all lookup codes for a device code.
        /// </summary>
        public static void SetLookupCodes(string devCode, IEnumerable<string> lookupCodes)
        {
            EnsureLoaded();
            _table[devCode.Trim()] = lookupCodes.Select(lc => lc.Trim()).Distinct().ToList();
            Save();
        }

        /// <summary>
        /// Adds a single lookup code for a device code (no-op if already present).
        /// </summary>
        public static void AddMapping(string devCode, string lookupCode)
        {
            EnsureLoaded();
            devCode    = devCode.Trim();
            lookupCode = lookupCode.Trim();
            if (!_table.TryGetValue(devCode, out var list))
            {
                list = new List<string>();
                _table[devCode] = list;
            }
            if (!list.Contains(lookupCode, StringComparer.OrdinalIgnoreCase))
                list.Add(lookupCode);
            Save();
        }

        /// <summary>
        /// Removes a specific (DevCode, LookupCode) mapping.
        /// Removes the DevCode entry entirely if no lookup codes remain.
        /// </summary>
        public static void RemoveMapping(string devCode, string lookupCode)
        {
            EnsureLoaded();
            if (!_table.TryGetValue(devCode, out var list)) return;
            list.RemoveAll(lc => string.Equals(lc, lookupCode, StringComparison.OrdinalIgnoreCase));
            if (list.Count == 0)
                _table.Remove(devCode);
            Save();
        }

        /// <summary>
        /// Replaces the entire mapping table with the supplied list.
        /// </summary>
        public static void SetAll(List<(string DevCode, string LookupCode)> mappings)
        {
            _table = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var (dc, lc) in mappings)
            {
                string key = dc.Trim();
                if (!_table.TryGetValue(key, out var list))
                {
                    list = new List<string>();
                    _table[key] = list;
                }
                string code = lc.Trim();
                if (!list.Contains(code, StringComparer.OrdinalIgnoreCase))
                    list.Add(code);
            }
            Save();
        }

        // ── Persistence ──────────────────────────────────────────────────

        private static void EnsureLoaded()
        {
            if (_loaded) return;
            Load();
        }

        private static void Load()
        {
            _loaded = true;
            if (File.Exists(FilePath))
            {
                try
                {
                    string json = File.ReadAllText(FilePath);
                    var raw = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json);
                    if (raw != null)
                    {
                        _table = new Dictionary<string, List<string>>(raw, StringComparer.OrdinalIgnoreCase);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError("Failed to load lookup code mappings. Falling back to defaults.", ex);
                }
            }

            // Seed with default mappings
            _table = BuildDefaults();
            Save();
        }

        private static void Save()
        {
            try
            {
                string json = JsonSerializer.Serialize(_table,
                    new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(FilePath, json);
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to save lookup code mappings.", ex);
                System.Windows.Forms.MessageBox.Show(
                    $"Failed to save lookup codes: {ex.Message}",
                    "Error",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error);
            }
        }

        private static Dictionary<string, List<string>> BuildDefaults()
        {
            var d = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            void Add(string dev, string lc)
            {
                if (!d.TryGetValue(dev, out var list)) { list = new List<string>(); d[dev] = list; }
                if (!list.Contains(lc, StringComparer.OrdinalIgnoreCase)) list.Add(lc);
            }

            Add("660F6", "xx");
            Add("EA006", "1N");
            Add("EH006", "1N");
            Add("EH006", "6V");
            Add("EH008", "2J");
            Add("EH008", "3K");
            Add("EH008", "6W");
            Add("EH015", "2B");
            Add("EH015", "3O");
            Add("EH036", "2H");
            Add("EH036", "6X");
            Add("EH038", "7K");
            Add("G3008", "NY");
            Add("G30F8", "2S");
            Add("G30G1", "TV");
            Add("G3NA1", "V9");
            Add("G3NA8", "J3");
            Add("GMNA1", "4S");
            Add("GN0E1", "ZK");
            Add("GNNA1", "V9");
            Add("GNNA8", "NY");
            Add("H30F8", "2S");
            Add("HNNA1", "V9");
            Add("HS008", "NY");
            Add("HSNA1", "1N");
            Add("N3008", "3K");
            Add("N3015", "3O");
            Add("NAD06", "1N");
            Add("NM008", "3K");
            Add("NM011", "3U");
            Add("NM021", "3T");
            Add("NM038", "7K");
            Add("NMD06", "1N");
            Add("NMD15", "3O");
            Add("NMD36", "2H");
            Add("NP008", "3K");
            Add("NUD06", "1N");
            Add("NVD06", "1N");
            Add("PM008", "NY");
            Add("PM0F1", "5C");
            Add("PM0F8", "2S");
            Add("PM0G1", "5E");
            Add("PMNA1", "V9");
            Add("PMNA8", "J3");
            Add("PS008", "NY");
            Add("PS0F8", "2S");
            Add("PSNA1", "V9");
            Add("PSNA8", "J3");

            return d;
        }
    }
}
