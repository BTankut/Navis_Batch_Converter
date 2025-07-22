using System;
using System.Windows;

namespace RevitNavisworksAutomation
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Setup global exception handling
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            DispatcherUnhandledException += OnDispatcherUnhandledException;

            // Initialize application settings
            InitializeSettings();
        }

        private void InitializeSettings()
        {
            // Ensure output directories exist
            var outputDir = System.Configuration.ConfigurationManager.AppSettings["DefaultOutputDirectory"];
            var logDir = System.Configuration.ConfigurationManager.AppSettings["DefaultLogDirectory"];

            if (!string.IsNullOrEmpty(outputDir) && !System.IO.Directory.Exists(outputDir))
            {
                System.IO.Directory.CreateDirectory(outputDir);
            }

            if (!string.IsNullOrEmpty(logDir) && !System.IO.Directory.Exists(logDir))
            {
                System.IO.Directory.CreateDirectory(logDir);
            }
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            LogException(exception);
            ShowErrorMessage(exception);
        }

        private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            LogException(e.Exception);
            ShowErrorMessage(e.Exception);
            e.Handled = true;
        }

        private void LogException(Exception exception)
        {
            try
            {
                var logPath = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "NavisBatchConverter",
                    "Logs",
                    $"crash_{DateTime.Now:yyyyMMdd_HHmmss}.log"
                );

                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(logPath));

                System.IO.File.WriteAllText(logPath, $@"
Crash Report - {DateTime.Now:yyyy-MM-dd HH:mm:ss}
============================================
Exception: {exception?.GetType().FullName}
Message: {exception?.Message}
Stack Trace:
{exception?.StackTrace}

Inner Exception: {exception?.InnerException?.Message}
Inner Stack Trace:
{exception?.InnerException?.StackTrace}
");
            }
            catch
            {
                // Fail silently if we can't write the log
            }
        }

        private void ShowErrorMessage(Exception exception)
        {
            MessageBox.Show(
                $"An unexpected error occurred:\n\n{exception?.Message}\n\nThe application will now close.",
                "Fatal Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );

            Environment.Exit(1);
        }
    }
}