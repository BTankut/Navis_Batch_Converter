using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.ApplicationServices;
using RevitNavisworksAutomation.Models;

namespace RevitNavisworksAutomation.Core
{
    /// <summary>
    /// Main export task that will be executed by RevitBatchProcessor
    /// This class contains the logic to export Revit views to Navisworks format
    /// </summary>
    public class RevitExportTask
    {
        private static Logger _logger = Logger.Instance;
        private static ConversionSettings _config;
        private static string _revitFilePath;
        
        /// <summary>
        /// Entry point for RevitBatchProcessor
        /// This method is called for each Revit file in the batch
        /// </summary>
        public static void Execute(Document doc, string revitFilePath)
        {
            _revitFilePath = revitFilePath;
            
            try
            {
                // Load configuration
                _config = ConversionSettings.Load();
                _config.Validate();
                
                _logger.Initialize(revitFilePath);
                _logger.Info($"Starting export for: {Path.GetFileName(revitFilePath)}");
                _logger.Info($"Document title: {doc.Title}");
                _logger.Info($"Revit version: {doc.Application.VersionName}");
                
                // Check if document is valid
                if (doc.IsFamilyDocument)
                {
                    _logger.Warning("Document is a family file. Skipping export.");
                    return;
                }
                
                // Step 1: Find eligible views
                var targetViews = ViewSelector.GetNavisworksViews(doc, _config.ViewFilter);
                
                if (!targetViews.Any())
                {
                    _logger.Warning("No eligible views found matching criteria. Skipping file.");
                    return;
                }
                
                _logger.Info($"Found {targetViews.Count} eligible views for export");
                
                // Step 2: Apply workset filtering if applicable
                if (doc.IsWorkshared && _config.WorksetSettings.FilterWorksets)
                {
                    _logger.Info("Applying workset filters...");
                    WorksetManager.ApplyWorksetFilter(doc, _config.WorksetSettings.IncludeWorksets);
                }
                
                // Step 3: Export each view
                int successCount = 0;
                int failCount = 0;
                
                foreach (var view in targetViews)
                {
                    try
                    {
                        _logger.Info($"Exporting view: {view.Name}");
                        
                        if (ExportViewToNWC(doc, view, revitFilePath))
                        {
                            successCount++;
                        }
                        else
                        {
                            failCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Failed to export view '{view.Name}'", ex);
                        failCount++;
                    }
                }
                
                _logger.Success($"Export completed for: {Path.GetFileName(revitFilePath)}");
                _logger.Info($"Views exported: {successCount}/{targetViews.Count} (Failed: {failCount})");
                
                // Write result file for the main application to read
                WriteResultFile(revitFilePath, successCount, failCount, targetViews.Count);
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleExportError(ex, revitFilePath);
                WriteErrorFile(revitFilePath, ex);
            }
        }
        
        /// <summary>
        /// Export a single 3D view to NWC format
        /// </summary>
        private static bool ExportViewToNWC(Document doc, View3D view, string revitFilePath)
        {
            try
            {
                // Create Navisworks export options
                var options = new NavisworksExportOptions
                {
                    ExportScope = NavisworksExportScope.View,
                    ViewId = new ViewId { IntegerValue = view.Id.IntegerValue },
                    ExportLinks = _config.ExportOptions.ExportLinks,
                    ExportParts = _config.ExportOptions.ExportParts,
                    ExportRoomGeometry = false,
                    Coordinates = _config.ExportOptions.UseSharedCoordinates ? 
                        NavisworksCoordinates.Shared : NavisworksCoordinates.Internal,
                    ExportElementIds = true,
                    ConvertElementProperties = _config.ExportOptions.ConvertElementProperties,
                    ExportUrls = false,
                    FindMissingMaterials = _config.ExportOptions.FindMissingMaterials,
                    DivideFileIntoLevels = false,
                    ExportRoomAsAttribute = _config.ExportOptions.ExportRoomAsAttribute
                };
                
                // Generate output file name
                string outputPath = _config.OutputDirectory;
                string fileName = GenerateFileName(revitFilePath, view.Name);
                
                // Ensure output directory exists
                Directory.CreateDirectory(outputPath);
                
                // Export the view
                _logger.Debug($"Exporting to: {Path.Combine(outputPath, fileName)}.nwc");
                
                var result = doc.Export(outputPath, fileName, options);
                
                if (result)
                {
                    _logger.Info($"Successfully exported view '{view.Name}' to {fileName}.nwc");
                    
                    // Log file size
                    var nwcPath = Path.Combine(outputPath, fileName + ".nwc");
                    if (File.Exists(nwcPath))
                    {
                        var fileInfo = new FileInfo(nwcPath);
                        _logger.Debug($"Output file size: {fileInfo.Length / 1024 / 1024:F2} MB");
                    }
                }
                else
                {
                    _logger.Warning($"Export returned false for view '{view.Name}'");
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error($"Exception during export of view '{view.Name}'", ex);
                return false;
            }
        }
        
        /// <summary>
        /// Generate a unique filename for the NWC export
        /// </summary>
        private static string GenerateFileName(string revitPath, string viewName)
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var baseName = Path.GetFileNameWithoutExtension(revitPath);
            var safeName = SanitizeFileName(viewName);
            
            // Limit filename length to avoid path issues
            if (baseName.Length > 50) baseName = baseName.Substring(0, 50);
            if (safeName.Length > 50) safeName = safeName.Substring(0, 50);
            
            return $"{baseName}_{safeName}_{timestamp}";
        }
        
        /// <summary>
        /// Remove invalid characters from filename
        /// </summary>
        private static string SanitizeFileName(string fileName)
        {
            var invalid = Path.GetInvalidFileNameChars();
            var sanitized = string.Join("_", fileName.Split(invalid, StringSplitOptions.RemoveEmptyEntries));
            
            // Replace common problematic characters
            sanitized = sanitized.Replace(" ", "_")
                                 .Replace(".", "_")
                                 .Replace(",", "_")
                                 .Replace(":", "_")
                                 .Replace(";", "_")
                                 .Replace("'", "")
                                 .Replace("\"", "")
                                 .Replace("/", "_")
                                 .Replace("\\", "_");
            
            return sanitized;
        }
        
        /// <summary>
        /// Write result file for main application to read
        /// </summary>
        private static void WriteResultFile(string revitFilePath, int successCount, int failCount, int totalViews)
        {
            try
            {
                var resultPath = Path.Combine(
                    _config.OutputDirectory, 
                    Path.GetFileNameWithoutExtension(revitFilePath) + "_result.json"
                );
                
                var result = new
                {
                    FilePath = revitFilePath,
                    FileName = Path.GetFileName(revitFilePath),
                    Timestamp = DateTime.Now,
                    Success = failCount == 0,
                    ViewsProcessed = totalViews,
                    ViewsExported = successCount,
                    ViewsFailed = failCount,
                    LogFile = _logger.LogFilePath
                };
                
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(result, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(resultPath, json);
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to write result file", ex);
            }
        }
        
        /// <summary>
        /// Write error file for main application to read
        /// </summary>
        private static void WriteErrorFile(string revitFilePath, Exception ex)
        {
            try
            {
                var errorPath = Path.Combine(
                    _config.OutputDirectory, 
                    Path.GetFileNameWithoutExtension(revitFilePath) + "_error.json"
                );
                
                var error = new
                {
                    FilePath = revitFilePath,
                    FileName = Path.GetFileName(revitFilePath),
                    Timestamp = DateTime.Now,
                    Success = false,
                    ErrorType = ex.GetType().Name,
                    ErrorMessage = ex.Message,
                    StackTrace = ex.StackTrace,
                    LogFile = _logger.LogFilePath
                };
                
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(error, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(errorPath, json);
            }
            catch
            {
                // Fail silently - we've already logged the main error
            }
        }
    }
}