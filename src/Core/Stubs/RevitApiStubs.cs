// Temporary stub classes for building without Revit API
// Remove this file when Revit API references are working

using System;
using System.Collections.Generic;
using System.Linq;

namespace Autodesk.Revit.DB
{
    public class Document
    {
        public string PathName { get; set; }
        public string Title { get; set; }
        public bool IsWorkshared { get; set; }
        public bool IsFamilyDocument { get; set; }
        public Autodesk.Revit.ApplicationServices.Application Application { get; set; }
        public WorksetTableInstance GetWorksetTable() => new WorksetTableInstance();
        public FilteredElementCollector FilteredElementCollector(Document doc) => new FilteredElementCollector(doc);
        public bool Export(string folder, string fileName, NavisworksExportOptions options) => true;
        public Element GetElement(ElementId id) => new Element();
    }

    public class View3D : Element
    {
        public bool IsTemplate { get; set; }
        public bool CanBePrinted { get; set; }
        public bool IsLocked { get; set; }
        public bool CanModifyViewSpecificElements { get; set; } = true;
        public ViewType ViewType { get; set; }
        public ElementId GenLevel { get; set; }
        public ElementId ViewTemplateId { get; set; }
        public ViewSheet GetViewSheet() => null;
        public BoundingBoxXYZ get_BoundingBox(View3D view) => new BoundingBoxXYZ();
        public Parameter get_Parameter(BuiltInParameter param) => new Parameter();
        public bool CanModifyWorksetVisibility() => true;
        public void SetWorksetVisibility(WorksetId worksetId, WorksetVisibility visibility) { }
    }

    public class WorksetTableInstance
    {
        public WorksetId GetActiveWorksetId() => new WorksetId();
        public void SetWorksetDefaultVisibilitySettings(Document doc, WorksetDefaultVisibilitySettings settings) { }
    }

    public class WorksetId
    {
    }

    public class Workset
    {
        public string Name { get; set; }
        public WorksetId Id { get; set; }
        public bool IsOpen { get; set; }
        public bool IsVisibleByDefault { get; set; }
        public bool IsEditable { get; set; }
        public string Owner { get; set; }
        public Guid UniqueId { get; set; } = Guid.NewGuid();
    }

    public class ElementId
    {
        public static ElementId InvalidElementId => new ElementId();
        public int IntegerValue { get; set; }
    }

    public class ViewSheet : Element
    {
    }

    public class NavisworksExportOptions
    {
        public bool ExportLinks { get; set; }
        public bool ExportParts { get; set; }
        public bool DivideFileIntoLevels { get; set; }
        public bool ExportElementIds { get; set; }
        public bool ConvertElementProperties { get; set; }
        public NavisworksCoordinates Coordinates { get; set; }
        public bool ExportRoomAsAttribute { get; set; }
        public bool ExportRoomGeometry { get; set; }
        public bool ExportUrls { get; set; }
        public bool FindMissingMaterials { get; set; }
        public ViewId ViewId { get; set; }
        public NavisworksExportScope ExportScope { get; set; }
    }

    public class ViewId : ElementId
    {
    }

    public enum NavisworksCoordinates
    {
        Shared,
        Internal
    }

    public enum NavisworksExportScope
    {
        View,
        Model
    }

    public class Transaction : IDisposable
    {
        public Transaction(Document doc, string name) { }
        public void Start() { }
        public void Commit() { }
        public void RollBack() { }
        public void Dispose() { }
    }

    public class FilteredElementCollector : IEnumerable<Element>
    {
        private List<Element> elements = new List<Element>();
        
        public FilteredElementCollector(Document doc) { }
        public FilteredElementCollector OfClass(Type type) => this;
        public FilteredElementCollector WhereElementIsNotElementType() => this;
        public IList<Element> ToElements() => elements;
        public IEnumerable<T> Cast<T>() where T : Element => elements.Cast<T>();
        public IEnumerator<Element> GetEnumerator() => elements.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class Element
    {
        public string Name { get; set; }
        public ElementId Id { get; set; }
    }

    public class FilteredWorksetCollector : IEnumerable<Workset>
    {
        private List<Workset> worksets = new List<Workset>();
        
        public FilteredWorksetCollector(Document doc) { }
        public FilteredWorksetCollector OfKind(WorksetKind kind) => this;
        public IEnumerable<T> Cast<T>() where T : Workset => worksets.Cast<T>();
        public IEnumerator<Workset> GetEnumerator() => worksets.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public enum WorksetKind
    {
        UserWorkset
    }

    public class WorksetDefaultVisibilitySettings
    {
        public static WorksetDefaultVisibilitySettings Visible => new WorksetDefaultVisibilitySettings();
        public static WorksetDefaultVisibilitySettings Hidden => new WorksetDefaultVisibilitySettings();
        public bool IsWorksetVisible(WorksetId id) => true;
        public void SetWorksetVisibility(WorksetId id, bool visible) { }
    }

    public static class WorksetTable  
    {
        public static bool IsWorksetEditable(Document doc, WorksetId id) => true;
        public static void OpenWorksets(Document doc, IList<WorksetId> worksetIds) { }
    }

    public enum WorksetVisibility
    {
        Visible,
        Hidden,
        DefaultVisibility
    }

    public class BoundingBoxXYZ
    {
        public XYZ Min { get; set; }
        public XYZ Max { get; set; }
        public bool Enabled { get; set; } = true;
    }

    public class XYZ
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
    }

    public class Parameter
    {
        public bool HasValue { get; set; } = true;
        public ElementId AsElementId() => new ElementId();
        public string AsString() => "";
        public string AsValueString() => "";
        public int AsInteger() => 0;
        public double AsDouble() => 0.0;
    }

    public enum BuiltInParameter
    {
        VIEW_PHASE,
        VIEW_DISCIPLINE,
        VIEW_DETAIL_LEVEL
    }

    public class View : Element
    {
    }

    public class Phase : Element
    {
    }

    public class Level : Element
    {
    }

    public class Viewport
    {
        public ElementId ViewId { get; set; }
        public ElementId SheetId { get; set; }
    }

    public enum ViewType
    {
        ThreeD,
        FloorPlan,
        CeilingPlan,
        Elevation,
        Section,
        Detail,
        DraftingView,
        Legend,
        Schedule,
        Sheet,
        Undefined
    }
}

namespace Autodesk.Revit.ApplicationServices
{
    public class Application
    {
        public string VersionNumber => "2021";
        public string VersionName => "Autodesk Revit 2021";
    }
}