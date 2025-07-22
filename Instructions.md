# Revit to Navisworks Batch Converter - Implementation Guide for Claude Code

## Project Overview
You will create an advanced automation solution with a **modern WPF GUI** that batch converts Revit (.rvt) files to Navisworks (.nwc/.nwd) format using RevitBatchProcessor. While your application will have a user-friendly interface, Revit and Navisworks will run in the background without opening their GUI applications.

## Github instruction
"https://github.com/BTankut/Navis_Batch_Converter" At the end of each task, commit your changes locally. At the end of each major step, also commit and push them to the remote repository.

## Core Requirements

### 1. Technology Stack
- **UI Framework**: WPF with Material Design
- **Primary Tool**: RevitBatchProcessor (GitHub: bvn-architecture/RevitBatchProcessor)
- **Execution Modes**: 
  - Integrated Mode (CLI - default, no BatchProcessor GUI)
  - Advanced Mode (Native GUI - optional, for power users)
- **Language**: C# (.NET Framework 4.7.2+)
- **Target Versions**: Revit/Navisworks 2021-2022
- **Additional**: PowerShell for orchestration, MahApps.Metro for modern UI

### 2. Key Features to Implement
- ✅ Modern WPF UI with Material Design
- ✅ Real-time progress tracking and visualization
- ✅ Drag & drop file support
- ✅ Background processing (Revit/Navisworks run without GUI)
- ✅ **Hybrid execution modes (Integrated CLI + Advanced GUI)**
- ✅ Workset filtering (user-selectable)
- ✅ 3D View filtering (views containing "navis_view")
- ✅ Batch processing multiple RVT files
- ✅ NWC to NWD combination
- ✅ Comprehensive logging with UI viewer
- ✅ Error recovery and retry logic
- ✅ Settings persistence
- ✅ Multi-threading support

## Implementation Steps

### Step 1: Project Structure
Create the following directory structure:
```
RevitNavisworksAutomation/
├── src/
│   ├── UI/
│   │   ├── MainWindow.xaml         # Main application window
│   │   ├── MainWindow.xaml.cs      # Code-behind
│   │   ├── ViewModels/             # MVVM ViewModels
│   │   │   ├── MainViewModel.cs
│   │   │   ├── FileListViewModel.cs
│   │   │   └── SettingsViewModel.cs
│   │   ├── Controls/               # Custom user controls
│   │   │   ├── FileListControl.xaml
│   │   │   ├── ProgressControl.xaml
│   │   │   └── SettingsPanel.xaml
│   │   └── Themes/                 # Material Design themes
│   ├── Core/
│   │   ├── RevitExportTask.cs      # Main export logic
│   │   ├── WorksetManager.cs       # Workset filtering
│   │   ├── ViewSelector.cs         # View filtering
│   │   ├── Logger.cs               # Logging utilities
│   │   └── ErrorHandler.cs         # Error management
│   ├── Models/
│   │   ├── ConversionJob.cs        # Job model
│   │   ├── ConversionSettings.cs   # Settings model
│   │   └── ConversionResult.cs     # Result model
├── scripts/
│   ├── RunBatchConverter.ps1       # PowerShell orchestrator
│   ├── Config.json                 # Configuration file
│   └── Install.ps1                 # Installation script
├── Resources/                       # Icons, images
├── logs/                           # Log output directory
├── output/                         # NWC/NWD output
├── App.xaml                        # WPF Application
├── App.xaml.cs
└── README.md                       # Documentation
```

### Step 2: Modern WPF User Interface

