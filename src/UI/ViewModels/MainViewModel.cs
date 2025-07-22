using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Linq;
using System.IO;
using Microsoft.Win32;
using System.Windows.Forms;

namespace RevitNavisworksAutomation.UI.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<ConversionJob> Files { get; }
        public ObservableCollection<string> RevitVersions { get; }
        public ObservableCollection<WorksetFilter> WorksetFilters { get; }

        // Commands
        public ICommand AddFilesCommand { get; }
        public ICommand AddFolderCommand { get; }
        public ICommand ClearAllCommand { get; }
        public ICommand StartConversionCommand { get; }
        public ICommand StopConversionCommand { get; }
        public ICommand RemoveFileCommand { get; }
        public ICommand ViewDetailsCommand { get; }
        public ICommand ViewLogsCommand { get; }
        public ICommand BrowseOutputDirectoryCommand { get; }
        public ICommand AddWorksetCommand { get; }

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
                        Files.Add(new ConversionJob(file));
                    }
                }
                OnPropertyChanged(nameof(FileCount));
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
                        Files.Add(new ConversionJob(file));
                    }
                }
                OnPropertyChanged(nameof(FileCount));
            }
        }

        private void StartConversion()
        {
            IsProcessing = true;
            StatusMessage = "Starting conversion...";
            StatusIcon = "Progress";
            StatusColor = "Orange";
            ShowProgressDialog = true;

            // TODO: Implement actual conversion logic
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

        public void SaveSettings()
        {
            // TODO: Implement settings persistence
        }

        private void LoadSettings()
        {
            // TODO: Implement settings loading
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

    public class ConversionJob : INotifyPropertyChanged
    {
        private bool _isSelected = true;
        private string _status = "Pending";
        private double _progress;

        public string FilePath { get; }
        public string FileName => Path.GetFileName(FilePath);
        public string FileSize { get; }

        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }

        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(); }
        }

        public double Progress
        {
            get => _progress;
            set { _progress = value; OnPropertyChanged(); }
        }

        public ConversionJob(string filePath)
        {
            FilePath = filePath;
            if (File.Exists(filePath))
            {
                var info = new FileInfo(filePath);
                FileSize = FormatFileSize(info.Length);
            }
            else
            {
                FileSize = "Unknown";
            }
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
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