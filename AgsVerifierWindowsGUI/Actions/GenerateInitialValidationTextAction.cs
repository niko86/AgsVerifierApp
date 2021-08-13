using AgsVerifierLibrary.Enums;
using AgsVerifierLibrary.Extensions;
using System;
using System.IO;
using System.Text;

namespace AgsVerifierWindowsGUI.Actions
{
    public static class GenerateInitialValidationTextAction
    {
        public static string Run(DateTime timestamp, string inputFilePath, AgsVersion selectedAgsVersion)
        {
            StringBuilder sb = new();

            sb.AppendLine($"AGS validation report");
            sb.AppendLine($"File Path: {inputFilePath}");
            sb.AppendLine($"Dictionary Version: {selectedAgsVersion.Name()}");
            sb.AppendLine($"Program version: 0.1.0 Beta");
            sb.AppendLine($"Started at {timestamp} (UTC)");
            sb.AppendLine(new string('-', 140));

            return sb.ToString();
        }
    }
}
