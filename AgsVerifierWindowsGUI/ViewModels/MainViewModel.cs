using AgsVerifierLibrary;
using AgsVerifierLibrary.Enums;
using AgsVerifierLibrary.Extensions;
using AgsVerifierWindowsGUI.Actions;
using AgsVerifierWindowsGUI.Commands;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

namespace AgsVerifierWindowsGUI.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private DataAccess _dataAccess;
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

        private bool _processAgsSuccess;
        public bool ProcessAgsSuccess
        {
            get => _processAgsSuccess;
            set
            {
                _processAgsSuccess = value;
                OnPropertyChanged(nameof(ProcessAgsSuccess));
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
            ErrorText = null;
            ProcessAgsSuccess = false;

            InputFilePath = OpenFileDialogAction.Run();
        }

        public async void ValidateAgs(object obj)
        {
            _dataAccess = new();

            var timestamp = DateTime.Now;
            var watch = Stopwatch.StartNew();

            ProcessAgsSuccess = await Task.Run(() => _dataAccess.ValidateAgsFile(SelectedAgsVersion, InputFilePath));

            ValidateAgsCommand.RaiseCanExecuteChanged();

            watch.Stop();

            ErrorText = GenerateValidationReportAction.Run(_dataAccess.Errors, timestamp, watch.Elapsed, InputFilePath, SelectedAgsVersion.Name());
        }

        private bool CanValidateAgsRun(object obj)
        {
            return InputFilePath is not null && ProcessAgsSuccess is false;
        }

        public void ExportValidationReport(object obj)
        {
            ExportValidationReportAction.Run(ErrorText);
        }

        private bool CanExportValidationReport(object obj)
        {
            return ProcessAgsSuccess;
        }

        public void ExportAgsToExcel(object obj)
        {
            ExportAgsToExcelAction.Run(_dataAccess.Ags);
        }

        private bool CanExportAgsToExcel(object obj)
        {
            return ProcessAgsSuccess;
        }
    }
}