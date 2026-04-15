using System;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;

namespace DRED
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Logger.Log("Application starting.");

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                var ex = e.ExceptionObject as Exception
                    ?? new Exception("Unknown unhandled non-Exception error.");
                Logger.LogError("Unhandled non-UI exception.", ex);
                MessageBox.Show(
                    "An unexpected error occurred and the application may close.\n\n" + ex.Message,
                    "Unhandled Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            };
            Application.ThreadException += (s, e) =>
            {
                Logger.LogError("Unhandled UI thread exception.", e.Exception);
                MessageBox.Show(
                    "An unexpected UI error occurred.\n\n" + e.Exception.Message,
                    "Unhandled Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            };

            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.Theme = MaterialSkinManager.Themes.DARK;
            materialSkinManager.ColorScheme = new ColorScheme(
                Primary.Blue700,
                Primary.Blue900,
                Primary.Blue200,
                Accent.LightBlue200,
                TextShade.WHITE
            );

            AppSettings.Load();

            if (string.IsNullOrWhiteSpace(AppSettings.DatabasePath))
            {
                using var settingsForm = new SettingsForm();
                if (settingsForm.ShowDialog() != DialogResult.OK)
                {
                    MessageBox.Show(
                        "A database path must be configured to run DRED. The application will now exit.",
                        "Configuration Required",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }
            }

            Application.Run(new MainForm());
        }
    }
}
