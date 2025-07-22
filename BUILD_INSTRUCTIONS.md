# Build Instructions for Navis Batch Converter

## Prerequisites

1. **Visual Studio 2019 or later** with:
   - .NET Framework 4.7.2 SDK
   - WPF Development workload

2. **Autodesk Revit 2021-2022** (for Revit API DLLs)

3. **RevitBatchProcessor** installed from:
   https://github.com/bvn-architecture/RevitBatchProcessor

## Build Steps

### 1. Open in Visual Studio

1. Open `NavisBatchConverter.sln` in Visual Studio
2. Wait for NuGet package restore to complete

### 2. Add Revit API References

1. Right-click on project → Add Reference
2. Browse to Revit installation folder (e.g., `C:\Program Files\Autodesk\Revit 2022`)
3. Add these DLLs:
   - `RevitAPI.dll`
   - `RevitAPIUI.dll`
4. Set "Copy Local" to False for both references

### 3. Update Project File

1. Open `NavisBatchConverter.csproj` in text editor
2. Uncomment the Revit API references section
3. Uncomment the Revit API dependent files:
   - `RevitExportTask.cs`
   - `WorksetManager.cs`
   - `ViewSelector.cs`
4. Comment out or delete the stub files:
   - `RevitExportTask_Stub.cs`
   - `WorksetManager_Stub.cs`
   - `ViewSelector_Stub.cs`

### 4. Build the Project

1. Set build configuration to Release
2. Build → Build Solution (or press F6)
3. The executable will be in `bin\Release\NavisBatchConverter.exe`

## Troubleshooting

### Missing Revit API Types

If you get errors about missing Revit API types:
1. Ensure Revit is installed
2. Verify the DLL paths in project references
3. Check that you're targeting the correct .NET Framework version

### NuGet Package Errors

If NuGet packages fail to restore:
1. Tools → NuGet Package Manager → Package Manager Console
2. Run: `Update-Package -reinstall`

### Material Design Errors

If you get XAML errors about Material Design:
1. Clean the solution
2. Rebuild
3. Close and reopen Visual Studio

## Running the Application

1. Ensure RevitBatchProcessor is installed
2. Run `NavisBatchConverter.exe` as Administrator (recommended)
3. Or use the PowerShell scripts directly:
   ```powershell
   .\scripts\RunBatchConverter.ps1 -InputFolder "C:\Your\Revit\Files"
   ```

## Development Notes

- The project uses stub files to allow compilation without Revit API
- Always test with actual Revit files before deployment
- Check logs in the `logs` directory for debugging