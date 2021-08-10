using AgsVerifierLibrary.Enums;
using AgsVerifierLibrary.Extensions;
using AgsVerifierLibrary.Models;
using Microsoft.Win32;
using OfficeOpenXml;
using System;
using System.IO;
using System.Linq;
using System.Windows;

namespace AgsVerifierWindowsGUI.Actions
{
    public static class ExportAgsToExcelAction
    {
        public static void Run(AgsContainer ags)
        {
            using ExcelPackage excelPackage = new();

            foreach (var group in ags.Groups)
            {
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add($"{group.Name} - AGS");

                for (int i = 0; i < group.Columns.Count; i++)
                {
                    if (group[i].Heading is "Index")
                        continue;

                    worksheet.Cells[1, i].Value = group[i].Heading;
                    worksheet.Cells[2, i].Value = group[i].Unit;
                    worksheet.Cells[3, i].Value = group[i].Type;

                    for (int j = 0; j < group[i].Data.Count; j++)
                    {
                        worksheet.Cells[4 + j, i].Value = group[i].Data[j];
                    }
                }
            }

            SaveFileDialog dlg = new()
            {
                DefaultExt = ".xlsx",
                Filter = "XLSX Files (*.xlsx)|*.xlsx",
            };

            if (dlg.ShowDialog() == true && dlg.FileNames.Length > 0)
            {
                try
                {
                    excelPackage.SaveAs(new FileInfo(dlg.FileName));
                    MessageBox.Show("Successfully exported AGS to Excel file", "Export AGS to Excel", MessageBoxButton.OK, MessageBoxImage.None);
                }
                catch (Exception)
                {
                    MessageBox.Show("Failed to export AGS to Excel file", "Export AGS to Excel", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
