using AgsVerifierLibrary;
using AgsVerifierLibrary.Enums;
using AgsVerifierLibrary.Extensions;
using AgsVerifierWindowsGUI.Actions;
using AgsVerifierWindowsGUI.Commands;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

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

                _dataAccess = null;
                ErrorText = null;
                ProcessAgsSuccess = false;
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

        private bool _activeIndeterminate;
        public bool ActiveIndeterminate
        {
            get => _activeIndeterminate;
            set
            {
                _activeIndeterminate = value;
                OnPropertyChanged(nameof(ActiveIndeterminate));
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

            var timestamp = DateTime.UtcNow;
            var watch = Stopwatch.StartNew();
            ActiveIndeterminate = true;

            ErrorText += GenerateInitialValidationTextAction.Run(timestamp, InputFilePath, SelectedAgsVersion);

            ProcessAgsSuccess = await Task.Run(() => _dataAccess.ValidateAgsFile(SelectedAgsVersion, InputFilePath));

            watch.Stop();
            ActiveIndeterminate = false;

            ValidateAgsCommand.RaiseCanExecuteChanged();

            ErrorText += GenerateValidationReportAction.Run(_dataAccess.Errors, watch.Elapsed);
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