**File: `App.xaml`**
```xml
<Application x:Class="RevitNavisworksAutomation.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:materialDesign="http://materialdesigninxaml.net/winfx/xaml/themes"
             StartupUri="UI/MainWindow.xaml">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <!-- Material Design -->
                <materialDesign:BundledTheme BaseTheme="Light" PrimaryColor="Teal" SecondaryColor="Amber" />
                <ResourceDictionary Source="pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Defaults.xaml" />
                
                <!-- MahApps -->
                <ResourceDictionary Source="pack://application:,,,/MahApps.Metro;component/Styles/Controls.xaml" />
                <ResourceDictionary Source="pack://application:,,,/MahApps.Metro;component/Styles/Fonts.xaml" />
                <ResourceDictionary Source="pack://application:,,,/MahApps.Metro;component/Styles/Themes/Light.Teal.xaml" />
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

**File: `UI/MainWindow.xaml`**
```xml
<mah:MetroWindow x:Class="RevitNavisworksAutomation.UI.MainWindow"
                 xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                 xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                 xmlns:mah="clr-namespace:MahApps.Metro.Controls;assembly=MahApps.Metro"
                 xmlns:materialDesign="http://materialdesigninxaml.net/winfx/xaml/themes"
                 xmlns:local="clr-namespace:RevitNavisworksAutomation.UI.Controls"
                 Title="Revit to Navisworks Converter"
                 Height="800" Width="1200"
                 WindowStartupLocation="CenterScreen"
                 Icon="/Resources/app_icon.ico">
    
    <mah:MetroWindow.LeftWindowCommands>
        <mah:WindowCommands>
            <Button Content="Settings" Click="OpenSettings_Click">
                <Button.ContentTemplate>
                    <DataTemplate>
                        <StackPanel Orientation="Horizontal">
                            <materialDesign:PackIcon Kind="Settings" />
                            <TextBlock Margin="4 0 0 0" VerticalAlignment="Center" Text="{Binding}" />
                        </StackPanel>
                    </DataTemplate>
                </Button.ContentTemplate>
            </Button>
        </mah:WindowCommands>
    </mah:MetroWindow.LeftWindowCommands>
    
    <mah:MetroWindow.RightWindowCommands>
        <mah:WindowCommands>
            <Button Content="Advanced Mode" Click="OpenBatchProcessor_Click" 
                    ToolTip="Open native RevitBatchProcessor GUI for advanced features">
                <Button.ContentTemplate>
                    <DataTemplate>
                        <StackPanel Orientation="Horizontal">
                            <materialDesign:PackIcon Kind="OpenInNew" />
                            <TextBlock Margin="4 0 0 0" Text="{Binding}" />
                        </StackPanel>
                    </DataTemplate>
                </Button.ContentTemplate>
            </Button>
        </mah:WindowCommands>
    </mah:MetroWindow.RightWindowCommands>

    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Toolbar -->
        <materialDesign:Card Grid.Row="0" Margin="10" Padding="10">
            <ToolBar>
                <Button Command="{Binding AddFilesCommand}" ToolTip="Add Revit Files">
                    <StackPanel Orientation="Horizontal">
                        <materialDesign:PackIcon Kind="FilePlus" />
                        <TextBlock Margin="8,0" Text="Add Files" />
                    </StackPanel>
                </Button>
                
                <Button Command="{Binding AddFolderCommand}" ToolTip="Add Folder">
                    <StackPanel Orientation="Horizontal">
                        <materialDesign:PackIcon Kind="FolderPlus" />
                        <TextBlock Margin="8,0" Text="Add Folder" />
                    </StackPanel>
                </Button>
                
                <Separator />
                
                <Button Command="{Binding ClearAllCommand}" ToolTip="Clear All">
                    <StackPanel Orientation="Horizontal">
                        <materialDesign:PackIcon Kind="DeleteSweep" />
                        <TextBlock Margin="8,0" Text="Clear" />
                    </StackPanel>
                </Button>
                
                <Separator />
                
                <ComboBox materialDesign:HintAssist.Hint="Revit Version" 
                          ItemsSource="{Binding RevitVersions}" 
                          SelectedItem="{Binding SelectedRevitVersion}"
                          Width="120" Margin="10,0">
                    <ComboBox.ItemTemplate>
                        <DataTemplate>
                            <TextBlock Text="{Binding}" />
                        </DataTemplate>
                    </ComboBox.ItemTemplate>
                </ComboBox>
            </ToolBar>
        </materialDesign:Card>

        <!-- Main Content -->
        <Grid Grid.Row="1">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="3*"/>
                <ColumnDefinition Width="5"/>
                <ColumnDefinition Width="2*"/>
            </Grid.ColumnDefinitions>

            <!-- File List Panel -->
            <materialDesign:Card Grid.Column="0" Margin="10">
                <Grid>
                    <Grid.RowDefinitions>
                        <RowDefinition Height="Auto"/>
                        <RowDefinition Height="*"/>
                        <RowDefinition Height="Auto"/>
                    </Grid.RowDefinitions>

                    <TextBlock Grid.Row="0" Margin="16,16,16,8" 
                               Style="{StaticResource MaterialDesignHeadline6TextBlock}">
                        Revit Files ({Binding FileCount})
                    </TextBlock>

                    <DataGrid Grid.Row="1" 
                              ItemsSource="{Binding Files}"
                              AutoGenerateColumns="False"
                              CanUserAddRows="False"
                              materialDesign:DataGridAssist.CellPadding="13 8 8 8"
                              materialDesign:DataGridAssist.ColumnHeaderPadding="8">
                        <DataGrid.Columns>
                            <DataGridCheckBoxColumn Binding="{Binding IsSelected}" 
                                                    Header=""
                                                    Width="50"/>
                            
                            <DataGridTextColumn Binding="{Binding FileName}" 
                                                Header="File Name" 
                                                Width="*"/>
                            
                            <DataGridTextColumn Binding="{Binding FileSize}" 
                                                Header="Size" 
                                                Width="80"/>
                            
                            <DataGridTextColumn Binding="{Binding Status}" 
                                                Header="Status" 
                                                Width="100">
                                <DataGridTextColumn.ElementStyle>
                                    <Style TargetType="TextBlock">
                                        <Style.Triggers>
                                            <DataTrigger Binding="{Binding Status}" Value="Completed">
                                                <Setter Property="Foreground" Value="Green"/>
                                            </DataTrigger>
                                            <DataTrigger Binding="{Binding Status}" Value="Failed">
                                                <Setter Property="Foreground" Value="Red"/>
                                            </DataTrigger>
                                            <DataTrigger Binding="{Binding Status}" Value="Processing">
                                                <Setter Property="Foreground" Value="Orange"/>
                                            </DataTrigger>
                                        </Style.Triggers>
                                    </Style>
                                </DataGridTextColumn.ElementStyle>
                            </DataGridTextColumn>
                            
                            <DataGridTemplateColumn Header="Actions" Width="100">
                                <DataGridTemplateColumn.CellTemplate>
                                    <DataTemplate>
                                        <StackPanel Orientation="Horizontal">
                                            <Button Style="{StaticResource MaterialDesignFlatButton}"
                                                    Command="{Binding DataContext.RemoveFileCommand, RelativeSource={RelativeSource AncestorType=DataGrid}}"
                                                    CommandParameter="{Binding}"
                                                    ToolTip="Remove">
                                                <materialDesign:PackIcon Kind="Delete" />
                                            </Button>
                                            <Button Style="{StaticResource MaterialDesignFlatButton}"
                                                    Command="{Binding DataContext.ViewDetailsCommand, RelativeSource={RelativeSource AncestorType=DataGrid}}"
                                                    CommandParameter="{Binding}"
                                                    ToolTip="View Details">
                                                <materialDesign:PackIcon Kind="Information" />
                                            </Button>
                                        </StackPanel>
                                    </DataTemplate>
                                </DataGridTemplateColumn.CellTemplate>
                            </DataGridTemplateColumn>
                        </DataGrid.Columns>
                    </DataGrid>

                    <ProgressBar Grid.Row="2" 
                                 Value="{Binding OverallProgress}" 
                                 Height="5"
                                 Visibility="{Binding IsProcessing, Converter={StaticResource BooleanToVisibilityConverter}}"/>
                </Grid>
            </materialDesign:Card>

            <GridSplitter Grid.Column="1" HorizontalAlignment="Stretch" />

            <!-- Settings Panel -->
            <materialDesign:Card Grid.Column="2" Margin="10">
                <ScrollViewer>
                    <StackPanel Margin="16">
                        <TextBlock Style="{StaticResource MaterialDesignHeadline6TextBlock}" 
                                   Margin="0,0,0,16">
                            Export Settings
                        </TextBlock>

                        <!-- View Filter -->
                        <Expander Header="View Filter" IsExpanded="True">
                            <StackPanel Margin="0,8">
                                <TextBox materialDesign:HintAssist.Hint="View name contains"
                                         Text="{Binding ViewFilterPattern}"
                                         Margin="0,4"/>
                                
                                <CheckBox Content="Case sensitive" 
                                          IsChecked="{Binding ViewFilterCaseSensitive}"
                                          Margin="0,8"/>
                                
                                <CheckBox Content="Require sheet association" 
                                          IsChecked="{Binding RequireSheetAssociation}"
                                          Margin="0,4"/>
                            </StackPanel>
                        </Expander>

                        <!-- Workset Filter -->
                        <Expander Header="Workset Filter" IsExpanded="True" Margin="0,16,0,0">
                            <StackPanel Margin="0,8">
                                <CheckBox Content="Filter worksets" 
                                          IsChecked="{Binding FilterWorksets}"
                                          Margin="0,4"/>
                                
                                <TextBlock Text="Include worksets:" Margin="0,8,0,4"/>
                                <ItemsControl ItemsSource="{Binding WorksetFilters}">
                                    <ItemsControl.ItemTemplate>
                                        <DataTemplate>
                                            <CheckBox Content="{Binding Name}" 
                                                      IsChecked="{Binding IsIncluded}"
                                                      Margin="16,2"/>
                                        </DataTemplate>
                                    </ItemsControl.ItemTemplate>
                                </ItemsControl>
                                
                                <TextBox materialDesign:HintAssist.Hint="Add custom workset"
                                         Margin="16,8,0,0">
                                    <TextBox.InputBindings>
                                        <KeyBinding Key="Enter" 
                                                    Command="{Binding AddWorksetCommand}"
                                                    CommandParameter="{Binding RelativeSource={RelativeSource AncestorType=TextBox}, Path=Text}"/>
                                    </TextBox.InputBindings>
                                </TextBox>
                            </StackPanel>
                        </Expander>

                        <!-- Export Options -->
                        <Expander Header="Export Options" IsExpanded="True" Margin="0,16,0,0">
                            <StackPanel Margin="0,8">
                                <CheckBox Content="Export links" 
                                          IsChecked="{Binding ExportLinks}"
                                          Margin="0,4"/>
                                
                                <CheckBox Content="Export parts" 
                                          IsChecked="{Binding ExportParts}"
                                          Margin="0,4"/>
                                
                                <CheckBox Content="Use shared coordinates" 
                                          IsChecked="{Binding UseSharedCoordinates}"
                                          Margin="0,4"/>
                                
                                <CheckBox Content="Convert element properties" 
                                          IsChecked="{Binding ConvertElementProperties}"
                                          Margin="0,4"/>
                            </StackPanel>
                        </Expander>

                        <!-- Post Processing -->
                        <Expander Header="Post Processing" IsExpanded="False" Margin="0,16,0,0">
                            <StackPanel Margin="0,8">
                                <CheckBox Content="Combine to NWD" 
                                          IsChecked="{Binding CombineToNWD}"
                                          Margin="0,4"/>
                                
                                <CheckBox Content="Delete NWC after combine" 
                                          IsChecked="{Binding DeleteNWCAfterCombine}"
                                          IsEnabled="{Binding CombineToNWD}"
                                          Margin="0,4"/>
                                
                                <CheckBox Content="Compress NWD" 
                                          IsChecked="{Binding CompressNWD}"
                                          IsEnabled="{Binding CombineToNWD}"
                                          Margin="0,4"/>
                            </StackPanel>
                        </Expander>

                        <!-- Output Directory -->
                        <TextBlock Text="Output Directory:" Margin="0,16,0,4"/>
                        <Grid>
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="*"/>
                                <ColumnDefinition Width="Auto"/>
                            </Grid.ColumnDefinitions>
                            
                            <TextBox Grid.Column="0" 
                                     Text="{Binding OutputDirectory}"
                                     IsReadOnly="True"/>
                            
                            <Button Grid.Column="1" 
                                    Style="{StaticResource MaterialDesignFlatButton}"
                                    Command="{Binding BrowseOutputDirectoryCommand}"
                                    Margin="4,0,0,0">
                                <materialDesign:PackIcon Kind="FolderOpen" />
                            </Button>
                        </Grid>
                    </StackPanel>
                </ScrollViewer>
            </materialDesign:Card>
        </Grid>

        <!-- Status Bar & Action Buttons -->
        <materialDesign:Card Grid.Row="2" Margin="10" Padding="10">
            <Grid>
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="*"/>
                    <ColumnDefinition Width="Auto"/>
                </Grid.ColumnDefinitions>

                <!-- Status -->
                <StackPanel Grid.Column="0" Orientation="Horizontal" VerticalAlignment="Center">
                    <materialDesign:PackIcon Kind="{Binding StatusIcon}" 
                                             Foreground="{Binding StatusColor}"
                                             VerticalAlignment="Center"
                                             Margin="0,0,8,0"/>
                    <TextBlock Text="{Binding StatusMessage}" VerticalAlignment="Center"/>
                    
                    <TextBlock Text="{Binding ElapsedTime, StringFormat='Elapsed: {0}'}" 
                               Margin="20,0,0,0"
                               VerticalAlignment="Center"
                               Visibility="{Binding IsProcessing, Converter={StaticResource BooleanToVisibilityConverter}}"/>
                </StackPanel>

                <!-- Action Buttons -->
                <StackPanel Grid.Column="1" Orientation="Horizontal">
                    <Button Style="{StaticResource MaterialDesignRaisedButton}"
                            materialDesign:ButtonAssist.CornerRadius="20"
                            Command="{Binding StartConversionCommand}"
                            IsEnabled="{Binding CanStartConversion}"
                            Visibility="{Binding IsProcessing, Converter={StaticResource BooleanToVisibilityConverter}, ConverterParameter=Inverted}"
                            Margin="0,0,10,0"
                            Width="120">
                        <StackPanel Orientation="Horizontal">
                            <materialDesign:PackIcon Kind="Play" />
                            <TextBlock Margin="8,0" Text="Start" />
                        </StackPanel>
                    </Button>
                    
                    <Button Style="{StaticResource MaterialDesignRaisedAccentButton}"
                            materialDesign:ButtonAssist.CornerRadius="20"
                            Command="{Binding StopConversionCommand}"
                            Visibility="{Binding IsProcessing, Converter={StaticResource BooleanToVisibilityConverter}}"
                            Width="120">
                        <StackPanel Orientation="Horizontal">
                            <materialDesign:PackIcon Kind="Stop" />
                            <TextBlock Margin="8,0" Text="Stop" />
                        </StackPanel>
                    </Button>
                    
                    <Button Style="{StaticResource MaterialDesignFlatButton}"
                            Command="{Binding ViewLogsCommand}"
                            Margin="10,0,0,0">
                        <StackPanel Orientation="Horizontal">
                            <materialDesign:PackIcon Kind="FileDocument" />
                            <TextBlock Margin="8,0" Text="View Logs" />
                        </StackPanel>
                    </Button>
                </StackPanel>
            </Grid>
        </materialDesign:Card>

        <!-- Progress Overlay -->
        <materialDesign:DialogHost Grid.RowSpan="3" 
                                   IsOpen="{Binding ShowProgressDialog}"
                                   DialogTheme="Inherit">
            <materialDesign:DialogHost.DialogContent>
                <local:ProgressControl DataContext="{Binding ProgressViewModel}" />
            </materialDesign:DialogHost.DialogContent>
        </materialDesign:DialogHost>
    </Grid>
