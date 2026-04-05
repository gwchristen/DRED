using System;
using System.Windows.Forms;

namespace DRED
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

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