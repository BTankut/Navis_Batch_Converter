// This is a stub file for compilation without Revit API
// The real ViewSelector.cs requires Revit API DLLs

using System.Collections.Generic;
using RevitNavisworksAutomation.Models;

namespace RevitNavisworksAutomation.Core
{
    public static class ViewSelector
    {
        // Stub implementation for compilation
        public static List<object> GetNavisworksViews(object doc, ViewFilterConfig config)
        {
            return new List<object>();
        }
    }
    
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