</mah:MetroWindow>
```

### Step 3: Core Export Task Implementation (CLI Mode)

**Important Note**: By default, we use RevitBatchProcessor in CLI mode (no GUI). The BatchProcessor window will NOT appear. For advanced users, we provide an "Advanced Mode" button to access the native BatchProcessor GUI when needed.

**File: `src/RevitExportTask.cs`**

```csharp
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.ApplicationServices;

namespace RevitNavisworksAutomation
{
    public class RevitExportTask
    {
        private static Logger _logger = new Logger();
        private static Config _config;

        public static void Execute(Document doc, string revitFilePath)
        {
            try
            {
                _config = Config.Load();
                _logger.Initialize(revitFilePath);
                
                _logger.Info($"Starting export for: {Path.GetFileName(revitFilePath)}");
                
                // Step 1: Find eligible views
                var targetViews = ViewSelector.GetNavisworksViews(doc, _config.ViewFilter);
                
                if (!targetViews.Any())
                {
                    _logger.Warning("No eligible views found. Skipping file.");
                    return;
                }
                
                // Step 2: Apply workset filtering
                if (doc.IsWorkshared && _config.FilterWorksets)
                {
                    WorksetManager.ApplyWorksetFilter(doc, _config.IncludeWorksets);
                }
                
                // Step 3: Export each view
                foreach (var view in targetViews)
                {
                    ExportViewToNWC(doc, view, revitFilePath);
                }
                
                _logger.Success($"Export completed for: {Path.GetFileName(revitFilePath)}");
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleExportError(ex, revitFilePath);
            }
        }
        
