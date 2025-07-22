using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace RevitNavisworksAutomation.Models
{
    public class ConversionSettings
    {
        public string RevitVersion { get; set; } = "2022";
        public string OutputDirectory { get; set; } = @"C:\Output\Navisworks";
        public string LogDirectory { get; set; } = @"C:\Output\Logs";
        
        public ViewFilterConfig ViewFilter { get; set; } = new ViewFilterConfig();
        public WorksetSettings WorksetSettings { get; set; } = new WorksetSettings();
        public ExportOptions ExportOptions { get; set; } = new ExportOptions();
        public ProcessingOptions Processing { get; set; } = new ProcessingOptions();
        public PostProcessingOptions PostProcessing { get; set; } = new PostProcessingOptions();
        
        public static ConversionSettings Load(string configPath = null)
        {
            if (string.IsNullOrEmpty(configPath))
            {
                configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts", "Config.json");
            }
            
            if (File.Exists(configPath))
            {
                var json = File.ReadAllText(configPath);
                return JsonConvert.DeserializeObject<ConversionSettings>(json);
            }
            
            return new ConversionSettings();
        }
        
        public void Save(string configPath = null)
        {
            if (string.IsNullOrEmpty(configPath))
            {
                configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts", "Config.json");
            }
            
            var json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(configPath, json);
        }
        
        public void Validate()
        {
            // Ensure directories exist
            if (!Directory.Exists(OutputDirectory))
            {
                Directory.CreateDirectory(OutputDirectory);
            }
            
            if (!Directory.Exists(LogDirectory))
            {
                Directory.CreateDirectory(LogDirectory);
            }
            
            // Validate settings
            if (Processing.MaxParallelFiles < 1)
            {
                Processing.MaxParallelFiles = 1;
            }
            
            if (Processing.TimeoutMinutesPerFile < 1)
            {
                Processing.TimeoutMinutesPerFile = 30;
            }
        }
    }
    
    public class ViewFilterConfig
    {
        public string Pattern { get; set; } = "navis_view";
        public bool CaseSensitive { get; set; } = false;
        public bool ExcludeTemplates { get; set; } = true;
        public bool RequireSheetAssociation { get; set; } = false;
    }
    
    public class WorksetSettings
    {
        public bool FilterWorksets { get; set; } = true;
        public List<string> IncludeWorksets { get; set; } = new List<string> { "Architecture", "Structure", "MEP" };
        public List<string> ExcludeWorksets { get; set; } = new List<string> { "Coordination", "Linked Files" };
    }
    
    public class ExportOptions
    {
        public bool ExportLinks { get; set; } = true;
        public bool ExportParts { get; set; } = true;
        public bool UseSharedCoordinates { get; set; } = true;
        public bool ConvertUnits { get; set; } = false;
        public bool ConvertElementProperties { get; set; } = true;
        public bool ExportRoomAsAttribute { get; set; } = true;
        public bool FindMissingMaterials { get; set; } = true;
    }
    
    public class ProcessingOptions
    {
        public int MaxRetries { get; set; } = 3;
        public int RetryDelaySeconds { get; set; } = 5;
        public int BatchSize { get; set; } = 50;
        public int MaxParallelFiles { get; set; } = 1;
        public int TimeoutMinutesPerFile { get; set; } = 30;
    }
    
    public class PostProcessingOptions
    {
        public bool CombineToNWD { get; set; } = true;
        public bool DeleteNWCAfterCombine { get; set; } = false;
        public bool CompressNWD { get; set; } = true;
    }
}