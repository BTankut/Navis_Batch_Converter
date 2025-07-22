using System;
using System.Collections.Generic;

namespace RevitNavisworksAutomation.Models
{
    public class ConversionResult
    {
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public ConversionStatus Status { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration => EndTime - StartTime;
        
        public List<string> OutputFiles { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
        public List<string> Errors { get; set; } = new List<string>();
        
        public string ErrorMessage { get; set; }
        public Exception Exception { get; set; }
        
        public int ViewsProcessed { get; set; }
        public int ViewsExported { get; set; }
        public long FileSizeBytes { get; set; }
        public long OutputSizeBytes { get; set; }
        
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
        
        public bool IsSuccess => Status == ConversionStatus.Completed;
        public bool HasWarnings => Warnings.Count > 0;
        public bool HasErrors => Errors.Count > 0;
        
        public string GetSummary()
        {
            var summary = $"{FileName}: {Status}";
            
            if (IsSuccess)
            {
                summary += $" - {ViewsExported} views exported in {Duration.TotalSeconds:F1}s";
            }
            else if (HasErrors)
            {
                summary += $" - {Errors.Count} errors";
            }
            
            return summary;
        }
    }
    
    public enum ConversionStatus
    {
        Pending,
        Preparing,
        Processing,
        PostProcessing,
        Completed,
        Failed,
        Cancelled,
        Skipped
    }
    
    public class ConversionProgress
    {
        public ConversionJob CurrentJob { get; set; }
        public string CurrentOperation { get; set; }
        public ConversionStatus Status { get; set; }
        public double FilePercentage { get; set; }
        public double OverallPercentage { get; set; }
        public int FilesCompleted { get; set; }
        public int TotalFiles { get; set; }
        public TimeSpan ElapsedTime { get; set; }
        public TimeSpan EstimatedTimeRemaining { get; set; }
        
        public string GetProgressText()
        {
            return $"Processing {FilesCompleted + 1} of {TotalFiles} - {OverallPercentage:F1}%";
        }
    }
    
    public class BatchConversionResult
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan TotalDuration => EndTime - StartTime;
        
        public List<ConversionResult> Results { get; set; } = new List<ConversionResult>();
        
        public int TotalFiles => Results.Count;
        public int SuccessfulFiles => Results.Count(r => r.IsSuccess);
        public int FailedFiles => Results.Count(r => r.Status == ConversionStatus.Failed);
        public int SkippedFiles => Results.Count(r => r.Status == ConversionStatus.Skipped);
        
        public double SuccessRate => TotalFiles > 0 ? (SuccessfulFiles * 100.0 / TotalFiles) : 0;
        
        public string NWDFilePath { get; set; }
        public bool NWDCreated { get; set; }
        
        public string GetSummary()
        {
            return $"Batch conversion completed: {SuccessfulFiles}/{TotalFiles} successful ({SuccessRate:F1}%) in {TotalDuration.TotalMinutes:F1} minutes";
        }
    }
}