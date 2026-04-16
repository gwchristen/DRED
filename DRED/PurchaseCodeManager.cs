using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace DRED
{
    /// <summary>
    /// Manages the device code → purchase code mapping table, persisted as JSON.
    /// Supports multiple purchase codes per device code.
    /// </summary>
    public static class PurchaseCodeManager
    {
        private static readonly object SyncLock = new();
        private static Dictionary<string, List<string>> _table = new(StringComparer.OrdinalIgnoreCase);
        private static bool _loaded = false;
        private static string _lastLoadedPath = string.Empty;

        public static List<(string DevCode, string PurchaseCode)> GetAll()
        {
            lock (SyncLock)
            {
                EnsureLoaded();
                var result = new List<(string, string)>();
                foreach (var kv in _table.OrderBy(x => x.Key))
                    foreach (var code in kv.Value)
                        result.Add((kv.Key, code));
                return result;
            }
        }

        public static List<string> GetPurchaseCodes(string devCode)
        {
            lock (SyncLock)
            {
                EnsureLoaded();
                return _table.TryGetValue(devCode.Trim(), out var codes)
                    ? new List<string>(codes)
                    : new List<string>();
            }
        }

        public static void AddMapping(string devCode, string purchaseCode)
        {
            lock (SyncLock)
            {
                EnsureLoaded();
                devCode = devCode.Trim();
                purchaseCode = purchaseCode.Trim();
                if (!_table.TryGetValue(devCode, out var list))
                {
                    list = new List<string>();
                    _table[devCode] = list;
                }
                if (!list.Contains(purchaseCode, StringComparer.OrdinalIgnoreCase))
                    list.Add(purchaseCode);
                Save();
            }
        }

        public static void RemoveMapping(string devCode, string purchaseCode)
        {
            lock (SyncLock)
            {
                EnsureLoaded();
                if (!_table.TryGetValue(devCode, out var list)) return;
                list.RemoveAll(pc => string.Equals(pc, purchaseCode, StringComparison.OrdinalIgnoreCase));
                if (list.Count == 0)
                    _table.Remove(devCode);
                Save();
            }
        }

        public static void SetAll(List<(string DevCode, string PurchaseCode)> mappings)
        {
            lock (SyncLock)
            {
                _table = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
                foreach (var (dc, pc) in mappings)
                {
                    string key = dc.Trim();
                    if (!_table.TryGetValue(key, out var list))
                    {
                        list = new List<string>();
                        _table[key] = list;
                    }

                    string code = pc.Trim();
                    if (!list.Contains(code, StringComparer.OrdinalIgnoreCase))
                        list.Add(code);
                }
                Save();
            }
        }

        public static void ResetCache()
        {
            lock (SyncLock)
            {
                _loaded = false;
                _lastLoadedPath = string.Empty;
                _table = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
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
                    var raw = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json);
                    if (raw != null)
                    {
                        _table = new Dictionary<string, List<string>>(raw, StringComparer.OrdinalIgnoreCase);
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

        private static Dictionary<string, List<string>> BuildDefaults()
            => new(StringComparer.OrdinalIgnoreCase);
    }
}