        private static void ExportViewToNWC(Document doc, View3D view, string revitFilePath)
        {
            var options = new NavisworksExportOptions
            {
                ExportScope = NavisworksExportScope.View,
                ViewId = view.Id,
                ExportLinks = _config.ExportLinks,
                ExportParts = _config.ExportParts,
                ExportRoomGeometry = false,
                Coordinates = _config.UseSharedCoordinates ? 
                    NavisworksCoordinates.Shared : NavisworksCoordinates.Internal,
                ExportElementIds = true,
                ConvertElementProperties = true,
                ExportUrls = false,
                FindMissingMaterials = true,
                DivideFileIntoLevels = false,
                ExportRoomAsAttribute = true
            };
            
            string outputPath = _config.OutputDirectory;
            string fileName = GenerateFileName(revitFilePath, view.Name);
            
            doc.Export(outputPath, fileName, options);
            _logger.Info($"Exported view '{view.Name}' to {fileName}.nwc");
        }
        
        private static string GenerateFileName(string revitPath, string viewName)
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var baseName = Path.GetFileNameWithoutExtension(revitPath);
            var safeName = SanitizeFileName(viewName);
            return $"{baseName}_{safeName}_{timestamp}";
        }
        
        private static string SanitizeFileName(string fileName)
        {
            return string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
        }
    }
}
```

### Step 3: Configuration System

**File: `scripts/Config.json`**
```json
{
  "RevitVersion": "2022",
  "OutputDirectory": "C:\\Output\\Navisworks",
  "LogDirectory": "C:\\Output\\Logs",
  "ViewFilter": {
    "Pattern": "navis_view",
    "CaseSensitive": false,
    "ExcludeTemplates": true,
    "RequireSheetAssociation": false
  },
  "WorksetSettings": {
    "FilterWorksets": true,
    "IncludeWorksets": ["Architecture", "Structure", "MEP"],
    "ExcludeWorksets": ["Coordination", "Linked Files"]
  },
  "ExportOptions": {
    "ExportLinks": true,
    "ExportParts": true,
    "UseSharedCoordinates": true,
    "ConvertUnits": false
  },
  "Processing": {
    "MaxRetries": 3,
    "RetryDelaySeconds": 5,
    "BatchSize": 50,
    "MaxParallelFiles": 1,
    "TimeoutMinutesPerFile": 30
  },
  "PostProcessing": {
    "CombineToNWD": true,
    "DeleteNWCAfterCombine": false,
    "CompressNWD": true
  }
}
```

### Step 4: Workset Manager Implementation

**File: `src/WorksetManager.cs`**

```csharp
using System;
using System.Linq;
using System.Collections.Generic;
using Autodesk.Revit.DB;

namespace RevitNavisworksAutomation
{
    public static class WorksetManager
    {
        private static Logger _logger = Logger.Instance;
        
        public static void ApplyWorksetFilter(Document doc, List<string> includePatterns)
        {
            if (!doc.IsWorkshared)
            {
                _logger.Warning("Document is not workshared. Skipping workset filtering.");
                return;
            }
            
            using (var trans = new Transaction(doc, "Configure Worksets for Export"))
            {
                trans.Start();
                
                try
                {
                    var allWorksets = new FilteredWorksetCollector(doc)
                        .OfKind(WorksetKind.UserWorkset)
                        .ToList();
                    
                    _logger.Info($"Found {allWorksets.Count} worksets in document");
                    
                    foreach (var workset in allWorksets)
                    {
                        bool shouldInclude = ShouldIncludeWorkset(workset.Name, includePatterns);
                        
                        // Set workset visibility for all 3D views
                        var views3D = new FilteredElementCollector(doc)
                            .OfClass(typeof(View3D))
                            .Cast<View3D>()
                            .Where(v => !v.IsTemplate);
                        
                        foreach (var view in views3D)
                        {
                            doc.GetWorksetTable().SetWorksetVisibility(
                                view, workset.Id, shouldInclude);
                        }
                        
                        _logger.Debug($"Workset '{workset.Name}' set to {(shouldInclude ? "visible" : "hidden")}");
                    }
                    
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.RollBack();
                    throw new InvalidOperationException($"Failed to configure worksets: {ex.Message}", ex);
                }
            }
        }
        
        private static bool ShouldIncludeWorkset(string worksetName, List<string> includePatterns)
        {
            return includePatterns.Any(pattern => 
                worksetName.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0);
        }
        
        public static Dictionary<string, bool> GetWorksetStatus(Document doc)
        {
            var result = new Dictionary<string, bool>();
            
            if (!doc.IsWorkshared) return result;
            
            var worksets = new FilteredWorksetCollector(doc)
                .OfKind(WorksetKind.UserWorkset);
            
            foreach (var workset in worksets)
            {
                result[workset.Name] = workset.IsOpen;
            }
            
            return result;
        }
    }
}
```

### Step 5: View Selector Implementation

**File: `src/ViewSelector.cs`**

```csharp
using System;
using System.Linq;
using System.Collections.Generic;
using Autodesk.Revit.DB;

namespace RevitNavisworksAutomation
{
    public static class ViewSelector
    {
        private static Logger _logger = Logger.Instance;
        
        public static List<View3D> GetNavisworksViews(Document doc, ViewFilterConfig config)
        {
            var collector = new FilteredElementCollector(doc)
                .OfClass(typeof(View3D))
                .Cast<View3D>();
            
            var filteredViews = collector.Where(view => 
                !view.IsTemplate &&
                MatchesPattern(view.Name, config.Pattern, config.CaseSensitive) &&
                (!config.RequireSheetAssociation || IsOnSheet(doc, view))
            ).ToList();
            
            _logger.Info($"Found {filteredViews.Count} views matching criteria");
            
            // Log detailed view information
            foreach (var view in filteredViews)
            {
                var info = GetViewInfo(doc, view);
                _logger.Debug($"View: {view.Name} | Level: {info.Level} | Phase: {info.Phase}");
            }
            
            return filteredViews;
        }
        
        private static bool MatchesPattern(string viewName, string pattern, bool caseSensitive)
        {
            var comparison = caseSensitive ? 
                StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            
            return viewName.IndexOf(pattern, comparison) >= 0;
        }
        
