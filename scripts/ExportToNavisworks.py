"""
Revit to Navisworks Export Script for RevitBatchProcessor
"""
import clr
import sys
import os
from datetime import datetime

# Create a log file
log_path = r"C:\Output\Navisworks\logs\python_script_" + datetime.now().strftime("%Y%m%d_%H%M%S") + ".log"
os.makedirs(os.path.dirname(log_path), exist_ok=True)

def log(message):
    """Write to log file"""
    with open(log_path, 'a') as f:
        f.write(f"[{datetime.now().strftime('%Y-%m-%d %H:%M:%S')}] {message}\n")
    log(message)

try:
    log("Starting ExportToNavisworks.py script")
    
    # Add Revit API references
    clr.AddReference('RevitAPI')
    clr.AddReference('RevitAPIUI')
    log("Revit API references added")
    
    from Autodesk.Revit.DB import *
    import Autodesk.Revit.DB as DB
    log("Revit API imports completed")
    
    # Get the current document
    doc = __revit__.ActiveUIDocument.Document
    log(f"Got document: {doc.Title}")
    
except Exception as ex:
    log(f"ERROR in initialization: {str(ex)}")
    import traceback
    log(traceback.format_exc())
    raise

def export_to_navisworks(doc):
    """Export current Revit document to Navisworks NWC format"""
    try:
        log("Starting export_to_navisworks function")
        log("Document title: " + doc.Title)
        
        # Get the document path
        if not doc.IsWorkshared:
            doc_path = doc.PathName
        else:
            doc_path = ModelPathUtils.ConvertModelPathToUserVisiblePath(doc.GetWorksharingCentralModelPath())
        
        if not doc_path:
            log("ERROR: Document has not been saved")
            return False
            
        # Create output path
        import os
        output_dir = r"C:\Output\Navisworks"
        if not os.path.exists(output_dir):
            os.makedirs(output_dir)
            
        file_name = os.path.splitext(os.path.basename(doc_path))[0]
        output_path = os.path.join(output_dir, file_name + ".nwc")
        
        # Create Navisworks export options
        options = NavisworksExportOptions()
        options.ExportScope = NavisworksExportScope.Model
        options.ExportLinks = True
        options.ExportParts = True
        options.ExportRoomGeometry = False
        options.ConvertElementProperties = True
        options.Coordinates = NavisworksCoordinates.Shared
        options.DivideFileIntoLevels = False
        options.ExportUrls = False
        options.FindMissingMaterials = True
        
        # Find first 3D view
        collector = FilteredElementCollector(doc).OfClass(View3D)
        views_3d = [v for v in collector if not v.IsTemplate]
        
        if views_3d:
            # Filter for views with "navis" in name
            navis_views = [v for v in views_3d if "navis" in v.Name.lower()]
            
            if navis_views:
                view = navis_views[0]
                log("Using view: " + view.Name)
            else:
                view = views_3d[0]
                log("No 'navis' view found, using: " + view.Name)
                
            options.ViewId = view.Id
            options.ExportScope = NavisworksExportScope.View
        else:
            log("No 3D views found, exporting entire model")
            options.ExportScope = NavisworksExportScope.Model
        
        # Export
        log("Exporting to: " + output_path)
        result = doc.Export(os.path.dirname(output_path), os.path.basename(output_path), options)
        
        if result:
            log("SUCCESS: Export completed")
            return True
        else:
            log("ERROR: Export failed")
            return False
            
    except Exception as ex:
        log("ERROR: " + str(ex))
        import traceback
        log(traceback.format_exc())
        return False

# Execute export
if __name__ == "__main__":
    success = export_to_navisworks(doc)
    if not success:
        raise Exception("Export failed")