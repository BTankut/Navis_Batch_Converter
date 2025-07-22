using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace RevitNavisworksAutomation.Models
{
    public class ConversionJob : INotifyPropertyChanged
    {
        private bool _isSelected = true;
        private ConversionStatus _status = ConversionStatus.Pending;
        private double _progress;
        private string _statusMessage;
        private DateTime? _startTime;
        private DateTime? _endTime;
        
        public string Id { get; } = Guid.NewGuid().ToString();
        public string FilePath { get; }
        public string FileName => Path.GetFileName(FilePath);
        public string Directory => Path.GetDirectoryName(FilePath);
        public string FileSize { get; }
        public long FileSizeBytes { get; }
        
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }
        
        public ConversionStatus Status
        {
            get => _status;
            set 
            { 
                _status = value; 
                OnPropertyChanged();
                OnPropertyChanged(nameof(StatusIcon));
                OnPropertyChanged(nameof(StatusColor));
            }
        }
        
        public double Progress
        {
            get => _progress;
            set { _progress = value; OnPropertyChanged(); }
        }
        
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }
        
        public DateTime? StartTime
        {
            get => _startTime;
            set 
            { 
                _startTime = value; 
                OnPropertyChanged();
                OnPropertyChanged(nameof(Duration));
            }
        }
        
        public DateTime? EndTime
        {
            get => _endTime;
            set 
            { 
                _endTime = value; 
                OnPropertyChanged();
                OnPropertyChanged(nameof(Duration));
            }
        }
        
        public TimeSpan? Duration
        {
            get
            {
                if (StartTime.HasValue && EndTime.HasValue)
                    return EndTime.Value - StartTime.Value;
                else if (StartTime.HasValue)
                    return DateTime.Now - StartTime.Value;
                else
                    return null;
            }
        }
        
        public string StatusIcon
        {
            get
            {
                switch (Status)
                {
                    case ConversionStatus.Pending:
                        return "Clock";
                    case ConversionStatus.Preparing:
                    case ConversionStatus.Processing:
                    case ConversionStatus.PostProcessing:
                        return "Progress";
                    case ConversionStatus.Completed:
                        return "CheckCircle";
                    case ConversionStatus.Failed:
                        return "AlertCircle";
                    case ConversionStatus.Cancelled:
                        return "Cancel";
                    case ConversionStatus.Skipped:
                        return "SkipNext";
                    default:
                        return "HelpCircle";
                }
            }
        }
        
        public string StatusColor
        {
            get
            {
                switch (Status)
                {
                    case ConversionStatus.Pending:
                        return "Gray";
                    case ConversionStatus.Preparing:
                    case ConversionStatus.Processing:
                    case ConversionStatus.PostProcessing:
                        return "Orange";
                    case ConversionStatus.Completed:
                        return "Green";
                    case ConversionStatus.Failed:
                        return "Red";
                    case ConversionStatus.Cancelled:
                        return "DarkGray";
                    case ConversionStatus.Skipped:
                        return "Blue";
                    default:
                        return "Black";
                }
            }
        }
        
        public ConversionResult Result { get; set; }
        
        public ConversionJob(string filePath)
        {
            FilePath = filePath;
            
            if (File.Exists(filePath))
            {
                var info = new FileInfo(filePath);
                FileSizeBytes = info.Length;
                FileSize = FormatFileSize(info.Length);
            }
            else
            {
                FileSize = "Unknown";
                FileSizeBytes = 0;
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
        
        public void UpdateProgress(double percentage, string message = null)
        {
            Progress = Math.Max(0, Math.Min(100, percentage));
            if (!string.IsNullOrEmpty(message))
            {
                StatusMessage = message;
            }
        }
        
        public void MarkAsStarted()
        {
            StartTime = DateTime.Now;
            Status = ConversionStatus.Processing;
            StatusMessage = "Processing...";
        }
        
        public void MarkAsCompleted(ConversionResult result = null)
        {
            EndTime = DateTime.Now;
            Status = ConversionStatus.Completed;
            Progress = 100;
            Result = result;
            StatusMessage = result?.GetSummary() ?? "Completed";
        }
        
        public void MarkAsFailed(string errorMessage, Exception ex = null)
        {
            EndTime = DateTime.Now;
            Status = ConversionStatus.Failed;
            StatusMessage = errorMessage;
            
            if (Result == null)
            {
                Result = new ConversionResult
                {
                    FilePath = FilePath,
                    FileName = FileName,
                    Status = ConversionStatus.Failed,
                    ErrorMessage = errorMessage,
                    Exception = ex
                };
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}