        private static bool IsOnSheet(Document doc, View3D view)
        {
            var viewports = new FilteredElementCollector(doc)
                .OfClass(typeof(Viewport))
                .Cast<Viewport>()
                .Where(vp => vp.ViewId == view.Id);
            
            return viewports.Any();
        }
        
        private static ViewInfo GetViewInfo(Document doc, View3D view)
        {
            return new ViewInfo
            {
                Name = view.Name,
                Level = view.GenLevel?.Name ?? "N/A",
                Phase = doc.GetElement(view.get_Parameter(
                    BuiltInParameter.VIEW_PHASE).AsElementId())?.Name ?? "N/A"
            };
        }
    }
    
    public class ViewFilterConfig
    {
        public string Pattern { get; set; }
        public bool CaseSensitive { get; set; }
        public bool ExcludeTemplates { get; set; }
        public bool RequireSheetAssociation { get; set; }
    }
    
    public class ViewInfo
    {
        public string Name { get; set; }
        public string Level { get; set; }
        public string Phase { get; set; }
    }
}
```

### Step 7: PowerShell Orchestrator (CLI Mode)

**File: `scripts/RunBatchConverter.ps1`**

**Note**: This script runs RevitBatchProcessor in CLI mode without showing its GUI. This is the default mode for our integrated experience.

```powershell
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
    
    # Step 3: Execute RevitBatchProcessor
    Write-Host "`nStep 1/3: Exporting NWC files from Revit..." -ForegroundColor Green
    
    $scriptPath = Join-Path $PSScriptRoot "..\src\RevitExportTask.cs"
    $logFolder = Join-Path $config.LogDirectory $timestamp
    
    $rbpArgs = @(
        "--task_script", $scriptPath,
        "--file_list", $fileListPath,
        "--revit_version", $RevitVersion,
        "--log_folder", $logFolder,
        "--detach",
        "--worksets", "open_all"
    )
    
    if ($config.Processing.MaxParallelFiles -gt 1) {
        $rbpArgs += "--max_parallel_processes", $config.Processing.MaxParallelFiles
    }
    
    $rbpProcess = Start-Process -FilePath $tools.RBP -ArgumentList $rbpArgs -NoNewWindow -PassThru
    
    # Monitor progress
    $startTime = Get-Date
    while (!$rbpProcess.HasExited) {
        $elapsed = (Get-Date) - $startTime
        Write-Progress -Activity "Processing Revit files" `
                      -Status "Elapsed: $($elapsed.ToString('hh\:mm\:ss'))" `
                      -PercentComplete -1
        Start-Sleep -Seconds 5
    }
    
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
            
            & $tools.Navis /i $nwcListPath /of $nwdOutput /over /version $RevitVersion
            
            Write-Host "Created NWD: $nwdOutput" -ForegroundColor Cyan
            
            # Optional: Delete NWC files
            if ($config.PostProcessing.DeleteNWCAfterCombine) {
                $nwcFiles | Remove-Item -Force
                Write-Host "Cleaned up $($nwcFiles.Count) NWC files" -ForegroundColor Gray
            }
        }
    }
    
    # Step 5: Generate summary report
    Write-Host "`nStep 3/3: Generating summary report..." -ForegroundColor Green
    
    $summaryPath = Join-Path $logFolder "summary.txt"
    $summary = @"
Revit to Navisworks Batch Conversion Summary
============================================
Start Time: $($startTime.ToString('yyyy-MM-dd HH:mm:ss'))
End Time: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
Duration: $((Get-Date) - $startTime)

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
"@
    
    $summary | Out-File $summaryPath -Encoding UTF8
    Write-Host "`nConversion completed successfully!" -ForegroundColor Green
    Write-Host "Summary saved to: $summaryPath" -ForegroundColor Gray
    
} catch {
    Write-Error "Batch conversion failed: $_"
    exit 1
} finally {
    # Cleanup temp files
    if (Test-Path $fileListPath) { Remove-Item $fileListPath -Force }
    if (Test-Path $nwcListPath) { Remove-Item $nwcListPath -Force }
}
```

### Step 7: Installation Script

**File: `scripts/Install.ps1`**

```powershell
#Requires -RunAsAdministrator

Write-Host "Installing Revit to Navisworks Automation Solution..." -ForegroundColor Cyan

# 1. Check RevitBatchProcessor
$rbpUrl = "https://github.com/bvn-architecture/RevitBatchProcessor/releases/latest"
Write-Host "`nChecking RevitBatchProcessor installation..."

if (!(Test-Path "$env:LOCALAPPDATA\RevitBatchProcessor\BatchRvt.exe")) {
    Write-Host "Please install RevitBatchProcessor from: $rbpUrl" -ForegroundColor Yellow
    Start-Process $rbpUrl
    Read-Host "Press Enter after installation is complete"
}

# 2. Create directories
$dirs = @(
    "C:\RevitNavisworksAutomation",
    "C:\RevitNavisworksAutomation\logs",
    "C:\RevitNavisworksAutomation\output",
    "C:\RevitNavisworksAutomation\config"
)

foreach ($dir in $dirs) {
    if (!(Test-Path $dir)) {
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
        Write-Host "Created: $dir" -ForegroundColor Green
    }
}

# 3. Copy files
Copy-Item -Path ".\*" -Destination "C:\RevitNavisworksAutomation\" -Recurse -Force

# 4. Create scheduled task (optional)
$createTask = Read-Host "`nCreate scheduled task for daily automation? (Y/N)"
if ($createTask -eq 'Y') {
    $taskName = "RevitNavisworksConversion"
    $action = New-ScheduledTaskAction -Execute "PowerShell.exe" `
        -Argument "-File C:\RevitNavisworksAutomation\scripts\RunBatchConverter.ps1 -InputFolder C:\Projects\Revit"
    
    $trigger = New-ScheduledTaskTrigger -Daily -At "2:00AM"
    
    Register-ScheduledTask -TaskName $taskName -Action $action -Trigger $trigger `
        -Description "Automated Revit to Navisworks conversion"
    
    Write-Host "Scheduled task created: $taskName" -ForegroundColor Green
}

Write-Host "`nInstallation complete!" -ForegroundColor Green
Write-Host "Run the converter using:" -ForegroundColor Cyan
Write-Host "  .\scripts\RunBatchConverter.ps1 -InputFolder 'C:\Your\Revit\Files'" -ForegroundColor White
```

## Critical Implementation Notes

### 1. **RevitBatchProcessor Setup**
- Download from: https://github.com/bvn-architecture/RevitBatchProcessor/releases
- Install to: `%LOCALAPPDATA%\RevitBatchProcessor\`
- **Important**: We'll use it in CLI mode by default (no GUI)
- The native GUI is only opened when user clicks "Advanced Mode"
- Ensure the C# task script references correct Revit API DLLs

### 2. **API References Required**
```xml
<!-- Revit API -->
<Reference Include="RevitAPI">
  <HintPath>C:\Program Files\Autodesk\Revit 2022\RevitAPI.dll</HintPath>
