using System;
using System.IO;
using System.Linq;

namespace DRED
{
    /// <summary>
    /// Provides database backup creation and retention cleanup utilities.
    /// </summary>
    public static class BackupManager
    {
        /// <summary>
        /// Gets the backup folder path and ensures it exists.
        /// </summary>
        /// <returns>The backup directory path.</returns>
        public static string GetBackupFolder()
        {
            string dbPath = AppSettings.DatabasePath;
            if (string.IsNullOrWhiteSpace(dbPath))
                throw new InvalidOperationException("Database path is not configured.");

            string dbDir = Path.GetDirectoryName(dbPath) ?? AppDomain.CurrentDomain.BaseDirectory;
            string backupDir = Path.Combine(dbDir, "Backups");
            Directory.CreateDirectory(backupDir);
            return backupDir;
        }

        /// <summary>
        /// Creates a timestamped backup copy of the configured database file.
        /// </summary>
        /// <param name="reason">A short reason label appended to the backup file name.</param>
        /// <returns>The full path to the newly created backup file.</returns>
        public static string CreateBackup(string reason)
        {
            string dbPath = AppSettings.DatabasePath;
            if (string.IsNullOrWhiteSpace(dbPath))
                throw new InvalidOperationException("Database path is not configured.");
            if (!File.Exists(dbPath))
                throw new FileNotFoundException("Database file not found.", dbPath);

            string backupDir = GetBackupFolder();
            string safeReason = string.IsNullOrWhiteSpace(reason)
                ? "manual"
                : string.Concat(reason.Trim().Select(ch =>
                    char.IsLetterOrDigit(ch) || ch == '-' || ch == '_' ? ch : '-'));
            string backupName = $"DRED_backup_{DateTime.Now:yyyy-MM-dd_HHmmss}_{safeReason}.accdb";
            string backupPath = Path.Combine(backupDir, backupName);

            File.Copy(dbPath, backupPath, overwrite: false);
            Logger.Log($"Database backup created ({reason}): '{backupPath}'.");
            return backupPath;
        }

        /// <summary>
        /// Deletes older backup files while retaining the most recent backups.
        /// </summary>
        /// <param name="keepCount">The number of newest backups to keep.</param>
        public static void CleanupOldBackups(int keepCount = 10)
        {
            keepCount = Math.Max(1, keepCount);
            string backupDir = GetBackupFolder();
            var backupFiles = new DirectoryInfo(backupDir)
                .GetFiles("DRED_backup_*.accdb")
                .OrderByDescending(f => f.CreationTimeUtc)
                .ToList();

            for (int i = keepCount; i < backupFiles.Count; i++)
            {
                try
                {
                    Logger.Log($"Deleting old backup: '{backupFiles[i].FullName}'.");
                    backupFiles[i].Delete();
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Failed to delete old backup '{backupFiles[i].FullName}'.", ex);
                }
            }
        }
    }
}
