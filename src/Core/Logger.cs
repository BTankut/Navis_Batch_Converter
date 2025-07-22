using System;
using System.IO;
using System.Text;
using System.Threading;

namespace RevitNavisworksAutomation.Core
{
    public class Logger
    {
        private static Logger _instance;
        private static readonly object _lock = new object();
        
        private readonly string _logDirectory;
        private readonly string _logFileName;
        private readonly StreamWriter _writer;
        private readonly StringBuilder _buffer;
        private readonly Timer _flushTimer;
        
        public static Logger Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new Logger();
                        }
                    }
                }
                return _instance;
            }
        }
        
        public string LogDirectory => _logDirectory;
        public string LogFilePath { get; private set; }
        
        private Logger()
        {
            _logDirectory = Path.Combine(Environment.CurrentDirectory, "logs");
            Directory.CreateDirectory(_logDirectory);
            
            _logFileName = $"conversion_{DateTime.Now:yyyyMMdd_HHmmss}.log";
            LogFilePath = Path.Combine(_logDirectory, _logFileName);
            
            _writer = new StreamWriter(LogFilePath, true, Encoding.UTF8);
            _buffer = new StringBuilder();
            
            // Flush buffer every 5 seconds
            _flushTimer = new Timer(FlushBuffer, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
            
            WriteHeader();
        }
        
        private void WriteHeader()
        {
            _writer.WriteLine("=====================================");
            _writer.WriteLine($"Navis Batch Converter Log");
            _writer.WriteLine($"Started: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            _writer.WriteLine("=====================================");
            _writer.WriteLine();
            _writer.Flush();
        }
        
        public void Info(string message)
        {
            Log("INFO", message);
        }
        
        public void Warning(string message)
        {
            Log("WARN", message);
        }
        
        public void Error(string message, Exception ex = null)
        {
            var errorMessage = ex != null ? $"{message} - {ex.Message}" : message;
            Log("ERROR", errorMessage);
            
            if (ex != null)
            {
                Log("ERROR", $"Stack Trace: {ex.StackTrace}");
            }
        }
        
        public void Success(string message)
        {
            Log("SUCCESS", message);
        }
        
        public void Debug(string message)
        {
            #if DEBUG
            Log("DEBUG", message);
            #endif
        }
        
        private void Log(string level, string message)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var logEntry = $"[{timestamp}] [{level,-7}] {message}";
            
            lock (_buffer)
            {
                _buffer.AppendLine(logEntry);
            }
            
            // Also write to console for debugging
            Console.WriteLine(logEntry);
        }
        
        private void FlushBuffer(object state)
        {
            lock (_buffer)
            {
                if (_buffer.Length > 0)
                {
                    _writer.Write(_buffer.ToString());
                    _writer.Flush();
                    _buffer.Clear();
                }
            }
        }
        
        public void Initialize(string contextInfo)
        {
            Info($"Initializing logger for: {contextInfo}");
        }
        
        public void Close()
        {
            _flushTimer?.Dispose();
            FlushBuffer(null);
            
            _writer.WriteLine();
            _writer.WriteLine("=====================================");
            _writer.WriteLine($"Finished: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            _writer.WriteLine("=====================================");
            
            _writer.Close();
            _writer.Dispose();
        }
        
        ~Logger()
        {
            Close();
        }
    }
    
    public static class LoggerExtensions
    {
        public static void LogProgress(this Logger logger, string operation, int current, int total)
        {
            var percentage = total > 0 ? (current * 100.0 / total) : 0;
            logger.Info($"{operation}: {current}/{total} ({percentage:F1}%)");
        }
        
        public static void LogDuration(this Logger logger, string operation, TimeSpan duration)
        {
            logger.Info($"{operation} completed in {duration.TotalSeconds:F2} seconds");
        }
    }
}