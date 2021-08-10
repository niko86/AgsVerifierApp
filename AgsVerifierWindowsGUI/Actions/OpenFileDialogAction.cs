using Microsoft.Win32;

namespace AgsVerifierWindowsGUI.Actions
{
    public static class OpenFileDialogAction
    {
        public static string Run()
        {
            OpenFileDialog dlg = new()
            {
                DefaultExt = ".ags",
                Filter = "AGS Files (*.ags)|*.ags",
                Multiselect = false,
            };

            return dlg.ShowDialog() == true && dlg.FileNames.Length > 0
                ? dlg.FileName
                : string.Empty;
        }
    }
}
