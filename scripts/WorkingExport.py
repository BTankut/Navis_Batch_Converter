"""Working Navisworks Export Script"""
import clr
clr.AddReference('RevitAPI')
clr.AddReference('RevitAPIUI')
from Autodesk.Revit.DB import *

# Get document from revit_script_util
import revit_script_util
doc = revit_script_util.GetScriptDocument()

print "Document title:", doc.Title

# Create output directory
import os
output_path = r"C:\Output\Navisworks"
if not os.path.exists(output_path):
    os.makedirs(output_path)

# Create NWC export
file_name = doc.Title.replace(".rvt", "") + ".nwc"

# Create export options
options = NavisworksExportOptions()
options.ExportScope = NavisworksExportScope.Model
options.ExportParts = True
options.ExportLinks = True
options.ConvertElementProperties = True

# Export
result = doc.Export(output_path, file_name, options)

if result:
    print "Export completed successfully:", file_name
else:
    print "Export failed!"