</Reference>
<Reference Include="RevitAPIUI">
  <HintPath>C:\Program Files\Autodesk\Revit 2022\RevitAPIUI.dll</HintPath>
</Reference>

<!-- NuGet Packages for UI -->
<PackageReference Include="MaterialDesignThemes" Version="4.6.1" />
<PackageReference Include="MahApps.Metro" Version="2.4.9" />
<PackageReference Include="Microsoft.Xaml.Behaviors.Wpf" Version="1.1.39" />
<PackageReference Include="Prism.Core" Version="8.1.97" />
```

### 3. **Error Handling Strategy**
- Implement try-catch blocks at every level
- Log all errors with full stack traces
- Use transaction rollback for Revit operations
- Implement retry logic with exponential backoff

### 4. **Performance Optimization**
- Process files in batches to manage memory
- Force garbage collection between batches
- Use async/await for I/O operations where possible
- Monitor system resources during execution

### 5. **Testing Checklist**
- [ ] Test UI responsiveness during conversion
- [ ] Test drag & drop functionality
- [ ] Test progress indicators and real-time updates
- [ ] **Verify Revit/Navisworks run without showing GUI in Integrated mode**
- [ ] **Test Advanced Mode button opens native BatchProcessor GUI**
- [ ] **Verify CLI mode doesn't show any BatchProcessor windows**
- [ ] Test with single file first
- [ ] Test with workshared models
- [ ] Test with linked models
- [ ] Test view filtering logic
- [ ] Test workset filtering
- [ ] Test error recovery
- [ ] Test NWD combination
- [ ] Test settings persistence
- [ ] Test cancel/stop functionality

## Advanced Features to Add

### 1. **Email Notifications**
```powershell
Send-MailMessage -To "team@company.com" `
    -Subject "Navisworks Conversion Complete" `
    -Body $summary `
    -SmtpServer "smtp.company.com"
```

### 2. **Database Logging**
- Log conversion history to SQL database
- Track file versions and changes
- Generate analytics reports

### 3. **Web Dashboard**
- Real-time conversion status
- Queue management
- Historical reports

### 4. **Cloud Integration**
- Upload NWD files to BIM 360
- Sync with cloud storage
- Remote monitoring

## UI ViewModels Implementation

### MainViewModel.cs
```csharp
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Linq;

namespace RevitNavisworksAutomation.UI.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly IConversionService _conversionService;
        private readonly IDialogService _dialogService;
        private readonly ILogger _logger;

        public ObservableCollection<ConversionJob> Files { get; }
        public ObservableCollection<string> RevitVersions { get; }
        public ObservableCollection<WorksetFilter> WorksetFilters { get; }

        // Commands
        public ICommand AddFilesCommand { get; }
        public ICommand AddFolderCommand { get; }
        public ICommand ClearAllCommand { get; }
        public ICommand StartConversionCommand { get; }
        public ICommand StopConversionCommand { get; }
        public ICommand RemoveFileCommand { get; }
        public ICommand ViewDetailsCommand { get; }
        public ICommand ViewLogsCommand { get; }
        public ICommand BrowseOutputDirectoryCommand { get; }
        public ICommand AddWorksetCommand { get; }

        // Properties
        private bool _isProcessing;
        public bool IsProcessing
        {
            get => _isProcessing;
            set { _isProcessing = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanStartConversion)); }
        }

        private string _statusMessage = "Ready";
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        private double _overallProgress;
        public double OverallProgress
        {
            get => _overallProgress;
            set { _overallProgress = value; OnPropertyChanged(); }
        }

        private string _selectedRevitVersion = "2022";
        public string SelectedRevitVersion
        {
            get => _selectedRevitVersion;
            set { _selectedRevitVersion = value; OnPropertyChanged(); SaveSettings(); }
        }

        private string _viewFilterPattern = "navis_view";
        public string ViewFilterPattern
        {
            get => _viewFilterPattern;
            set { _viewFilterPattern = value; OnPropertyChanged(); SaveSettings(); }
        }

        private string _outputDirectory = @"C:\Output\Navisworks";
        public string OutputDirectory
        {
            get => _outputDirectory;
            set { _outputDirectory = value; OnPropertyChanged(); SaveSettings(); }
        }

        public int FileCount => Files?.Count ?? 0;
        public bool CanStartConversion => !IsProcessing && Files.Any(f => f.IsSelected);

        public MainViewModel(IConversionService conversionService, IDialogService dialogService, ILogger logger)
        {
            _conversionService = conversionService;
            _dialogService = dialogService;
            _logger = logger;

            Files = new ObservableCollection<ConversionJob>();
            RevitVersions = new ObservableCollection<string> { "2021", "2022", "2023", "2024" };
            WorksetFilters = new ObservableCollection<WorksetFilter>
            {
                new WorksetFilter { Name = "Architecture", IsIncluded = true },
                new WorksetFilter { Name = "Structure", IsIncluded = true },
                new WorksetFilter { Name = "MEP", IsIncluded = true },
                new WorksetFilter { Name = "Coordination", IsIncluded = false }
            };

            InitializeCommands();
            LoadSettings();
        }

        private void InitializeCommands()
        {
            AddFilesCommand = new RelayCommand(async () => await AddFiles());
            AddFolderCommand = new RelayCommand(async () => await AddFolder());
            ClearAllCommand = new RelayCommand(() => Files.Clear());
            StartConversionCommand = new RelayCommand(async () => await StartConversion(), () => CanStartConversion);
            StopConversionCommand = new RelayCommand(() => _conversionService.CancelConversion());
            RemoveFileCommand = new RelayCommand<ConversionJob>(RemoveFile);
            ViewDetailsCommand = new RelayCommand<ConversionJob>(async (job) => await ViewDetails(job));
            ViewLogsCommand = new RelayCommand(() => System.Diagnostics.Process.Start("explorer.exe", _logger.LogDirectory));
            BrowseOutputDirectoryCommand = new RelayCommand(BrowseOutputDirectory);
            AddWorksetCommand = new RelayCommand<string>(AddWorkset);
        }

        private async Task AddFiles()
        {
            var files = await _dialogService.ShowFilePickerAsync("Revit Files|*.rvt", multiSelect: true);
            if (files != null)
            {
                foreach (var file in files)
                {
                    if (!Files.Any(f => f.FilePath == file))
                    {
                        Files.Add(new ConversionJob(file));
                    }
                }
                OnPropertyChanged(nameof(FileCount));
            }
        }

        private async Task StartConversion()
        {
            try
            {
                IsProcessing = true;
                StatusMessage = "Starting conversion...";

                var settings = CreateConversionSettings();
                var selectedFiles = Files.Where(f => f.IsSelected).ToList();

                _logger.Info($"Starting conversion of {selectedFiles.Count} files");

                await _conversionService.ConvertFilesAsync(selectedFiles, settings, 
                    new Progress<ConversionProgress>(OnProgressUpdated));

                StatusMessage = "Conversion completed successfully!";
                await _dialogService.ShowMessageAsync("Success", 
                    $"Successfully converted {selectedFiles.Count} files.");
            }
            catch (Exception ex)
            {
                _logger.Error($"Conversion failed: {ex.Message}", ex);
                StatusMessage = "Conversion failed!";
                await _dialogService.ShowErrorAsync("Conversion Error", ex.Message);
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private void OnProgressUpdated(ConversionProgress progress)
        {
            OverallProgress = progress.OverallPercentage;
            StatusMessage = progress.CurrentOperation;
            
            if (progress.CurrentJob != null)
            {
                progress.CurrentJob.Status = progress.Status;
                progress.CurrentJob.Progress = progress.FilePercentage;
            }
        }

        private ConversionSettings CreateConversionSettings()
        {
            return new ConversionSettings
            {
                RevitVersion = SelectedRevitVersion,
                OutputDirectory = OutputDirectory,
                ViewFilter = new ViewFilterConfig
                {
                    Pattern = ViewFilterPattern,
                    CaseSensitive = ViewFilterCaseSensitive,
                    ExcludeTemplates = true,
                    RequireSheetAssociation = RequireSheetAssociation
                },
                WorksetSettings = new WorksetSettings
                {
                    FilterWorksets = FilterWorksets,
                    IncludeWorksets = WorksetFilters.Where(w => w.IsIncluded)
                                                   .Select(w => w.Name)
                                                   .ToList()
                },
                ExportOptions = new ExportOptions
                {
                    ExportLinks = ExportLinks,
                    ExportParts = ExportParts,
                    UseSharedCoordinates = UseSharedCoordinates,
                    ConvertElementProperties = ConvertElementProperties
                },
                PostProcessing = new PostProcessingOptions
                {
                    CombineToNWD = CombineToNWD,
                    DeleteNWCAfterCombine = DeleteNWCAfterCombine,
                    CompressNWD = CompressNWD
                }
            };
        }

        // Property change notification
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Supporting classes
    public class WorksetFilter : INotifyPropertyChanged
    {
        private string _name;
        private bool _isIncluded;

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public bool IsIncluded
        {
            get => _isIncluded;
            set { _isIncluded = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
```

