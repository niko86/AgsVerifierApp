using AgsVerifierLibrary;
using AgsVerifierLibrary.Comparers;
using AgsVerifierLibrary.Enums;
using AgsVerifierLibrary.Extensions;
using AgsVerifierLibrary.Models;
using AgsVerifierWindowsGUI.Commands;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace AgsVerifierWindowsGUI.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private List<RuleError> _errors;
        private bool _processAgsSuccess;

        public RelayCommand OpenFileDialogCommand { get; private set; }
        public RelayCommand ValidateAgsCommand { get; private set; }
        public RelayCommand ExportValidationReportCommand { get; private set; }
        public RelayCommand ExportAgsToExcelCommand { get; private set; }

        public MainViewModel()
        {
            OpenFileDialogCommand = new RelayCommand(OpenFileDialog);
            ValidateAgsCommand = new RelayCommand(ValidateAgs, CanValidateAgsRun);
            ExportValidationReportCommand = new RelayCommand(ExportValidationReport, CanExportValidationReport);
            ExportAgsToExcelCommand = new RelayCommand(ExportAgsToExcel, CanExportAgsToExcel);
        }

        private AgsVersion _selectedAgsVersion = AgsVersion.V404;
        public AgsVersion SelectedAgsVersion
        {
            get => _selectedAgsVersion;
            set
            {
                _selectedAgsVersion = value;
                OnPropertyChanged(nameof(SelectedAgsVersion));
            }
        }

        private string _inputFilePath;
        public string InputFilePath
        {
            get => _inputFilePath;
            set
            {
                _inputFilePath = value;
                OnPropertyChanged(nameof(InputFilePath));
            }
        }

        private string _errorText;
        public string ErrorText
        {
            get => _errorText;
            set
            {
                _errorText = value;
                OnPropertyChanged(nameof(ErrorText));
            }
        }

        public void OpenFileDialog(object obj)
        {
            OpenFileDialog dlg = new()
            {
                DefaultExt = ".ags",
                Filter = "AGS Files (*.ags)|*.ags",
                Multiselect = false,
            };

            InputFilePath = dlg.ShowDialog() == true && dlg.FileNames.Length > 0
                ? dlg.FileName
                : string.Empty;
        }

        public void ValidateAgs(object obj)
        {
            DataAccess dataAccess = new();

            _processAgsSuccess = dataAccess.ValidateAgsFile(SelectedAgsVersion, InputFilePath);
            _errors = dataAccess.Errors;

            StringBuilder sb = new();

            var groupedErrors = _errors.OrderBy(i => i.RuleId).GroupBy(k => k.RuleName);

            sb.AppendLine($"AGS validation report ");
            sb.AppendLine($"File to be validated: {InputFilePath}");
            sb.AppendLine($"Validation carried out using {SelectedAgsVersion.Name()}");
            sb.AppendLine($"Started: {DateTime.Now:G}");
            sb.AppendLine(new string('-', 140));

            foreach (var groupedError in groupedErrors)
            {
                sb.AppendLine($"{groupedError.First().Status}: Rule {groupedError.First().RuleName}");

                foreach (var error in groupedError)
                {
                    sb.AppendLine($"  Line {error.RowNumber}: {error.Message}");
                }

                sb.AppendLine();
            }
            sb.AppendLine(new string('-', 140));
            sb.AppendLine($"Finished: {DateTime.Now:G}");

            ErrorText = sb.ToString();
        }

        private bool CanValidateAgsRun(object obj)
        {
            return InputFilePath is not null;
        }

        public void ExportValidationReport(object obj)
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
                    File.WriteAllText(dlg.FileName, ErrorText);
                    MessageBox.Show("Successfully exported validation report to text file", "Export to Validation Report", MessageBoxButton.OK, MessageBoxImage.None);
                }
                catch (Exception)
                {
                    MessageBox.Show("Failed to export validation report to text file", "Export to Validation Report", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                
            }

        }

        private bool CanExportValidationReport(object obj)
        {
            return _processAgsSuccess;
        }

        public void ExportAgsToExcel(object obj)
        {
            MessageBox.Show("Exported to Excel");
        }

        private bool CanExportAgsToExcel(object obj)
        {
            return _processAgsSuccess;
        }
    }
}