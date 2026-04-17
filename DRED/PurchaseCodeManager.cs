using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace DRED
{
    /// <summary>
    /// Manages table-specific device code → purchase code mappings, persisted as JSON.
    /// Supports multiple purchase codes per device code.
    /// </summary>
    public static class PurchaseCodeManager
    {
        private static readonly string[] KnownTables =
            { "OH_Meters", "IM_Meters", "OH_Transformers", "IM_Transformers" };
        private static readonly object SyncLock = new();
        private static Dictionary<string, Dictionary<string, List<string>>> _table = BuildDefaults();
        private static bool _loaded = false;
        private static string _lastLoadedPath = string.Empty;

        /// <summary>
        /// Returns all table/device/purchase mappings.
        /// </summary>
        /// <returns>A list of mapping tuples sorted by table and device code.</returns>
        public static List<(string TableName, string DevCode, string PurchaseCode)> GetAll()
        {
            lock (SyncLock)
            {
                EnsureLoaded();
                var result = new List<(string, string, string)>();
                foreach (var tableKv in _table.OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase))
                {
                    foreach (var devKv in tableKv.Value.OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase))
                    {
                        foreach (var code in devKv.Value)
                            result.Add((tableKv.Key, devKv.Key, code));
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// Returns all device-to-purchase mappings for a single table.
        /// </summary>
        /// <param name="tableName">The table name to query.</param>
        /// <returns>A list of mappings for the specified table.</returns>
        public static List<(string DevCode, string PurchaseCode)> GetAllForTable(string tableName)
        {
            lock (SyncLock)
            {
                EnsureLoaded();
                string normalizedTableName = Normalize(tableName);
                if (!_table.TryGetValue(normalizedTableName, out var tableMappings))
                    return new List<(string, string)>();

                var result = new List<(string, string)>();
                foreach (var kv in tableMappings.OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase))
                    foreach (var code in kv.Value)
                        result.Add((kv.Key, code));
                return result;
            }
        }

        /// <summary>
        /// Returns purchase codes for a specific table and device code.
        /// </summary>
        /// <param name="tableName">The table name to query.</param>
        /// <param name="devCode">The device code to query.</param>
        /// <returns>A list of matching purchase codes, or an empty list when none exist.</returns>
        public static List<string> GetPurchaseCodes(string tableName, string devCode)
        {
            lock (SyncLock)
            {
                EnsureLoaded();
                tableName = Normalize(tableName);
                devCode = Normalize(devCode);
                if (!_table.TryGetValue(tableName, out var tableMappings) ||
                    !tableMappings.TryGetValue(devCode, out var codes))
                    return new List<string>();
                return new List<string>(codes);
            }
        }

        /// <summary>
        /// Adds a table/device/purchase mapping if it does not already exist.
        /// </summary>
        /// <param name="tableName">The table name.</param>
        /// <param name="devCode">The device code.</param>
        /// <param name="purchaseCode">The purchase code to add.</param>
        public static void AddMapping(string tableName, string devCode, string purchaseCode)
        {
            lock (SyncLock)
            {
                EnsureLoaded();
                tableName = Normalize(tableName);
                devCode = Normalize(devCode);
                purchaseCode = Normalize(purchaseCode);

                var tableMappings = EnsureTable(tableName);
                if (!tableMappings.TryGetValue(devCode, out var list))
                {
                    list = new List<string>();
                    tableMappings[devCode] = list;
                }
                if (!list.Contains(purchaseCode, StringComparer.OrdinalIgnoreCase))
                    list.Add(purchaseCode);
                Save();
            }
        }

        /// <summary>
        /// Removes a specific table/device/purchase mapping.
        /// </summary>
        /// <param name="tableName">The table name.</param>
        /// <param name="devCode">The device code.</param>
        /// <param name="purchaseCode">The purchase code to remove.</param>
        public static void RemoveMapping(string tableName, string devCode, string purchaseCode)
        {
            lock (SyncLock)
            {
                EnsureLoaded();
                tableName = Normalize(tableName);
                devCode = Normalize(devCode);
                purchaseCode = Normalize(purchaseCode);

                if (!_table.TryGetValue(tableName, out var tableMappings) ||
                    !tableMappings.TryGetValue(devCode, out var list))
                    return;

                list.RemoveAll(pc => string.Equals(pc, purchaseCode, StringComparison.OrdinalIgnoreCase));
                if (list.Count == 0)
                    tableMappings.Remove(devCode);
                Save();
            }
        }

        /// <summary>
        /// Replaces all mappings for a table with the supplied mapping list.
        /// </summary>
        /// <param name="tableName">The table name to replace mappings for.</param>
        /// <param name="mappings">The full set of mappings to persist.</param>
        public static void SetAllForTable(string tableName, List<(string DevCode, string PurchaseCode)> mappings)
        {
            lock (SyncLock)
            {
                EnsureLoaded();
                tableName = Normalize(tableName);
                var tableMappings = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
                foreach (var (dc, pc) in mappings)
                {
                    string key = Normalize(dc);
                    if (!tableMappings.TryGetValue(key, out var list))
                    {
                        list = new List<string>();
                        tableMappings[key] = list;
                    }

                    string code = Normalize(pc);
                    if (!list.Contains(code, StringComparer.OrdinalIgnoreCase))
                        list.Add(code);
                }
                _table[tableName] = tableMappings;
                Save();
            }
        }

        /// <summary>
        /// Clears cached mappings so data is reloaded from storage on next access.
        /// </summary>
        public static void ResetCache()
        {
            lock (SyncLock)
            {
                _loaded = false;
                _lastLoadedPath = string.Empty;
                _table = BuildDefaults();
            }
        }

        private static void EnsureLoaded()
        {
            string currentPath = GetFilePath();
            if (_loaded && !string.Equals(_lastLoadedPath, currentPath, StringComparison.OrdinalIgnoreCase))
                ResetCache();
            if (_loaded) return;
            Load();
        }

        private static void Load()
        {
            string filePath = GetFilePath();
            _loaded = true;
            _lastLoadedPath = filePath;
            MigrateLegacyFileIfNeeded(filePath);
            if (File.Exists(filePath))
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    if (TryDeserializeLegacy(json, out var legacy))
                    {
                        _table = ExpandLegacyToAllTables(legacy);
                        Logger.Log(
                            $"Migrated purchase code mappings from legacy flat format to table-aware format in '{filePath}'.");
                        Save();
                        return;
                    }

                    var raw = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, List<string>>>>(json);
                    if (raw != null)
                    {
                        _table = BuildDefaults();
                        foreach (var tableKv in raw)
                        {
                            var tableMappings = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
                            foreach (var devKv in tableKv.Value)
                            {
                                var distinctCodes = devKv.Value
                                    .Where(code => !string.IsNullOrWhiteSpace(code))
                                    .Select(Normalize)
                                    .Distinct(StringComparer.OrdinalIgnoreCase)
                                    .ToList();
                                if (distinctCodes.Count > 0)
                                    tableMappings[Normalize(devKv.Key)] = distinctCodes;
                            }
                            _table[Normalize(tableKv.Key)] = tableMappings;
                        }
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError("Failed to load purchase code mappings. Falling back to defaults.", ex);
                }
            }

            _table = BuildDefaults();
            Save();
        }

        private static void Save()
        {
            try
            {
                string filePath = GetFilePath();
                _lastLoadedPath = filePath;
                string? directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrWhiteSpace(directory))
                    Directory.CreateDirectory(directory);

                string json = JsonSerializer.Serialize(_table,
                    new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to save purchase code mappings.", ex);
                System.Windows.Forms.MessageBox.Show(
                    $"Failed to save purchase codes: {ex.Message}",
                    "Error",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error);
            }
        }

        private static string GetFilePath()
        {
            if (!string.IsNullOrWhiteSpace(AppSettings.PurchaseCodesPath))
                return AppSettings.PurchaseCodesPath;

            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "DRED", "purchase_codes.json");
        }

        private static void MigrateLegacyFileIfNeeded(string newPath)
        {
            if (!string.IsNullOrWhiteSpace(AppSettings.PurchaseCodesPath))
                return;

            string legacyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "purchase_codes.json");
            if (File.Exists(newPath) || !File.Exists(legacyPath))
                return;

            try
            {
                string? newDirectory = Path.GetDirectoryName(newPath);
                if (!string.IsNullOrWhiteSpace(newDirectory))
                    Directory.CreateDirectory(newDirectory);
                File.Copy(legacyPath, newPath);
                Logger.Log($"Migrated purchase_codes.json from '{legacyPath}' to '{newPath}'.");
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to migrate purchase_codes.json to new location.", ex);
            }
        }

        private static Dictionary<string, Dictionary<string, List<string>>> BuildDefaults()
        {
            var result = new Dictionary<string, Dictionary<string, List<string>>>(StringComparer.OrdinalIgnoreCase);
            foreach (string table in KnownTables)
                result[table] = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            return result;
        }

        private static string Normalize(string value) => value.Trim();

        private static Dictionary<string, List<string>> EnsureTable(string tableName)
        {
            if (!_table.TryGetValue(tableName, out var tableMappings))
            {
                tableMappings = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
                _table[tableName] = tableMappings;
            }
            return tableMappings;
        }

        private static bool TryDeserializeLegacy(string json, out Dictionary<string, List<string>> legacy)
        {
            legacy = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.ValueKind != JsonValueKind.Object)
                    return false;

                bool hasLegacyValues = false;
                foreach (var prop in doc.RootElement.EnumerateObject())
                {
                    if (prop.Value.ValueKind != JsonValueKind.Array)
                        continue;

                    hasLegacyValues = true;
                    var codes = new List<string>();
                    foreach (var val in prop.Value.EnumerateArray())
                    {
                        if (val.ValueKind == JsonValueKind.String)
                        {
                            string normalized = Normalize(val.GetString() ?? string.Empty);
                            if (!string.IsNullOrWhiteSpace(normalized) &&
                                !codes.Contains(normalized, StringComparer.OrdinalIgnoreCase))
                                codes.Add(normalized);
                        }
                    }

                    if (codes.Count > 0)
                        legacy[Normalize(prop.Name)] = codes;
                }

                return hasLegacyValues;
            }
            catch
            {
                return false;
            }
        }

        private static Dictionary<string, Dictionary<string, List<string>>> ExpandLegacyToAllTables(
            Dictionary<string, List<string>> legacy)
        {
            var result = BuildDefaults();
            foreach (var tableName in KnownTables)
            {
                var target = result[tableName];
                foreach (var kv in legacy)
                    target[kv.Key] = new List<string>(kv.Value);
            }

            return result;
        }
    }
}