### MainWindow Code-Behind Implementation
**File: `UI/MainWindow.xaml.cs`**

```csharp
using System;
using System.IO;
using System.Diagnostics;
using System.Windows;
using MahApps.Metro.Controls;
using Newtonsoft.Json;
using System.Linq;

namespace RevitNavisworksAutomation.UI
{
    public partial class MainWindow : MetroWindow
    {
        private MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel(
                new ConversionService(),
                new DialogService(),
                new Logger()
            );
            DataContext = _viewModel;
        }

        private void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow();
            settingsWindow.Owner = this;
            settingsWindow.ShowDialog();
        }

        private void OpenBatchProcessor_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "This will open RevitBatchProcessor's native interface.\n\n" +
                "Features available in Advanced Mode:\n" +
                "• Python script support\n" +
                "• Dynamo graph execution\n" +
                "• Advanced task scheduling\n" +
                "• Detailed session logging\n\n" +
                "The native interface will open in a separate window.\n" +
                "Continue?",
                "Advanced Mode",
                MessageBoxButton.YesNo,
                MessageBoxImage.Information);

            if (result == MessageBoxResult.Yes)
            {
                LaunchBatchProcessorGUI();
            }
        }

        private void LaunchBatchProcessorGUI()
        {
            try
            {
                var rbpPath = $@"{Environment.GetEnvironmentVariable("LOCALAPPDATA")}\RevitBatchProcessor\BatchRvt.exe";
                
                if (!File.Exists(rbpPath))
                {
                    MessageBox.Show(
                        "RevitBatchProcessor not found!\n\n" +
                        "Please install it from:\n" +
                        "https://github.com/bvn-architecture/RevitBatchProcessor",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                // Prepare configuration for BatchProcessor
                PrepareAdvancedModeConfiguration();

                // Launch native GUI
                var processInfo = new ProcessStartInfo
                {
                    FileName = rbpPath,
                    UseShellExecute = true,
                    WorkingDirectory = Path.GetDirectoryName(rbpPath)
                };

                Process.Start(processInfo);

                // Log the action
                _viewModel.Logger.Info("Launched RevitBatchProcessor in Advanced Mode");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to launch Advanced Mode:\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void PrepareAdvancedModeConfiguration()
        {
            // Export current file list for BatchProcessor
            var selectedFiles = _viewModel.Files
                .Where(f => f.IsSelected)
                .Select(f => f.FilePath)
                .ToList();

            if (selectedFiles.Any())
            {
                var tempDir = Path.Combine(Path.GetTempPath(), "RevitNavisworksAutomation");
                Directory.CreateDirectory(tempDir);

                // Create file list
                var fileListPath = Path.Combine(tempDir, $"files_{DateTime.Now:yyyyMMddHHmmss}.txt");
                File.WriteAllLines(fileListPath, selectedFiles);

                // Create config file for BatchProcessor
                var config = new
                {
                    TaskScriptFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, 
                                                      "Scripts", "NavisExport.cs"),
                    RevitFileListFilePath = fileListPath,
                    DataExportFolderPath = _viewModel.OutputDirectory,
                    RevitSessionOption = "UseSeparateSessionPerFile",
                    RevitProcessingOption = "BatchRevitFileProcessing",
                    CentralFileOpenOption = "Detach",
                    DeleteLocalAfter = false,
                    DiscardWorksetsOnDetach = false,
                    AuditOnOpen = false,
                    ProcessingTimeOutMinutes = 60
                };

                var configPath = Path.Combine(tempDir, "batchprocessor_config.json");
                File.WriteAllText(configPath, JsonConvert.SerializeObject(config, Formatting.Indented));

                // Copy the config to clipboard for easy paste in BatchProcessor
                Clipboard.SetText(configPath);
                
                MessageBox.Show(
                    "Configuration prepared!\n\n" +
                    "The config file path has been copied to clipboard.\n" +
                    "You can paste it in BatchProcessor's settings.",
                    "Advanced Mode Ready",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _viewModel.SaveSettings();
        }
    }
}
```

