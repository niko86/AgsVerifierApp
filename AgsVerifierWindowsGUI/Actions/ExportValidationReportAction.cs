using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;

namespace AgsVerifierWindowsGUI.Actions
{
    public static class ExportValidationReportAction
    {
        public static void Run(string errorText)
        {
            SaveFileDialog dlg = new()
            {
                DefaultExt = ".txt",
                Filter = "TXT Files (*.txt)|*.txt",
            };

            if (dlg.ShowDialog() == true && dlg.FileNames.Length > 0)
            {
                try
                {
                    File.WriteAllText(dlg.FileName, errorText);
                    MessageBox.Show("Successfully exported validation report to text file", "Export to Validation Report", MessageBoxButton.OK, MessageBoxImage.None);
                }
                catch (Exception)
                {
                    MessageBox.Show("Failed to export validation report to text file", "Export to Validation Report", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
