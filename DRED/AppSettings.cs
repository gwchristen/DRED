using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace DRED
{
    /// <summary>
    /// Manages application settings, stored as JSON in the user's AppData folder.
    /// </summary>
    public static class AppSettings
    {
        private static readonly string SettingsDirectory =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DRED");

        private static readonly string SettingsFilePath =
            Path.Combine(SettingsDirectory, "settings.json");

        public static string DatabasePath { get; set; } = string.Empty;
        public static int AutoRefreshInterval { get; set; } = 60;
        public static string LockPin { get; set; } = "1234";
        public static List<string> AuthorizedUsers { get; set; } = new();

        public static void Load()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    string json = File.ReadAllText(SettingsFilePath);
                    var data = JsonSerializer.Deserialize<SettingsData>(json);
                    if (data != null)
                    {
                        DatabasePath = data.DatabasePath ?? string.Empty;
                        AutoRefreshInterval = data.AutoRefreshInterval;
                        LockPin = string.IsNullOrWhiteSpace(data.LockPin) ? "1234" : data.LockPin;
                        AuthorizedUsers = data.AuthorizedUsers?
                            .Where(u => !string.IsNullOrWhiteSpace(u))
                            .Select(u => u.Trim())
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .ToList() ?? new List<string>();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to load app settings. Using defaults.", ex);
            }
        }

        public static void Save()
        {
            try
            {
                Directory.CreateDirectory(SettingsDirectory);
                var data = new SettingsData
                {
                    DatabasePath = DatabasePath,
                    AutoRefreshInterval = AutoRefreshInterval,
                    LockPin = string.IsNullOrWhiteSpace(LockPin) ? "1234" : LockPin,
                    AuthorizedUsers = AuthorizedUsers
                        .Where(u => !string.IsNullOrWhiteSpace(u))
                        .Select(u => u.Trim())
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList(),
                };
                string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsFilePath, json);
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to save app settings.", ex);
                System.Windows.Forms.MessageBox.Show(
                    $"Failed to save settings: {ex.Message}",
                    "Error",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error);
            }
        }

        private class SettingsData
        {
            public string? DatabasePath { get; set; }
            public int AutoRefreshInterval { get; set; } = 60;
            public string LockPin { get; set; } = "1234";
            public List<string> AuthorizedUsers { get; set; } = new();
        }
    }
}
