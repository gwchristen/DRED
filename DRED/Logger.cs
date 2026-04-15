using System;
using System.IO;
using System.Text;

namespace DRED
{
    /// <summary>
    /// Provides simple file-based logging for the application.
    /// </summary>
    public static class Logger
    {
        private static readonly object SyncRoot = new();
        private static readonly string LogDirectory =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DRED");
        private static readonly string LogFilePath = Path.Combine(LogDirectory, "dred.log");
        private static readonly string OldLogFilePath = Path.Combine(LogDirectory, "dred.log.old");
        public const int LogMaxSizeMB = 5;

        /// <summary>
        /// Writes an informational log entry.
        /// </summary>
        public static void Log(string message) => Write("INFO", message, null);

        /// <summary>
        /// Writes a warning log entry.
        /// </summary>
        public static void LogWarning(string message) => Write("WARN", message, null);

        /// <summary>
        /// Writes an error log entry.
        /// </summary>
        public static void LogError(string message, Exception? ex = null) => Write("ERROR", message, ex);

        private static void Write(string level, string message, Exception? ex)
        {
            try
            {
                lock (SyncRoot)
                {
                    Directory.CreateDirectory(LogDirectory);
                    RotateIfNeeded();

                    var sb = new StringBuilder();
                    sb.Append('[').Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).Append("] ");
                    sb.Append('[').Append(level).Append("] ");
                    sb.Append('[').Append(Environment.UserName).Append("] ");
                    sb.AppendLine(message ?? string.Empty);

                    if (ex != null)
                    {
                        sb.AppendLine($"{ex.GetType().FullName}: {ex.Message}");
                        if (!string.IsNullOrWhiteSpace(ex.StackTrace))
                            sb.AppendLine(ex.StackTrace);
                    }

                    File.AppendAllText(LogFilePath, sb.ToString());
                }
            }
            catch
            {
                // Logging failures should never crash the app.
            }
        }

        private static void RotateIfNeeded()
        {
            if (!File.Exists(LogFilePath))
                return;

            long maxBytes = LogMaxSizeMB * 1024L * 1024L;
            var info = new FileInfo(LogFilePath);
            if (info.Length <= maxBytes)
                return;

            if (File.Exists(OldLogFilePath))
                File.Delete(OldLogFilePath);
            File.Move(LogFilePath, OldLogFilePath);
        }
    }
}
