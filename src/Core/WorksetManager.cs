using System;
using System.Linq;
using System.Collections.Generic;
using Autodesk.Revit.DB;

namespace RevitNavisworksAutomation.Core
{
    /// <summary>
    /// Manages workset filtering and visibility for Navisworks export
    /// </summary>
    public static class WorksetManager
    {
        private static Logger _logger = Logger.Instance;
        
        /// <summary>
        /// Apply workset filters to control which worksets are included in the export
        /// </summary>
        public static void ApplyWorksetFilter(Document doc, List<string> includePatterns)
        {
            if (!doc.IsWorkshared)
            {
                _logger.Warning("Document is not workshared. Skipping workset filtering.");
                return;
            }
            
            try
            {
                using (var trans = new Transaction(doc, "Configure Worksets for Export"))
                {
                    trans.Start();
                    
                    try
                    {
                        // Get all user worksets
                        var allWorksets = new FilteredWorksetCollector(doc)
                            .OfKind(WorksetKind.UserWorkset)
                            .ToList();
                        
                        _logger.Info($"Found {allWorksets.Count} worksets in document");
                        
                        // Configure workset visibility for export
                        var worksetTable = doc.GetWorksetTable();
                        var activeWSId = worksetTable.GetActiveWorksetId();
                        
                        foreach (var workset in allWorksets)
                        {
                            bool shouldInclude = ShouldIncludeWorkset(workset.Name, includePatterns);
                            
                            // Log workset status
                            _logger.Debug($"Workset '{workset.Name}' - Include: {shouldInclude}");
                            
                            // Set default visibility for new views
                            var defaultVisibility = shouldInclude ? 
                                WorksetDefaultVisibilitySettings.Visible : 
                                WorksetDefaultVisibilitySettings.Hidden;
                            
                            worksetTable.SetWorksetDefaultVisibilitySettings(workset.Id, defaultVisibility);
                            
                            // Apply to all 3D views
                            ApplyWorksetVisibilityToViews(doc, workset.Id, shouldInclude);
                        }
                        
                        trans.Commit();
                        _logger.Success("Workset filtering applied successfully");
                    }
                    catch (Exception ex)
                    {
                        trans.RollBack();
                        throw new InvalidOperationException($"Failed to configure worksets: {ex.Message}", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error applying workset filter", ex);
                throw;
            }
        }
        
        /// <summary>
        /// Apply workset visibility to all 3D views
        /// </summary>
        private static void ApplyWorksetVisibilityToViews(Document doc, WorksetId worksetId, bool visible)
        {
            try
            {
                // Get all 3D views
                var views3D = new FilteredElementCollector(doc)
                    .OfClass(typeof(View3D))
                    .Cast<View3D>()
                    .Where(v => !v.IsTemplate && v.CanModifyViewSpecificElements());
                
                foreach (var view in views3D)
                {
                    try
                    {
                        // Skip views that don't support workset visibility
                        if (!view.CanModifyWorksetVisibility())
                        {
                            continue;
                        }
                        
                        // Set workset visibility for the view
                        if (visible)
                        {
                            view.SetWorksetVisibility(worksetId, WorksetVisibility.Visible);
                        }
                        else
                        {
                            view.SetWorksetVisibility(worksetId, WorksetVisibility.Hidden);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Debug($"Could not set workset visibility for view '{view.Name}': {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error applying workset visibility to views", ex);
            }
        }
        
        /// <summary>
        /// Determine if a workset should be included based on patterns
        /// </summary>
        private static bool ShouldIncludeWorkset(string worksetName, List<string> includePatterns)
        {
            if (includePatterns == null || !includePatterns.Any())
            {
                return true; // Include all if no patterns specified
            }
            
            // Check if workset name matches any include pattern
            return includePatterns.Any(pattern => 
                MatchesPattern(worksetName, pattern));
        }
        
        /// <summary>
        /// Check if workset name matches pattern (supports wildcards)
        /// </summary>
        private static bool MatchesPattern(string worksetName, string pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern))
            {
                return false;
            }
            
            // Support wildcards
            if (pattern.Contains("*") || pattern.Contains("?"))
            {
                var regexPattern = "^" + System.Text.RegularExpressions.Regex.Escape(pattern)
                    .Replace("\\*", ".*")
                    .Replace("\\?", ".") + "$";
                
                return System.Text.RegularExpressions.Regex.IsMatch(
                    worksetName, 
                    regexPattern, 
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            }
            
            // Simple case-insensitive contains check
            return worksetName.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0;
        }
        
        /// <summary>
        /// Get current workset status for all worksets
        /// </summary>
        public static Dictionary<string, WorksetInfo> GetWorksetStatus(Document doc)
        {
            var result = new Dictionary<string, WorksetInfo>();
            
            if (!doc.IsWorkshared)
            {
                return result;
            }
            
            try
            {
                var worksets = new FilteredWorksetCollector(doc)
                    .OfKind(WorksetKind.UserWorkset);
                
                foreach (var workset in worksets)
                {
                    var info = new WorksetInfo
                    {
                        Id = workset.Id,
                        Name = workset.Name,
                        IsOpen = workset.IsOpen,
                        IsEditable = workset.IsEditable,
                        Owner = workset.Owner,
                        UniqueId = workset.UniqueId
                    };
                    
                    result[workset.Name] = info;
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting workset status", ex);
            }
            
            return result;
        }
        
        /// <summary>
        /// Get all workset names in the document
        /// </summary>
        public static List<string> GetAllWorksetNames(Document doc)
        {
            if (!doc.IsWorkshared)
            {
                return new List<string>();
            }
            
            try
            {
                return new FilteredWorksetCollector(doc)
                    .OfKind(WorksetKind.UserWorkset)
                    .Select(w => w.Name)
                    .OrderBy(name => name)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting workset names", ex);
                return new List<string>();
            }
        }
        
        /// <summary>
        /// Open all worksets for processing
        /// </summary>
        public static void OpenAllWorksets(Document doc)
        {
            if (!doc.IsWorkshared)
            {
                return;
            }
            
            try
            {
                using (var trans = new Transaction(doc, "Open All Worksets"))
                {
                    trans.Start();
                    
                    var closedWorksets = new FilteredWorksetCollector(doc)
                        .OfKind(WorksetKind.UserWorkset)
                        .Where(w => !w.IsOpen)
                        .Select(w => w.Id)
                        .ToList();
                    
                    if (closedWorksets.Any())
                    {
                        _logger.Info($"Opening {closedWorksets.Count} closed worksets");
                        WorksetTable.OpenWorksets(doc, closedWorksets);
                    }
                    
                    trans.Commit();
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error opening worksets", ex);
            }
        }
        
        /// <summary>
        /// Create a workset configuration for the current document
        /// </summary>
        public static WorksetConfiguration CreateConfiguration(Document doc, List<string> includePatterns)
        {
            var config = new WorksetConfiguration();
            
            if (!doc.IsWorkshared)
            {
                config.IsWorkshared = false;
                return config;
            }
            
            config.IsWorkshared = true;
            var allWorksets = GetWorksetStatus(doc);
            
            foreach (var kvp in allWorksets)
            {
                var worksetConfig = new WorksetConfigItem
                {
                    Name = kvp.Key,
                    IsIncluded = ShouldIncludeWorkset(kvp.Key, includePatterns),
                    IsOpen = kvp.Value.IsOpen,
                    IsEditable = kvp.Value.IsEditable
                };
                
                config.Worksets.Add(worksetConfig);
            }
            
            return config;
        }
    }
    
    /// <summary>
    /// Workset information
    /// </summary>
    public class WorksetInfo
    {
        public WorksetId Id { get; set; }
        public string Name { get; set; }
        public bool IsOpen { get; set; }
        public bool IsEditable { get; set; }
        public string Owner { get; set; }
        public Guid UniqueId { get; set; }
    }
    
    /// <summary>
    /// Workset configuration
    /// </summary>
    public class WorksetConfiguration
    {
        public bool IsWorkshared { get; set; }
        public List<WorksetConfigItem> Worksets { get; set; } = new List<WorksetConfigItem>();
        
        public int IncludedCount => Worksets.Count(w => w.IsIncluded);
        public int ExcludedCount => Worksets.Count(w => !w.IsIncluded);
    }
    
    /// <summary>
    /// Individual workset configuration item
    /// </summary>
    public class WorksetConfigItem
    {
        public string Name { get; set; }
        public bool IsIncluded { get; set; }
        public bool IsOpen { get; set; }
        public bool IsEditable { get; set; }
    }
}