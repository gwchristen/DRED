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
                materialSkinManager.AddFormToManage(settingsForm);
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