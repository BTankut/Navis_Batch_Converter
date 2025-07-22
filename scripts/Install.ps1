#Requires -RunAsAdministrator

Write-Host "Installing Revit to Navisworks Automation Solution..." -ForegroundColor Cyan

# 1. Check RevitBatchProcessor
$rbpUrl = "https://github.com/bvn-architecture/RevitBatchProcessor/releases/latest"
Write-Host "`nChecking RevitBatchProcessor installation..."

if (!(Test-Path "$env:LOCALAPPDATA\RevitBatchProcessor\BatchRvt.exe")) {
    Write-Host "RevitBatchProcessor not found!" -ForegroundColor Yellow
    Write-Host "Please install RevitBatchProcessor from: $rbpUrl" -ForegroundColor Yellow
    
    $response = Read-Host "Would you like to open the download page now? (Y/N)"
    if ($response -eq 'Y') {
        Start-Process $rbpUrl
    }
    
    Read-Host "Press Enter after installation is complete"
    
    # Verify installation
    if (!(Test-Path "$env:LOCALAPPDATA\RevitBatchProcessor\BatchRvt.exe")) {
        Write-Error "RevitBatchProcessor still not found. Installation cannot continue."
        exit 1
    }
}
else {
    Write-Host "RevitBatchProcessor found!" -ForegroundColor Green
}

# 2. Check Revit installation
Write-Host "`nChecking Revit installations..."
$revitVersions = @("2021", "2022", "2023", "2024")
$foundRevit = $false

foreach ($version in $revitVersions) {
    $revitPath = "C:\Program Files\Autodesk\Revit $version\Revit.exe"
    if (Test-Path $revitPath) {
        Write-Host "Found Revit $version" -ForegroundColor Green
        $foundRevit = $true
    }
}

if (!$foundRevit) {
    Write-Error "No Revit installation found. Please install Revit 2021-2024."
    exit 1
}

# 3. Check Navisworks installation
Write-Host "`nChecking Navisworks installations..."
$foundNavis = $false

foreach ($version in $revitVersions) {
    $navisPath = "C:\Program Files\Autodesk\Navisworks Manage $version\roamer.exe"
    if (Test-Path $navisPath) {
        Write-Host "Found Navisworks Manage $version" -ForegroundColor Green
        $foundNavis = $true
    }
}

if (!$foundNavis) {
    Write-Warning "No Navisworks Manage installation found. The converter may not work properly."
    $continue = Read-Host "Continue anyway? (Y/N)"
    if ($continue -ne 'Y') {
        exit 1
    }
}

# 4. Create directories
Write-Host "`nCreating application directories..."
$dirs = @(
    "C:\ProgramData\NavisBatchConverter",
    "C:\ProgramData\NavisBatchConverter\logs",
    "C:\ProgramData\NavisBatchConverter\output",
    "C:\ProgramData\NavisBatchConverter\config"
)

foreach ($dir in $dirs) {
    if (!(Test-Path $dir)) {
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
        Write-Host "Created: $dir" -ForegroundColor Green
    }
}

# 5. Copy configuration
Write-Host "`nCopying configuration files..."
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$configSource = Join-Path $scriptPath "Config.json"
$configDest = "C:\ProgramData\NavisBatchConverter\config\Config.json"

if (Test-Path $configSource) {
    Copy-Item $configSource $configDest -Force
    Write-Host "Configuration copied to: $configDest" -ForegroundColor Green
}

# 6. Set permissions
Write-Host "`nSetting permissions..."
$acl = Get-Acl "C:\ProgramData\NavisBatchConverter"
$permission = "Users", "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow"
$accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule $permission
$acl.SetAccessRule($accessRule)
Set-Acl "C:\ProgramData\NavisBatchConverter" $acl
Write-Host "Permissions set successfully" -ForegroundColor Green

# 7. Create desktop shortcut
Write-Host "`nCreating desktop shortcut..."
$desktopPath = [Environment]::GetFolderPath("Desktop")
$shortcutPath = Join-Path $desktopPath "Navis Batch Converter.lnk"
$targetPath = Join-Path (Split-Path -Parent $scriptPath) "NavisBatchConverter.exe"

if (Test-Path $targetPath) {
    $WshShell = New-Object -ComObject WScript.Shell
    $Shortcut = $WshShell.CreateShortcut($shortcutPath)
    $Shortcut.TargetPath = $targetPath
    $Shortcut.WorkingDirectory = Split-Path -Parent $targetPath
    $Shortcut.IconLocation = $targetPath
    $Shortcut.Description = "Revit to Navisworks Batch Converter"
    $Shortcut.Save()
    Write-Host "Desktop shortcut created" -ForegroundColor Green
}

# 8. Register file association (optional)
$registerFileAssoc = Read-Host "`nRegister .rvt files with Navis Batch Converter? (Y/N)"
if ($registerFileAssoc -eq 'Y') {
    # This would require registry modifications
    Write-Host "File association feature not yet implemented" -ForegroundColor Yellow
}

# 9. Create scheduled task (optional)
$createTask = Read-Host "`nCreate scheduled task for daily automation? (Y/N)"
if ($createTask -eq 'Y') {
    $taskName = "NavisBatchConverter"
    $taskDescription = "Automated Revit to Navisworks conversion"
    
    # Define the action
    $action = New-ScheduledTaskAction -Execute "PowerShell.exe" `
        -Argument "-ExecutionPolicy Bypass -File `"$scriptPath\RunBatchConverter.ps1`" -InputFolder `"C:\Projects\Revit`""
    
    # Define the trigger (daily at 2 AM)
    $trigger = New-ScheduledTaskTrigger -Daily -At "2:00AM"
    
    # Define settings
    $settings = New-ScheduledTaskSettingsSet `
        -AllowStartIfOnBatteries `
        -DontStopIfGoingOnBatteries `
        -StartWhenAvailable `
        -RunOnlyIfNetworkAvailable `
        -DontStopOnIdleEnd
    
    # Register the task
    try {
        Register-ScheduledTask -TaskName $taskName `
            -Action $action `
            -Trigger $trigger `
            -Settings $settings `
            -Description $taskDescription `
            -Force
        
        Write-Host "Scheduled task created: $taskName" -ForegroundColor Green
    }
    catch {
        Write-Warning "Failed to create scheduled task: $_"
    }
}

# 10. Final summary
Write-Host "`n=====================================" -ForegroundColor Cyan
Write-Host "Installation completed successfully!" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Build the application in Visual Studio"
Write-Host "2. Run NavisBatchConverter.exe"
Write-Host "3. Configure settings as needed"
Write-Host ""
Write-Host "For command-line usage:" -ForegroundColor Yellow
Write-Host "  .\scripts\RunBatchConverter.ps1 -InputFolder 'C:\Your\Revit\Files'" -ForegroundColor White
Write-Host ""
Write-Host "Log files will be saved to: C:\ProgramData\NavisBatchConverter\logs" -ForegroundColor Gray
Write-Host "Output files will be saved to: C:\ProgramData\NavisBatchConverter\output" -ForegroundColor Gray