using System;
using System.Linq;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using RevitNavisworksAutomation.Models;

namespace RevitNavisworksAutomation.Core
{
    /// <summary>
    /// Handles selection and filtering of 3D views for Navisworks export
    /// </summary>
    public static class ViewSelector
    {
        private static Logger _logger = Logger.Instance;
        
        /// <summary>
        /// Get all 3D views that match the specified filter criteria
        /// </summary>
        public static List<View3D> GetNavisworksViews(Document doc, ViewFilterConfig config)
        {
            try
            {
                _logger.Info("Searching for eligible 3D views...");
                
                // Get all 3D views in the document
                var collector = new FilteredElementCollector(doc)
                    .OfClass(typeof(View3D))
                    .Cast<View3D>();
                
                // Apply filters
                var filteredViews = collector.Where(view => 
                    IsEligibleView(view, config) &&
                    MatchesPattern(view.Name, config.Pattern, config.CaseSensitive) &&
                    (!config.RequireSheetAssociation || IsOnSheet(doc, view))
                ).ToList();
                
                _logger.Info($"Found {filteredViews.Count} views matching criteria");
                
                // Log detailed view information
                foreach (var view in filteredViews)
                {
                    var info = GetViewInfo(doc, view);
                    _logger.Debug($"View: {view.Name} | Type: {info.ViewType} | Level: {info.Level} | Phase: {info.Phase}");
                }
                
                return filteredViews;
            }
            catch (Exception ex)
            {
                _logger.Error("Error while selecting views", ex);
                return new List<View3D>();
            }
        }
        
        /// <summary>
        /// Check if a view is eligible for export
        /// </summary>
        private static bool IsEligibleView(View3D view, ViewFilterConfig config)
        {
            // Skip view templates if configured
            if (config.ExcludeTemplates && view.IsTemplate)
            {
                return false;
            }
            
            // Skip certain system views
            if (IsSystemView(view))
            {
                return false;
            }
            
            // Check if view is valid and can be exported
            if (!view.CanBePrinted)
            {
                _logger.Debug($"View '{view.Name}' cannot be printed/exported");
                return false;
            }
            
            // Check if view has valid extents
            try
            {
                var bbox = view.get_BoundingBox(null);
                if (bbox == null || !bbox.Enabled)
                {
                    _logger.Debug($"View '{view.Name}' has no valid bounding box");
                    return false;
                }
            }
            catch
            {
                _logger.Debug($"View '{view.Name}' bounding box check failed");
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Check if view name matches the pattern
        /// </summary>
        private static bool MatchesPattern(string viewName, string pattern, bool caseSensitive)
        {
            if (string.IsNullOrWhiteSpace(pattern))
            {
                return true; // No pattern means all views match
            }
            
            var comparison = caseSensitive ? 
                StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            
            // Support wildcards
            if (pattern.Contains("*"))
            {
                return MatchesWildcard(viewName, pattern, caseSensitive);
            }
            
            // Simple contains check
            return viewName.IndexOf(pattern, comparison) >= 0;
        }
        
        /// <summary>
        /// Match view name against wildcard pattern
        /// </summary>
        private static bool MatchesWildcard(string viewName, string pattern, bool caseSensitive)
        {
            // Convert wildcard pattern to regex
            var regexPattern = "^" + System.Text.RegularExpressions.Regex.Escape(pattern)
                .Replace("\\*", ".*")
                .Replace("\\?", ".") + "$";
            
            var options = caseSensitive ? 
                System.Text.RegularExpressions.RegexOptions.None : 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase;
            
            return System.Text.RegularExpressions.Regex.IsMatch(viewName, regexPattern, options);
        }
        
        /// <summary>
        /// Check if view is placed on a sheet
        /// </summary>
        private static bool IsOnSheet(Document doc, View3D view)
        {
            try
            {
                // Get all viewports in the document
                var viewports = new FilteredElementCollector(doc)
                    .OfClass(typeof(Viewport))
                    .Cast<Viewport>()
                    .Where(vp => vp.ViewId == view.Id);
                
                return viewports.Any();
            }
            catch (Exception ex)
            {
                _logger.Debug($"Error checking sheet association for view '{view.Name}': {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Check if this is a system view that should be excluded
        /// </summary>
        private static bool IsSystemView(View3D view)
        {
            var systemViewNames = new[]
            {
                "{3D}",
                "Project View",
                "Working View",
                "Coordination View",
                "System Browser"
            };
            
            return systemViewNames.Any(name => 
                view.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
        
        /// <summary>
        /// Get detailed information about a view
        /// </summary>
        private static ViewInfo GetViewInfo(Document doc, View3D view)
        {
            var info = new ViewInfo
            {
                Name = view.Name,
                ViewType = view.ViewType.ToString()
            };
            
            try
            {
                // Get level if applicable
                info.Level = view.GenLevel?.Name ?? "N/A";
                
                // Get phase
                var phaseParam = view.get_Parameter(BuiltInParameter.VIEW_PHASE);
                if (phaseParam != null && phaseParam.HasValue)
                {
                    var phaseId = phaseParam.AsElementId();
                    var phase = doc.GetElement(phaseId) as Phase;
                    info.Phase = phase?.Name ?? "N/A";
                }
                else
                {
                    info.Phase = "N/A";
                }
                
                // Get discipline
                var disciplineParam = view.get_Parameter(BuiltInParameter.VIEW_DISCIPLINE);
                if (disciplineParam != null && disciplineParam.HasValue)
                {
                    info.Discipline = disciplineParam.AsValueString() ?? "N/A";
                }
                else
                {
                    info.Discipline = "N/A";
                }
                
                // Check if view is locked
                info.IsLocked = view.IsLocked;
                
                // Get view template
                var templateId = view.ViewTemplateId;
                if (templateId != ElementId.InvalidElementId)
                {
                    var template = doc.GetElement(templateId) as View;
                    info.ViewTemplate = template?.Name ?? "None";
                }
                else
                {
                    info.ViewTemplate = "None";
                }
            }
            catch (Exception ex)
            {
                _logger.Debug($"Error getting view info: {ex.Message}");
            }
            
            return info;
        }
        
        /// <summary>
        /// Get all unique view names that match a pattern
        /// </summary>
        public static List<string> GetMatchingViewNames(Document doc, string pattern, bool caseSensitive = false)
        {
            try
            {
                var views = new FilteredElementCollector(doc)
                    .OfClass(typeof(View3D))
                    .Cast<View3D>()
                    .Where(v => !v.IsTemplate && MatchesPattern(v.Name, pattern, caseSensitive))
                    .Select(v => v.Name)
                    .Distinct()
                    .OrderBy(name => name)
                    .ToList();
                
                return views;
            }
            catch
            {
                return new List<string>();
            }
        }
    }
    
    /// <summary>
    /// View information data class
    /// </summary>
    public class ViewInfo
    {
        public string Name { get; set; }
        public string ViewType { get; set; }
        public string Level { get; set; }
        public string Phase { get; set; }
        public string Discipline { get; set; }
        public string ViewTemplate { get; set; }
        public bool IsLocked { get; set; }
    }
}