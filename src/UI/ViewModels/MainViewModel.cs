using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Linq;
using System.IO;
using Microsoft.Win32;
using System.Diagnostics;
using System.Windows.Forms;
using System.Windows;
using RevitNavisworksAutomation.Core;
using RevitNavisworksAutomation.Models;

namespace RevitNavisworksAutomation.UI.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<ConversionJob> Files { get; }
        public ObservableCollection<string> RevitVersions { get; }
        public ObservableCollection<WorksetFilter> WorksetFilters { get; }

        // Commands
        public ICommand AddFilesCommand { get; private set; }
        public ICommand AddFolderCommand { get; private set; }
        public ICommand ClearAllCommand { get; private set; }
        public ICommand StartConversionCommand { get; private set; }
        public ICommand StopConversionCommand { get; private set; }
        public ICommand RemoveFileCommand { get; private set; }
        public ICommand ViewDetailsCommand { get; private set; }
        public ICommand ViewLogsCommand { get; private set; }
        public ICommand BrowseOutputDirectoryCommand { get; private set; }
        public ICommand AddWorksetCommand { get; private set; }

        // Properties
        private bool _isProcessing;
        public bool IsProcessing
        {
            get => _isProcessing;
            set { _isProcessing = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanStartConversion)); }
        }

        private string _statusMessage = "Ready";
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        private double _overallProgress;
        public double OverallProgress
        {
            get => _overallProgress;
            set { _overallProgress = value; OnPropertyChanged(); }
        }

        private double _progress;
        public double Progress
        {
            get => _progress;
            set { _progress = value; OnPropertyChanged(); }
        }

        private string _progressText = "Ready";
        public string ProgressText
        {
            get => _progressText;
            set { _progressText = value; OnPropertyChanged(); }
        }

        private string _selectedRevitVersion = "2022";
        public string SelectedRevitVersion
        {
            get => _selectedRevitVersion;
            set { _selectedRevitVersion = value; OnPropertyChanged(); SaveSettings(); }
        }

        private string _viewFilterPattern = "navis_view";
        public string ViewFilterPattern
        {
            get => _viewFilterPattern;
            set { _viewFilterPattern = value; OnPropertyChanged(); SaveSettings(); }
        }

        private bool _viewFilterCaseSensitive;
        public bool ViewFilterCaseSensitive
        {
            get => _viewFilterCaseSensitive;
            set { _viewFilterCaseSensitive = value; OnPropertyChanged(); SaveSettings(); }
        }

        private bool _requireSheetAssociation;
        public bool RequireSheetAssociation
        {
            get => _requireSheetAssociation;
            set { _requireSheetAssociation = value; OnPropertyChanged(); SaveSettings(); }
        }

        private bool _filterWorksets = true;
        public bool FilterWorksets
        {
            get => _filterWorksets;
            set { _filterWorksets = value; OnPropertyChanged(); SaveSettings(); }
        }

        private bool _exportLinks = true;
        public bool ExportLinks
        {
            get => _exportLinks;
            set { _exportLinks = value; OnPropertyChanged(); SaveSettings(); }
        }

        private bool _exportParts = true;
        public bool ExportParts
        {
            get => _exportParts;
            set { _exportParts = value; OnPropertyChanged(); SaveSettings(); }
        }

        private bool _useSharedCoordinates = true;
        public bool UseSharedCoordinates
        {
            get => _useSharedCoordinates;
            set { _useSharedCoordinates = value; OnPropertyChanged(); SaveSettings(); }
        }

        private bool _convertElementProperties = true;
        public bool ConvertElementProperties
        {
            get => _convertElementProperties;
            set { _convertElementProperties = value; OnPropertyChanged(); SaveSettings(); }
        }

        private bool _combineToNWD = true;
        public bool CombineToNWD
        {
            get => _combineToNWD;
            set { _combineToNWD = value; OnPropertyChanged(); SaveSettings(); }
        }

        private bool _deleteNWCAfterCombine;
        public bool DeleteNWCAfterCombine
        {
            get => _deleteNWCAfterCombine;
            set { _deleteNWCAfterCombine = value; OnPropertyChanged(); SaveSettings(); }
        }

        private bool _compressNWD = true;
        public bool CompressNWD
        {
            get => _compressNWD;
            set { _compressNWD = value; OnPropertyChanged(); SaveSettings(); }
        }

        private bool _findMissingMaterials = true;
        public bool FindMissingMaterials
        {
            get => _findMissingMaterials;
            set { _findMissingMaterials = value; OnPropertyChanged(); SaveSettings(); }
        }

        private bool _excludeTemplates = true;
        public bool ExcludeTemplates
        {
            get => _excludeTemplates;
            set { _excludeTemplates = value; OnPropertyChanged(); SaveSettings(); }
        }

        private string _outputDirectory = @"C:\Output\Navisworks";
        public string OutputDirectory
        {
            get => _outputDirectory;
            set { _outputDirectory = value; OnPropertyChanged(); SaveSettings(); }
        }

        private string _statusIcon = "CheckCircle";
        public string StatusIcon
        {
            get => _statusIcon;
            set { _statusIcon = value; OnPropertyChanged(); }
        }

        private string _statusColor = "Green";
        public string StatusColor
        {
            get => _statusColor;
            set { _statusColor = value; OnPropertyChanged(); }
        }

        private string _elapsedTime = "00:00:00";
        public string ElapsedTime
        {
            get => _elapsedTime;
            set { _elapsedTime = value; OnPropertyChanged(); }
        }

        private bool _showProgressDialog;
        public bool ShowProgressDialog
        {
            get => _showProgressDialog;
            set { _showProgressDialog = value; OnPropertyChanged(); }
        }

        public int FileCount => Files?.Count ?? 0;
        public bool CanStartConversion => !IsProcessing && Files.Any(f => f.IsSelected);

        public MainViewModel()
        {
            Files = new ObservableCollection<ConversionJob>();
            Files.CollectionChanged += (s, e) => 
            {
                OnPropertyChanged(nameof(FileCount));
                OnPropertyChanged(nameof(CanStartConversion));
            };
            
            RevitVersions = new ObservableCollection<string> { "2021", "2022", "2023", "2024" };
            WorksetFilters = new ObservableCollection<WorksetFilter>
            {
                new WorksetFilter { Name = "Architecture", IsIncluded = true },
                new WorksetFilter { Name = "Structure", IsIncluded = true },
                new WorksetFilter { Name = "MEP", IsIncluded = true },
                new WorksetFilter { Name = "Coordination", IsIncluded = false }
            };

            InitializeCommands();
            LoadSettings();
        }

        private void InitializeCommands()
        {
            AddFilesCommand = new RelayCommand(AddFiles);
            AddFolderCommand = new RelayCommand(AddFolder);
            ClearAllCommand = new RelayCommand(() => Files.Clear());
            StartConversionCommand = new RelayCommand(StartConversion, () => CanStartConversion);
            StopConversionCommand = new RelayCommand(StopConversion);
            RemoveFileCommand = new RelayCommand<ConversionJob>(RemoveFile);
            ViewDetailsCommand = new RelayCommand<ConversionJob>(ViewDetails);
            ViewLogsCommand = new RelayCommand(ViewLogs);
            BrowseOutputDirectoryCommand = new RelayCommand(BrowseOutputDirectory);
            AddWorksetCommand = new RelayCommand<string>(AddWorkset);
        }

        private void AddFiles()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Select Revit Files",
                Filter = "Revit Files (*.rvt)|*.rvt|All Files (*.*)|*.*",
                Multiselect = true
            };

            if (dialog.ShowDialog() == true)
            {
                foreach (var file in dialog.FileNames)
                {
                    if (!Files.Any(f => f.FilePath == file))
                    {
                        var job = new ConversionJob(file);
                        job.PropertyChanged += (s, e) => 
                        {
                            if (e.PropertyName == nameof(ConversionJob.IsSelected))
                            {
                                OnPropertyChanged(nameof(CanStartConversion));
                                CommandManager.InvalidateRequerySuggested();
                            }
                        };
                        Files.Add(job);
                    }
                }
                OnPropertyChanged(nameof(FileCount));
                OnPropertyChanged(nameof(CanStartConversion));
            }
        }

        private void AddFolder()
        {
            var dialog = new FolderBrowserDialog
            {
                Description = "Select folder containing Revit files",
                ShowNewFolderButton = false
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var revitFiles = Directory.GetFiles(dialog.SelectedPath, "*.rvt", SearchOption.AllDirectories);
                foreach (var file in revitFiles)
                {
                    if (!Files.Any(f => f.FilePath == file))
                    {
                        var job = new ConversionJob(file);
                        job.PropertyChanged += (s, e) => 
                        {
                            if (e.PropertyName == nameof(ConversionJob.IsSelected))
                            {
                                OnPropertyChanged(nameof(CanStartConversion));
                                CommandManager.InvalidateRequerySuggested();
                            }
                        };
                        Files.Add(job);
                    }
                }
                OnPropertyChanged(nameof(FileCount));
                OnPropertyChanged(nameof(CanStartConversion));
            }
        }

        private async void StartConversion()
        {
            IsProcessing = true;
            StatusMessage = "Starting conversion...";
            StatusIcon = "Progress";
            StatusColor = "Orange";
            ShowProgressDialog = true;
            Progress = 0;
            ProgressText = "Initializing conversion...";
            
            try
            {
                // Get selected files
                var selectedFiles = Files.Where(f => f.IsSelected).ToList();
                
                // Update file statuses
                foreach (var file in selectedFiles)
                {
                    file.Status = ConversionStatus.Processing;
                    file.StatusMessage = "Processing...";
                    file.Progress = 0;
                }
                
                // Create batch script for RevitBatchProcessor
                var batchScript = CreateBatchScript(selectedFiles);
                
                // Run RevitBatchProcessor
                await RunRevitBatchProcessor(batchScript);
                
                // Update statuses on completion
                foreach (var file in selectedFiles)
                {
                    if (file.Status == ConversionStatus.Processing)
                    {
                        file.Status = ConversionStatus.Completed;
                        file.StatusMessage = "Completed";
                        file.Progress = 100;
                    }
                }
                
                StatusMessage = "Conversion completed successfully!";
                StatusIcon = "CheckCircle";
                StatusColor = "Green";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                StatusIcon = "AlertCircle";
                StatusColor = "Red";
                Logger.Instance.Error("Conversion failed", ex);
                
                // Update file statuses on error
                foreach (var file in Files.Where(f => f.IsSelected))
                {
                    if (file.Status == ConversionStatus.Processing)
                    {
                        file.Status = ConversionStatus.Failed;
                        file.StatusMessage = "Failed";
                        file.ErrorMessage = ex.Message;
                    }
                }
            }
            finally
            {
                IsProcessing = false;
                ShowProgressDialog = false;
                Progress = 100;
            }
        }

        private void StopConversion()
        {
            IsProcessing = false;
            StatusMessage = "Conversion stopped";
            StatusIcon = "StopCircle";
            StatusColor = "Red";
            ShowProgressDialog = false;
        }

        private void RemoveFile(ConversionJob job)
        {
            if (job != null)
            {
                Files.Remove(job);
                OnPropertyChanged(nameof(FileCount));
            }
        }

        private void ViewDetails(ConversionJob job)
        {
            // TODO: Implement file details view
        }

        private void ViewLogs()
        {
            var logDir = Path.Combine(Environment.CurrentDirectory, "logs");
            if (Directory.Exists(logDir))
            {
                Process.Start("explorer.exe", logDir);
            }
        }

        private void BrowseOutputDirectory()
        {
            var dialog = new FolderBrowserDialog
            {
                Description = "Select output directory",
                SelectedPath = OutputDirectory,
                ShowNewFolderButton = true
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                OutputDirectory = dialog.SelectedPath;
            }
        }

        private void AddWorkset(string worksetName)
        {
            if (!string.IsNullOrWhiteSpace(worksetName) && 
                !WorksetFilters.Any(w => w.Name.Equals(worksetName, StringComparison.OrdinalIgnoreCase)))
            {
                WorksetFilters.Add(new WorksetFilter { Name = worksetName, IsIncluded = true });
            }
        }

        private string CreateBatchScript(List<ConversionJob> files)
        {
            // Create a JSON configuration file for the PowerShell script
            var config = new
            {
                InputFiles = files.Select(f => f.FilePath).ToList(),
                OutputDirectory = OutputDirectory,
                RevitVersion = SelectedRevitVersion,
                ViewFilter = new
                {
                    Pattern = ViewFilterPattern,
                    CaseSensitive = ViewFilterCaseSensitive,
                    RequireSheetAssociation = RequireSheetAssociation
                },
                WorksetFilter = new
                {
                    Enabled = FilterWorksets,
                    IncludedWorksets = WorksetFilters.Where(w => w.IsIncluded).Select(w => w.Name).ToList()
                },
                ExportOptions = new
                {
                    ExportLinks = ExportLinks,
                    ExportParts = ExportParts,
                    UseSharedCoordinates = UseSharedCoordinates,
                    ConvertElementProperties = ConvertElementProperties,
                    FindMissingMaterials = FindMissingMaterials
                },
                PostProcessing = new
                {
                    CombineToNWD = CombineToNWD,
                    DeleteNWCAfterCombine = DeleteNWCAfterCombine,
                    CompressNWD = CompressNWD
                }
            };
            
            var configPath = Path.Combine(Path.GetTempPath(), $"NavisBatchConfig_{DateTime.Now:yyyyMMdd_HHmmss}.json");
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(config, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(configPath, json);
            
            return configPath;
        }
        
        private async System.Threading.Tasks.Task RunRevitBatchProcessor(string batchScript)
        {
            await System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    // Ensure output directory exists
                    if (!Directory.Exists(OutputDirectory))
                    {
                        Directory.CreateDirectory(OutputDirectory);
                    }
                    
                    var logDir = Path.Combine(OutputDirectory, "logs");
                    if (!Directory.Exists(logDir))
                    {
                        Directory.CreateDirectory(logDir);
                    }
                    
                    // Find BatchRvt.exe
                    var rbpPath = FindRevitBatchProcessor();
                    
                    // Read configuration
                    var configJson = File.ReadAllText(batchScript);
                    dynamic config = Newtonsoft.Json.JsonConvert.DeserializeObject(configJson);
                    var inputFiles = config.InputFiles.ToObject<List<string>>();
                    
                    // Create file list
                    var fileListPath = Path.Combine(Path.GetTempPath(), "NavisBatchFiles.txt");
                    // Use ASCII encoding for RevitBatchProcessor compatibility
                    using (var writer = new StreamWriter(fileListPath, false, System.Text.Encoding.ASCII))
                    {
                        foreach (var file in inputFiles)
                        {
                            writer.WriteLine(file);
                        }
                    }
                    
                    // Use the working Python script
                    var scriptDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts");
                    var pythonScript = Path.Combine(scriptDir, "WorkingExport.py");
                    
                    if (!File.Exists(pythonScript))
                    {
                        throw new FileNotFoundException($"Python script not found: {pythonScript}");
                    }
                    
                    // Run BatchRvt directly
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = rbpPath,
                        Arguments = $"--file_list \"{fileListPath}\" --task_script \"{pythonScript}\" --revit_version {config.RevitVersion} --log_folder \"{logDir}\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = false, // Show window to see what's happening
                        WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory,
                        StandardOutputEncoding = System.Text.Encoding.UTF8,
                        StandardErrorEncoding = System.Text.Encoding.UTF8
                    };
                    
                    // Set environment variables
                    startInfo.EnvironmentVariables["PYTHONIOENCODING"] = "utf-8";
                    
                    using (var process = Process.Start(startInfo))
                    {
                        var allOutput = new System.Text.StringBuilder();
                        var allErrors = new System.Text.StringBuilder();
                        bool processCompleted = false;
                        
                        process.OutputDataReceived += (s, e) =>
                        {
                            if (!string.IsNullOrEmpty(e.Data))
                            {
                                allOutput.AppendLine(e.Data);
                                System.Windows.Application.Current?.Dispatcher?.BeginInvoke(new Action(() =>
                                {
                                    StatusMessage = e.Data;
                                    Logger.Instance.Info(e.Data);
                                    
                                    // Update progress text in dialog
                                    ProgressText = e.Data;
                                    
                                    // Parse progress from BatchRvt output
                                    if (e.Data.Contains("Processing file") && e.Data.Contains(" of "))
                                    {
                                        var match = System.Text.RegularExpressions.Regex.Match(e.Data, @"(\d+) of (\d+)");
                                        if (match.Success)
                                        {
                                            var current = int.Parse(match.Groups[1].Value);
                                            var total = int.Parse(match.Groups[2].Value);
                                            Progress = (current * 100.0) / total;
                                            ProgressText = $"Processing file {current} of {total}";
                                        }
                                    }
                                    else if (e.Data.Contains("Opening file"))
                                    {
                                        Progress = 30;
                                        ProgressText = "Opening Revit file...";
                                    }
                                    else if (e.Data.Contains("Task script operation started"))
                                    {
                                        Progress = 60;
                                        ProgressText = "Exporting to Navisworks...";
                                    }
                                    else if (e.Data.Contains("Task script operation completed"))
                                    {
                                        Progress = 90;
                                        ProgressText = "Finalizing export...";
                                    }
                                    
                                    // Check for completion messages
                                    if (e.Data.Contains("Operation completed") || 
                                        e.Data.Contains("plain-text copy of the Log File has been saved"))
                                    {
                                        processCompleted = true;
                                        Progress = 100;
                                        ProgressText = "Conversion completed!";
                                    }
                                }));
                            }
                        };
                        
                        process.ErrorDataReceived += (s, e) =>
                        {
                            if (!string.IsNullOrEmpty(e.Data))
                            {
                                allErrors.AppendLine(e.Data);
                                Logger.Instance.Error(e.Data);
                            }
                        };
                        
                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();
                        
                        // Wait with timeout
                        if (!process.WaitForExit(300000)) // 5 minutes timeout
                        {
                            process.Kill();
                            throw new TimeoutException("BatchRvt process timed out after 5 minutes");
                        }
                        
                        // Give a moment for final output
                        System.Threading.Thread.Sleep(1000);
                        
                        if (process.ExitCode != 0 && !processCompleted)
                        {
                            var errorDetails = allErrors.Length > 0 ? allErrors.ToString() : "No error details available";
                            var outputDetails = allOutput.Length > 0 ? allOutput.ToString() : "No output available";
                            
                            Logger.Instance.Error($"BatchRvt Exit Code: {process.ExitCode}");
                            Logger.Instance.Error($"BatchRvt Errors:\n{errorDetails}");
                            Logger.Instance.Error($"BatchRvt Output:\n{outputDetails}");
                            
                            throw new Exception($"BatchRvt failed with exit code: {process.ExitCode}\n\nError Output:\n{errorDetails}\n\nStandard Output:\n{outputDetails}");
                        }
                    }
                    
                    // Clean up temp file
                    try { File.Delete(fileListPath); } catch { }
                    
                    System.Windows.Application.Current?.Dispatcher?.BeginInvoke(new Action(() =>
                    {
                        StatusMessage = "Conversion completed successfully!";
                        StatusIcon = "CheckCircle";
                        StatusColor = "Green";
                    }));
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to run RevitBatchProcessor: {ex.Message}", ex);
                }
            });
        }


        public void SaveSettings()
        {
            // TODO: Implement settings persistence
        }

        private void LoadSettings()
        {
            // TODO: Implement settings loading
        }

        private string FindRevitBatchProcessor()
        {
            var possiblePaths = new[]
            {
                @"C:\Users\BT\AppData\Local\RevitBatchProcessor\BatchRvt.exe",
                @"C:\Program Files (x86)\RevitBatchProcessor\RevitBatchProcessor.exe",
                @"C:\Program Files\RevitBatchProcessor\RevitBatchProcessor.exe",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"RevitBatchProcessor\RevitBatchProcessor.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"RevitBatchProcessor\RevitBatchProcessor.exe")
            };
            
            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }
            
            throw new FileNotFoundException("RevitBatchProcessor not found! Please install from: https://github.com/bvn-architecture/RevitBatchProcessor/releases");
        }

        // Property change notification
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Supporting classes
    public class WorksetFilter : INotifyPropertyChanged
    {
        private string _name;
        private bool _isIncluded;

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public bool IsIncluded
        {
            get => _isIncluded;
            set { _isIncluded = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Command implementation
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;
        public void Execute(object parameter) => _execute();
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Predicate<T> _canExecute;

        public RelayCommand(Action<T> execute, Predicate<T> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke((T)parameter) ?? true;
        public void Execute(object parameter) => _execute((T)parameter);
    }
}