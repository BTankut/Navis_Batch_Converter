[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
    [string]$InputFolder,
    
    [string]$ConfigFile = ".\Config.json",
    
    [string]$RevitVersion = "2022",
    
    [switch]$IncludeSubfolders,
    
    [string[]]$FileFilter = @("*.rvt"),
    
    [switch]$WhatIf
)

# Load configuration
$config = Get-Content $ConfigFile | ConvertFrom-Json

# Validate prerequisites
function Test-Prerequisites {
    $rbpPath = "$env:LOCALAPPDATA\RevitBatchProcessor\BatchRvt.exe"
    if (!(Test-Path $rbpPath)) {
        throw "RevitBatchProcessor not found. Please install from: https://github.com/bvn-architecture/RevitBatchProcessor"
    }
    
    $navisPath = "C:\Program Files\Autodesk\Navisworks Manage $RevitVersion\FileToolsTaskRunner.exe"
    if (!(Test-Path $navisPath)) {
        throw "Navisworks $RevitVersion not found at: $navisPath"
    }
    
    return @{
        RBP = $rbpPath
        Navis = $navisPath
    }
}

# Main execution
try {
    Write-Host "=== Revit to Navisworks Batch Converter ===" -ForegroundColor Cyan
    Write-Host "Configuration: $ConfigFile" -ForegroundColor Gray
    
    $tools = Test-Prerequisites
    
    # Step 1: Collect Revit files
    $searchOption = if ($IncludeSubfolders) { "AllDirectories" } else { "TopDirectoryOnly" }
    $revitFiles = @()
    
    foreach ($filter in $FileFilter) {
        $revitFiles += Get-ChildItem -Path $InputFolder -Filter $filter -Recurse:$IncludeSubfolders
    }
    
    Write-Host "`nFound $($revitFiles.Count) Revit files to process" -ForegroundColor Yellow
    
    if ($WhatIf) {
        Write-Host "`n[WhatIf Mode] Would process:" -ForegroundColor Magenta
        $revitFiles | ForEach-Object { Write-Host "  - $($_.Name)" }
        return
    }
    
    # Step 2: Create file list for BatchProcessor
    $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $fileListPath = Join-Path $env:TEMP "revit_batch_$timestamp.txt"
    $revitFiles | Select-Object -ExpandProperty FullName | Out-File $fileListPath -Encoding UTF8
    
    # Step 3: Execute RevitBatchProcessor in CLI mode (no GUI)
    Write-Host "`nStep 1/3: Exporting NWC files from Revit..." -ForegroundColor Green
    
    $scriptPath = Join-Path $PSScriptRoot "..\src\Core\RevitExportTask.cs"
    $logFolder = Join-Path $config.LogDirectory $timestamp
    
    # Create log folder
    New-Item -ItemType Directory -Path $logFolder -Force | Out-Null
    
    $rbpArgs = @(
        "--task_script", $scriptPath,
        "--file_list", $fileListPath,
        "--revit_version", $RevitVersion,
        "--log_folder", $logFolder,
        "--detach",
        "--worksets", "open_all",
        "--show_message_boxes", "No",  # Prevent GUI popups
        "--enable_data_export"
    )
    
    if ($config.Processing.MaxParallelFiles -gt 1) {
        $rbpArgs += "--max_parallel_processes", $config.Processing.MaxParallelFiles
    }
    
    # Run in background without showing GUI
    $rbpStartInfo = New-Object System.Diagnostics.ProcessStartInfo
    $rbpStartInfo.FileName = $tools.RBP
    $rbpStartInfo.Arguments = $rbpArgs -join " "
    $rbpStartInfo.UseShellExecute = $false
    $rbpStartInfo.CreateNoWindow = $true
    $rbpStartInfo.RedirectStandardOutput = $true
    $rbpStartInfo.RedirectStandardError = $true
    
    $rbpProcess = New-Object System.Diagnostics.Process
    $rbpProcess.StartInfo = $rbpStartInfo
    
    # Start process
    $rbpProcess.Start() | Out-Null
    
    # Monitor progress
    $startTime = Get-Date
    $outputBuilder = New-Object System.Text.StringBuilder
    
    # Read output asynchronously
    $outputHandler = {
        if ($EventArgs.Data -ne $null) {
            $script:outputBuilder.AppendLine($EventArgs.Data)
            Write-Host $EventArgs.Data
        }
    }
    
    Register-ObjectEvent -InputObject $rbpProcess -EventName OutputDataReceived -Action $outputHandler | Out-Null
    Register-ObjectEvent -InputObject $rbpProcess -EventName ErrorDataReceived -Action $outputHandler | Out-Null
    
    $rbpProcess.BeginOutputReadLine()
    $rbpProcess.BeginErrorReadLine()
    
    # Wait for completion with progress
    while (!$rbpProcess.HasExited) {
        $elapsed = (Get-Date) - $startTime
        Write-Progress -Activity "Processing Revit files" `
                      -Status "Elapsed: $($elapsed.ToString('hh\:mm\:ss'))" `
                      -PercentComplete -1
        Start-Sleep -Seconds 5
    }
    
    $rbpProcess.WaitForExit()
    
    if ($rbpProcess.ExitCode -ne 0) {
        throw "RevitBatchProcessor failed with exit code: $($rbpProcess.ExitCode)"
    }
    
    # Step 4: Combine NWC files to NWD (if configured)
    if ($config.PostProcessing.CombineToNWD) {
        Write-Host "`nStep 2/3: Combining NWC files into NWD..." -ForegroundColor Green
        
        $nwcFiles = Get-ChildItem -Path $config.OutputDirectory -Filter "*.nwc" | 
                    Where-Object { $_.LastWriteTime -gt $startTime }
        
        if ($nwcFiles.Count -gt 0) {
            $nwcListPath = Join-Path $env:TEMP "nwc_files_$timestamp.txt"
            $nwcFiles | Select-Object -ExpandProperty FullName | Out-File $nwcListPath -Encoding UTF8
            
            $nwdOutput = Join-Path $config.OutputDirectory "Combined_$timestamp.nwd"
            
            # Create NWD using Navisworks command line
            $navisArgs = @(
                "/i", $nwcListPath,
                "/of", $nwdOutput,
                "/over",
                "/version", $RevitVersion
            )
            
            $navisProcess = Start-Process -FilePath $tools.Navis `
                                        -ArgumentList $navisArgs `
                                        -NoNewWindow `
                                        -Wait `
                                        -PassThru
            
            if ($navisProcess.ExitCode -eq 0) {
                Write-Host "Created NWD: $nwdOutput" -ForegroundColor Cyan
                
                # Optional: Delete NWC files
                if ($config.PostProcessing.DeleteNWCAfterCombine) {
                    $nwcFiles | Remove-Item -Force
                    Write-Host "Cleaned up $($nwcFiles.Count) NWC files" -ForegroundColor Gray
                }
            }
            else {
                Write-Warning "Failed to create NWD file. Exit code: $($navisProcess.ExitCode)"
            }
        }
    }
    
    # Step 5: Generate summary report
    Write-Host "`nStep 3/3: Generating summary report..." -ForegroundColor Green
    
    $summaryPath = Join-Path $logFolder "summary.txt"
    $endTime = Get-Date
    $duration = $endTime - $startTime
    
    $summary = @"
Revit to Navisworks Batch Conversion Summary
============================================
Start Time: $($startTime.ToString('yyyy-MM-dd HH:mm:ss'))
End Time: $($endTime.ToString('yyyy-MM-dd HH:mm:ss'))
Duration: $($duration.ToString())

Input Files: $($revitFiles.Count)
Output Directory: $($config.OutputDirectory)
Log Directory: $logFolder

Configuration:
- Revit Version: $RevitVersion
- View Filter: $($config.ViewFilter.Pattern)
- Workset Filter: $($config.WorksetSettings.FilterWorksets)
- Include Worksets: $($config.WorksetSettings.IncludeWorksets -join ', ')

Results:
- NWC Files Created: $($nwcFiles.Count)
- NWD File: $(if ($nwdOutput) { Split-Path $nwdOutput -Leaf } else { 'N/A' })

Processing Log:
$($outputBuilder.ToString())
"@
    
    $summary | Out-File $summaryPath -Encoding UTF8
    Write-Host "`nConversion completed successfully!" -ForegroundColor Green
    Write-Host "Summary saved to: $summaryPath" -ForegroundColor Gray
    
} catch {
    Write-Error "Batch conversion failed: $_"
    exit 1
} finally {
    # Cleanup temp files
    if (Test-Path $fileListPath) { Remove-Item $fileListPath -Force -ErrorAction SilentlyContinue }
    if (Test-Path $nwcListPath) { Remove-Item $nwcListPath -Force -ErrorAction SilentlyContinue }
    
    # Remove progress
    Write-Progress -Activity "Processing Revit files" -Completed
}