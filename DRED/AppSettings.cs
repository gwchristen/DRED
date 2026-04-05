using System;
using System.IO;
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
                    }
                }
            }
            catch
            {
                // If settings cannot be loaded, use defaults
            }
        }

        public static void Save()
        {
            try
            {
                Directory.CreateDirectory(SettingsDirectory);
                var data = new SettingsData { DatabasePath = DatabasePath };
                string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsFilePath, json);
            }
            catch (Exception ex)
            {
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
        }
    }
}
