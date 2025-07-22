using System;
using System.IO;
using System.Diagnostics;
using System.Windows;
using MahApps.Metro.Controls;
using Newtonsoft.Json;
using System.Linq;
using RevitNavisworksAutomation.UI.ViewModels;

namespace RevitNavisworksAutomation.UI
{
    public partial class MainWindow : MetroWindow
    {
        private MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            DataContext = _viewModel;
        }

        private void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Settings window will be implemented soon.", "Settings", MessageBoxButton.OK, MessageBoxImage.Information);
            // TODO: Implement settings window
        }

        private void OpenBatchProcessor_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "This will open RevitBatchProcessor's native interface.\n\n" +
                "Features available in Advanced Mode:\n" +
                "• Python script support\n" +
                "• Dynamo graph execution\n" +
                "• Advanced task scheduling\n" +
                "• Detailed session logging\n\n" +
                "The native interface will open in a separate window.\n" +
                "Continue?",
                "Advanced Mode",
                MessageBoxButton.YesNo,
                MessageBoxImage.Information);

            if (result == MessageBoxResult.Yes)
            {
                LaunchBatchProcessorGUI();
            }
        }

        private void LaunchBatchProcessorGUI()
        {
            try
            {
                // Try to find RevitBatchProcessor GUI
                var possiblePaths = new[]
                {
                    @"C:\Program Files (x86)\RevitBatchProcessor\RevitBatchProcessor.exe",
                    @"C:\Program Files\RevitBatchProcessor\RevitBatchProcessor.exe",
                    $@"{Environment.GetEnvironmentVariable("LOCALAPPDATA")}\RevitBatchProcessor\BatchRvtGUI.exe",
                    $@"{Environment.GetEnvironmentVariable("LOCALAPPDATA")}\RevitBatchProcessor\RevitBatchProcessorGUI.exe",
                    $@"{Environment.GetEnvironmentVariable("LOCALAPPDATA")}\RevitBatchProcessor\RevitBatchProcessor.exe"
                };

                string guiPath = null;
                foreach (var path in possiblePaths)
                {
                    if (File.Exists(path))
                    {
                        guiPath = path;
                        break;
                    }
                }

                if (guiPath == null)
                {
                    MessageBox.Show(
                        "RevitBatchProcessor GUI not found!\n\n" +
                        "Please install it from:\n" +
                        "https://github.com/bvn-architecture/RevitBatchProcessor",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                // Prepare configuration for BatchProcessor
                PrepareAdvancedModeConfiguration();

                // Launch native GUI
                var processInfo = new ProcessStartInfo
                {
                    FileName = guiPath,
                    UseShellExecute = true,
                    WorkingDirectory = Path.GetDirectoryName(guiPath)
                };

                Process.Start(processInfo);

                // Log the action
                // TODO: Add proper logging
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to launch Advanced Mode:\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void PrepareAdvancedModeConfiguration()
        {
            // Export current file list for BatchProcessor
            var selectedFiles = _viewModel.Files
                .Where(f => f.IsSelected)
                .Select(f => f.FilePath)
                .ToList();

            if (selectedFiles.Any())
            {
                var tempDir = Path.Combine(Path.GetTempPath(), "RevitNavisworksAutomation");
                Directory.CreateDirectory(tempDir);

                // Create file list
                var fileListPath = Path.Combine(tempDir, $"files_{DateTime.Now:yyyyMMddHHmmss}.txt");
                File.WriteAllLines(fileListPath, selectedFiles);

                // Create config file for BatchProcessor
                var config = new
                {
                    TaskScriptFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, 
                                                      "Scripts", "NavisExport.cs"),
                    RevitFileListFilePath = fileListPath,
                    DataExportFolderPath = _viewModel.OutputDirectory,
                    RevitSessionOption = "UseSeparateSessionPerFile",
                    RevitProcessingOption = "BatchRevitFileProcessing",
                    CentralFileOpenOption = "Detach",
                    DeleteLocalAfter = false,
                    DiscardWorksetsOnDetach = false,
                    AuditOnOpen = false,
                    ProcessingTimeOutMinutes = 60
                };

                var configPath = Path.Combine(tempDir, "batchprocessor_config.json");
                File.WriteAllText(configPath, JsonConvert.SerializeObject(config, Formatting.Indented));

                // Copy the config to clipboard for easy paste in BatchProcessor
                Clipboard.SetText(configPath);
                
                MessageBox.Show(
                    "Configuration prepared!\n\n" +
                    "The config file path has been copied to clipboard.\n" +
                    "You can paste it in BatchProcessor's settings.",
                    "Advanced Mode Ready",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _viewModel.SaveSettings();
        }
    }
}