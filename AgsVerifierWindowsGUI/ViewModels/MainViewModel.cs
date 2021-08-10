using AgsVerifierLibrary;
using AgsVerifierLibrary.Enums;
using AgsVerifierLibrary.Extensions;
using AgsVerifierWindowsGUI.Actions;
using AgsVerifierWindowsGUI.Commands;
using System.Windows;

namespace AgsVerifierWindowsGUI.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly DataAccess _dataAccess;

        public RelayCommand OpenFileDialogCommand { get; private set; }
        public RelayCommand ValidateAgsCommand { get; private set; }
        public RelayCommand ExportValidationReportCommand { get; private set; }
        public RelayCommand ExportAgsToExcelCommand { get; private set; }

        public MainViewModel()
        {
            _dataAccess = new();

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
            InputFilePath = OpenFileDialogAction.Run();
        }

        public void ValidateAgs(object obj)
        {
            ProcessAgsSuccess = _dataAccess.ValidateAgsFile(SelectedAgsVersion, InputFilePath);

            ErrorText = GenerateValidationReportAction.Run(_dataAccess.Errors, InputFilePath, SelectedAgsVersion.Name());
        }

        private bool CanValidateAgsRun(object obj)
        {
            return InputFilePath is not null;
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
            MessageBox.Show("Exported to Excel");
        }

        private bool CanExportAgsToExcel(object obj)
        {
            return ProcessAgsSuccess;
        }
    }
}