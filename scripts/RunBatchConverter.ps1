[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
    [string]$ConfigFile
)

# Error handling
$ErrorActionPreference = "Stop"

# Load configuration
Write-Host "Loading configuration from: $ConfigFile" -ForegroundColor Cyan
$config = Get-Content $ConfigFile -Raw | ConvertFrom-Json

# Find RevitBatchProcessor
function Find-RevitBatchProcessor {
    $possiblePaths = @(
        "C:\Program Files (x86)\RevitBatchProcessor\RevitBatchProcessor.exe",
        "C:\Program Files\RevitBatchProcessor\RevitBatchProcessor.exe",
        "${env:ProgramFiles(x86)}\RevitBatchProcessor\RevitBatchProcessor.exe",
        "${env:ProgramFiles}\RevitBatchProcessor\RevitBatchProcessor.exe",
        "${env:LOCALAPPDATA}\RevitBatchProcessor\BatchRvt.exe"
    )
    
    foreach ($path in $possiblePaths) {
        if (Test-Path $path) {
            return $path
        }
    }
    
    throw "RevitBatchProcessor not found! Please install from: https://github.com/bvn-architecture/RevitBatchProcessor/releases"
}

# Create Python script for RevitBatchProcessor
function Create-TaskScript {
    param($Config)
    
    $scriptContent = @'
import clr
import sys
import json
from System.IO import Path, Directory

# Add Revit API references
clr.AddReference('RevitAPI')
clr.AddReference('RevitAPIUI')
from Autodesk.Revit.DB import *

def process_document(doc, config):
    """Process a single Revit document for Navisworks export"""
    try:
        # Import the RevitExportTask module
        sys.path.append(r"C:\Users\BT\CascadeProjects\Navis_Batch_Converter\src\Core")
        from RevitExportTask import Execute
        
        # Execute the export
        Execute(doc, doc.PathName)
        
        return True
    except Exception as ex:
        print("ERROR: " + str(ex))
        return False

# Main entry point for RevitBatchProcessor
if __name__ == "__main__":
    # Read configuration
    config_path = sys.argv[1] if len(sys.argv) > 1 else None
    if config_path and Path.Exists(config_path):
        with open(config_path, 'r') as f:
            config = json.load(f)
    else:
        config = {}
    
    # Process the document
    doc = __revit__.ActiveUIDocument.Document
    success = process_document(doc, config)
    
    if not success:
        raise Exception("Export failed")
'@
    
    $scriptPath = [System.IO.Path]::Combine([System.IO.Path]::GetTempPath(), "NavisBatchTask.py")
    $scriptContent | Out-File -FilePath $scriptPath -Encoding UTF8
    return $scriptPath
}

# Main execution
try {
    Write-Host "=== Revit to Navisworks Batch Converter ===" -ForegroundColor Cyan
    Write-Host "Starting conversion process..." -ForegroundColor Gray
    
    # Find RevitBatchProcessor
    $rbpPath = Find-RevitBatchProcessor
    Write-Host "Found RevitBatchProcessor at: $rbpPath" -ForegroundColor Green
    
    # Create task script
    $taskScript = Create-TaskScript -Config $config
    Write-Host "Created task script: $taskScript" -ForegroundColor Gray
    
    # Create file list
    $fileListPath = [System.IO.Path]::Combine([System.IO.Path]::GetTempPath(), "NavisBatchFiles.txt")
    $config.InputFiles | Out-File -FilePath $fileListPath -Encoding UTF8
    
    # Use the pre-written Python script
    $pythonScript = [System.IO.Path]::Combine($PSScriptRoot, "ExportToNavisworks.py")
    if (!(Test-Path $pythonScript)) {
        throw "Python script not found: $pythonScript"
    }
    
    # Create settings file for BatchRvt
    $settingsPath = [System.IO.Path]::Combine([System.IO.Path]::GetTempPath(), "batch_settings.json")
    $settings = @{
        TaskScriptFilePath = $pythonScript
        RevitFileListFilePath = $fileListPath
        DataExportFolderPath = $config.OutputDirectory
        RevitSessionOption = "UseSeparateSessionPerFile"
        RevitProcessingOption = "BatchRevitFileProcessing"
        CentralFileOpenOption = "Detach"
        DeleteLocalAfter = $false
        DiscardWorksetsOnDetach = $false
        AuditOnOpen = $false
        ProcessingTimeOutMinutes = 60
        ShowMessageBoxOnTaskScriptError = $false
        LogFolderPath = [System.IO.Path]::Combine($config.OutputDirectory, "logs")
        ShowRevitProcessErrorMessages = $false
        EnableDataExport = $true
        RevitFileProcessingOption = "UseSpecificRevitVersion"
        SpecificRevitVersionNumber = $config.RevitVersion
    }
    $settings | ConvertTo-Json | Out-File -FilePath $settingsPath -Encoding UTF8
    
    # RevitBatchProcessor arguments - direct command line
    $rbpArgs = @(
        "--task_script", $pythonScript,
        "--file_list", $fileListPath,
        "--output_folder", $config.OutputDirectory,
        "--log_folder", [System.IO.Path]::Combine($config.OutputDirectory, "logs"),
        "--revit_version", $config.RevitVersion
    )
    
    Write-Host "Processing $($config.InputFiles.Count) files..." -ForegroundColor Yellow
    
    # Run RevitBatchProcessor
    Write-Host "Running: $rbpPath" -ForegroundColor Cyan
    Write-Host "With arguments:" -ForegroundColor Gray
    Write-Host "  Task Script: $pythonScript" -ForegroundColor Gray
    Write-Host "  File List: $fileListPath" -ForegroundColor Gray
    Write-Host "  Output: $($config.OutputDirectory)" -ForegroundColor Gray
    Write-Host "  Revit Version: $($config.RevitVersion)" -ForegroundColor Gray
    
    $process = Start-Process -FilePath $rbpPath -ArgumentList $rbpArgs -PassThru -Wait
    
    Write-Host "Exit code: $($process.ExitCode)" -ForegroundColor Yellow
    
    # Check for log files
    $logFiles = Get-ChildItem -Path ([System.IO.Path]::Combine($config.OutputDirectory, "logs")) -Filter "*.log" | Sort-Object LastWriteTime -Descending | Select-Object -First 5
    if ($logFiles) {
        Write-Host "Recent log files:" -ForegroundColor Cyan
        $logFiles | ForEach-Object { Write-Host "  - $($_.Name) ($($_.LastWriteTime))" }
    }
    
    if ($process.ExitCode -eq 0) {
        Write-Host "Conversion completed successfully!" -ForegroundColor Green
        
        # Post-processing: Combine NWC to NWD if requested
        if ($config.PostProcessing.CombineToNWD) {
            Write-Host "Combining NWC files to NWD..." -ForegroundColor Cyan
            # TODO: Implement NWD combination using Navisworks Batch Utility
        }
    }
    else {
        throw "RevitBatchProcessor failed with exit code: $($process.ExitCode)"
    }
}
catch {
    Write-Error $_.Exception.Message
    exit 1
}
finally {
    # Cleanup temp files
    if (Test-Path $taskScript) { Remove-Item $taskScript -Force }
    if (Test-Path $fileListPath) { Remove-Item $fileListPath -Force }
    if (Test-Path $settingsPath) { Remove-Item $settingsPath -Force }
}

Write-Host "Done!" -ForegroundColor Green