using AgsVerifierWindowsGUI.ViewModels;
using System.Windows;

namespace AgsVerifierWindowsGUI
{
    public partial class App : Application
    {
        public App()
        {
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            // Instantiate PrimaryWindowView
            MainWindow = new MainWindow
            {
                DataContext = new MainViewModel()
            };

            // Finish startup
            MainWindow.Show();
            base.OnStartup(e);
        }
    }
}