### Progress Control XAML
```xml
<UserControl x:Class="RevitNavisworksAutomation.UI.Controls.ProgressControl"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:materialDesign="http://materialdesigninxaml.net/winfx/xaml/themes"
             Width="500" Height="300">
    <materialDesign:Card Padding="20">
        <Grid>
            <Grid.RowDefinitions>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="*"/>
                <RowDefinition Height="Auto"/>
            </Grid.RowDefinitions>

            <TextBlock Grid.Row="0" 
                       Text="Conversion Progress" 
                       Style="{StaticResource MaterialDesignHeadline5TextBlock}"
                       HorizontalAlignment="Center"
                       Margin="0,0,0,20"/>

            <!-- Current File Progress -->
            <StackPanel Grid.Row="1" Margin="0,0,0,20">
                <TextBlock Text="{Binding CurrentFileName}" 
                           HorizontalAlignment="Center"
                           Margin="0,0,0,8"/>
                
                <ProgressBar Value="{Binding CurrentFileProgress}" 
                             Height="8"
                             materialDesign:TransitionAssist.DisableTransitions="True"/>
                
                <TextBlock Text="{Binding CurrentOperation}" 
                           HorizontalAlignment="Center"
                           Margin="0,8,0,0"
                           FontSize="12"
                           Foreground="{DynamicResource MaterialDesignBodyLight}"/>
            </StackPanel>

            <!-- File List -->
            <ScrollViewer Grid.Row="2" VerticalScrollBarVisibility="Auto">
                <ItemsControl ItemsSource="{Binding ProcessingJobs}">
                    <ItemsControl.ItemTemplate>
                        <DataTemplate>
                            <Border BorderBrush="{DynamicResource MaterialDesignDivider}" 
                                    BorderThickness="0,0,0,1"
                                    Padding="0,8">
                                <Grid>
                                    <Grid.ColumnDefinitions>
                                        <ColumnDefinition Width="Auto"/>
                                        <ColumnDefinition Width="*"/>
                                        <ColumnDefinition Width="Auto"/>
                                    </Grid.ColumnDefinitions>

                                    <materialDesign:PackIcon Grid.Column="0" 
                                                             Kind="{Binding StatusIcon}" 
                                                             Foreground="{Binding StatusColor}"
                                                             VerticalAlignment="Center"
                                                             Margin="0,0,8,0"/>
                                    
                                    <TextBlock Grid.Column="1" 
                                               Text="{Binding FileName}"
                                               VerticalAlignment="Center"/>
                                    
                                    <TextBlock Grid.Column="2" 
                                               Text="{Binding ElapsedTime}"
                                               VerticalAlignment="Center"
                                               FontSize="12"
                                               Foreground="{DynamicResource MaterialDesignBodyLight}"/>
                                </Grid>
                            </Border>
                        </DataTemplate>
                    </ItemsControl.ItemTemplate>
                </ItemsControl>
            </ScrollViewer>

            <!-- Overall Progress -->
            <Grid Grid.Row="3" Margin="0,20,0,0">
                <Grid.RowDefinitions>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="Auto"/>
                </Grid.RowDefinitions>

                <TextBlock Grid.Row="0" 
                           Text="{Binding OverallProgressText}" 
                           HorizontalAlignment="Center"
                           Margin="0,0,0,8"/>
                
                <ProgressBar Grid.Row="1" 
                             Value="{Binding OverallProgress}"
                             Height="10"/>
            </Grid>
        </Grid>
    </materialDesign:Card>
</UserControl>
```

## UI Design Mockup

The application should have a modern, professional look with the following layout:

```
┌─────────────────────────────────────────────────────────────────┐
│ 🔧 Revit to Navisworks Converter   [Settings] [Advanced Mode] [X] │
├─────────────────────────────────────────────────────────────────┤
│ [+ Add Files] [+ Add Folder] [🗑 Clear] | Revit Version: [2022▼] │
├─────────────────────────────────────────────────────────────────┤
│ ┌─────────────────────────────────┬──────────────────────────┐ │
│ │ Revit Files (12)                │ Export Settings          │ │
│ │ ┌─────────────────────────────┐ │ ┌──────────────────────┐ │ │
│ │ │ ☑ Model1.rvt  2.3MB ✓Done  │ │ │ ▼ View Filter        │ │ │
│ │ │ ☑ Model2.rvt  1.8MB ⟳Proc. │ │ │   [navis_view    ]   │ │ │
│ │ │ ☐ Model3.rvt  3.1MB ⏸Wait  │ │ │   ☐ Case sensitive   │ │ │
│ │ │ ☑ Model4.rvt  2.5MB ✗Fail  │ │ │ ▼ Workset Filter     │ │ │
│ │ └─────────────────────────────┘ │ │   ☑ Architecture     │ │ │
│ │ [████████████░░░░░] 65%        │ │   ☑ Structure        │ │ │
│ └─────────────────────────────────┴──────────────────────────┘ │
│ 🟢 Processing file 8 of 12... | Elapsed: 00:12:34  [▶ START]   │
└─────────────────────────────────────────────────────────────────┘
```

Key UI Elements:
- **Material Design** components with teal primary color
- **Advanced Mode** button for accessing native BatchProcessor GUI
- **File list** with checkboxes, status icons, and progress
- **Settings panel** with collapsible sections
- **Real-time status** updates and elapsed time
- **Progress bars** for individual files and overall progress
- **Modern buttons** with icons and rounded corners

### Common Issues:

1. **"Revit API not found"**
   - Ensure correct Revit version installed
   - Check API DLL paths in project references

2. **"Worksets not filtering correctly"**
   - Verify workset names in config match exactly
   - Check if model is workshared

3. **"No views exported"**
   - Confirm view names contain filter pattern
   - Check view is not a template
   - Verify 3D view type

4. **"Memory errors with large files"**
   - Reduce batch size in config
   - Increase system page file
   - Process files sequentially

## Support Resources

- RevitBatchProcessor Docs: https://github.com/bvn-architecture/RevitBatchProcessor/wiki
- Revit API Forum: https://forums.autodesk.com/t5/revit-api-forum/bd-p/160
- Navisworks API Forum: https://forums.autodesk.com/t5/navisworks-api-forum/bd-p/navisworks-api-forum-en

---

## Execution Modes Summary

### 🎯 **Default: Integrated Mode (CLI)**
- RevitBatchProcessor runs in background without GUI
- Full progress tracking in our modern UI
- Perfect for standard conversion tasks
- No additional windows appear

### 🚀 **Advanced Mode (Native GUI)**
- Accessed via "Advanced Mode" button
- Opens RevitBatchProcessor's native interface
- For complex scenarios requiring:
  - Python scripts
  - Dynamo graphs
  - Custom task scheduling
  - Advanced debugging

### 🤖 **Hybrid Approach Benefits**
1. **Best of Both Worlds**: Simple UI for most users, power features when needed
2. **No Conflicts**: Modes are separate and don't interfere
3. **User Choice**: Users decide which mode fits their needs
4. **Professional Experience**: Clean, modern interface by default

### 📝 **Implementation Key Points**
- CLI arguments include `--show_message_boxes "No"` to prevent popups
- `CreateNoWindow = true` ensures BatchProcessor GUI doesn't appear
- Advanced Mode launches with `UseShellExecute = true` to show GUI
- Configuration is shared between modes for consistency

**Note for Claude Code**: Follow this guide step-by-step to create a modern WPF application with Material Design UI. The application should have a professional, user-friendly interface while ensuring Revit and Navisworks run in the background without showing their GUI. Start with the basic structure, implement the UI components, then add the core conversion logic. Test frequently to ensure a smooth user experience.