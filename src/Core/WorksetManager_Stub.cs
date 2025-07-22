// This is a stub file for compilation without Revit API
// The real WorksetManager.cs requires Revit API DLLs

using System;
using System.Collections.Generic;

namespace RevitNavisworksAutomation.Core
{
    public static class WorksetManager
    {
        // Stub implementation for compilation
        public static void ApplyWorksetFilter(object doc, List<string> includePatterns)
        {
            // Stub
        }
    }
    
    public class WorksetInfo
    {
        public object Id { get; set; }
        public string Name { get; set; }
        public bool IsOpen { get; set; }
        public bool IsEditable { get; set; }
        public string Owner { get; set; }
        public Guid UniqueId { get; set; }
    }
    
    public class WorksetConfiguration
    {
        public bool IsWorkshared { get; set; }
        public List<WorksetConfigItem> Worksets { get; set; } = new List<WorksetConfigItem>();
        
        public int IncludedCount => 0;
        public int ExcludedCount => 0;
    }
    
    public class WorksetConfigItem
    {
        public string Name { get; set; }
        public bool IsIncluded { get; set; }
        public bool IsOpen { get; set; }
        public bool IsEditable { get; set; }
    }
}