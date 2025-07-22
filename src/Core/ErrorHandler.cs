using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RevitNavisworksAutomation.Core
{
    public static class ErrorHandler
    {
        private static readonly Logger _logger = Logger.Instance;
        private static readonly Dictionary<string, int> _errorCounts = new Dictionary<string, int>();
        private static readonly List<ErrorRecord> _errorHistory = new List<ErrorRecord>();
        
        public static void HandleExportError(Exception ex, string filePath)
        {
            var errorKey = GetErrorKey(ex);
            
            // Track error frequency
            if (_errorCounts.ContainsKey(errorKey))
            {
                _errorCounts[errorKey]++;
            }
            else
            {
                _errorCounts[errorKey] = 1;
            }
            
            // Log the error
            _logger.Error($"Export failed for {Path.GetFileName(filePath)}", ex);
            
            // Record error history
            _errorHistory.Add(new ErrorRecord
            {
                Timestamp = DateTime.Now,
                FilePath = filePath,
                ErrorType = ex.GetType().Name,
                Message = ex.Message,
                StackTrace = ex.StackTrace
            });
            
            // Determine if we should retry
            var shouldRetry = DetermineRetryStrategy(ex, filePath);
            
            if (shouldRetry)
            {
                _logger.Info($"Error is recoverable. Will retry {Path.GetFileName(filePath)}");
            }
            else
            {
                _logger.Warning($"Error is not recoverable. Skipping {Path.GetFileName(filePath)}");
            }
        }
        
        private static string GetErrorKey(Exception ex)
        {
            return $"{ex.GetType().Name}:{ex.Message?.Split('\n')[0]}";
        }
        
        private static bool DetermineRetryStrategy(Exception ex, string filePath)
        {
            // Check if it's a known recoverable error
            var recoverableErrors = new[]
            {
                "System.IO.IOException",
                "System.UnauthorizedAccessException",
                "System.OutOfMemoryException"
            };
            
            if (recoverableErrors.Contains(ex.GetType().FullName))
            {
                var errorCount = _errorHistory.Count(e => e.FilePath == filePath);
                return errorCount < 3; // Retry up to 3 times
            }
            
            // Check for specific Revit API errors
            if (ex.Message.Contains("Central file") || ex.Message.Contains("workshared"))
            {
                return true; // Retry worksharing issues
            }
            
            if (ex.Message.Contains("corrupt") || ex.Message.Contains("damaged"))
            {
                return false; // Don't retry corrupt files
            }
            
            // Default: don't retry unknown errors
            return false;
        }
        
        public static void GenerateErrorReport(string outputPath)
        {
            var report = new System.Text.StringBuilder();
            
            report.AppendLine("Error Summary Report");
            report.AppendLine("===================");
            report.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine();
            
            // Error frequency
            report.AppendLine("Error Frequency:");
            foreach (var kvp in _errorCounts.OrderByDescending(x => x.Value))
            {
                report.AppendLine($"  {kvp.Key}: {kvp.Value} occurrences");
            }
            report.AppendLine();
            
            // Failed files
            report.AppendLine("Failed Files:");
            var failedFiles = _errorHistory.Select(e => e.FilePath).Distinct();
            foreach (var file in failedFiles)
            {
                report.AppendLine($"  - {Path.GetFileName(file)}");
                var errors = _errorHistory.Where(e => e.FilePath == file);
                foreach (var error in errors)
                {
                    report.AppendLine($"    [{error.Timestamp:HH:mm:ss}] {error.ErrorType}: {error.Message}");
                }
            }
            
            File.WriteAllText(outputPath, report.ToString());
            _logger.Info($"Error report generated: {outputPath}");
        }
        
        public static void Reset()
        {
            _errorCounts.Clear();
            _errorHistory.Clear();
        }
        
        public static int GetErrorCount(string filePath = null)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return _errorHistory.Count;
            }
            
            return _errorHistory.Count(e => e.FilePath == filePath);
        }
        
        public static bool HasRecoverableError(string filePath)
        {
            var lastError = _errorHistory.LastOrDefault(e => e.FilePath == filePath);
            if (lastError == null) return false;
            
            var recoverableTypes = new[] { "IOException", "UnauthorizedAccessException", "OutOfMemoryException" };
            return recoverableTypes.Any(t => lastError.ErrorType.Contains(t));
        }
    }
    
    internal class ErrorRecord
    {
        public DateTime Timestamp { get; set; }
        public string FilePath { get; set; }
        public string ErrorType { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
    }
}