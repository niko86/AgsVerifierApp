using AgsVerifierLibrary.Enums;
using AgsVerifierLibrary.Extensions;
using System;
using System.Reflection;
using System.Text;
using static AgsVerifierLibrary.Enums.EnumTools;

namespace AgsVerifierWindowsGUI.Actions
{
    public static class GenerateInitialValidationTextAction
    {
        public static string Run(DateTime timestamp, string inputFilePath, AgsVersion selectedAgsVersion)
        {
            StringBuilder sb = new();

            sb.AppendLine($"AGS validation report");
            sb.AppendLine($"File Path: {inputFilePath}");
            sb.AppendLine($"Dictionary Version: {FastStr(selectedAgsVersion)}");
            sb.AppendLine($"Program version: {Assembly.GetEntryAssembly().GetName().Version} Beta");
            sb.AppendLine($"Started at {timestamp} (UTC)");
            sb.AppendLine(new string('-', 140));
            
            return sb.ToString();
        }
    }
}
