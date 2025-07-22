# Navis Batch Converter

A modern WPF application for batch converting Revit (.rvt) files to Navisworks (.nwc/.nwd) format using RevitBatchProcessor.

![Status](https://img.shields.io/badge/Status-Active%20Development-green)
![Version](https://img.shields.io/badge/Version-1.0.0--beta-blue)
![.NET](https://img.shields.io/badge/.NET-4.7.2-purple)

## Features

- ✅ **Modern UI**: Material Design with MahApps.Metro
- ✅ **Batch Processing**: Convert multiple Revit files at once
- ✅ **Drag & Drop**: Easy file management
- 🔄 **Smart Filtering**: Filter by views and worksets (in development)
- ✅ **Real-time Progress**: Track conversion status
- ✅ **Integrated Mode**: RevitBatchProcessor runs seamlessly in background
- ✅ **Error Handling**: Comprehensive error recovery and retry logic
- ✅ **Comprehensive Logging**: Detailed conversion logs

## Requirements

- Windows 10/11
- .NET Framework 4.7.2 or higher
- Autodesk Revit 2021-2022
- Autodesk Navisworks Manage/Simulate 2021-2022
- Navisworks Exporter for Revit 2021-2022
- RevitBatchProcessor v1.11.0+
- Python 2.7 (comes with RevitBatchProcessor)

## Installation

1. Clone the repository:
```bash
git clone https://github.com/BTankut/Navis_Batch_Converter.git
```

2. Install prerequisites:
   - Download and install [RevitBatchProcessor](https://github.com/bvn-architecture/RevitBatchProcessor/releases)
   - Install Navisworks Exporter for your Revit version from Autodesk

3. Build the project:
   - Open `NavisBatchConverter.sln` in Visual Studio
   - Restore NuGet packages
   - Build in Release mode

## Quick Start

1. Launch `NavisBatchConverter.exe`
2. Click "Add Files" or drag & drop Revit files into the window
3. Select your Revit version (2021 or 2022) from the dropdown
4. Configure export options (optional):
   - Output directory (default: `C:\Output\Navisworks`)
   - Export settings (links, parts, etc.)
5. Click "START" to begin conversion
6. Monitor progress in the progress dialog
7. Find your NWC files in the output directory

## Current Status

### Working Features ✅
- File selection and batch processing
- Real-time progress tracking
- Navisworks export via RevitBatchProcessor
- Error handling and logging
- Support for Revit 2021/2022

### Known Issues 🔧
- ASCII encoding error with Turkish characters in file paths (Revit 2021)
- Workset filtering not yet implemented
- View filtering not yet implemented
- NWC to NWD combination not yet implemented

## Configuration

Edit `scripts/Config.json` to customize:

```json
{
  "RevitVersion": "2022",
  "OutputDirectory": "C:\\Output\\Navisworks",
  "ViewFilter": {
    "Pattern": "navis_view",
    "CaseSensitive": false
  },
  "WorksetSettings": {
    "FilterWorksets": true,
    "IncludeWorksets": ["Architecture", "Structure", "MEP"]
  }
}
```

## Project Structure

```
Navis_Batch_Converter/
├── src/
│   ├── UI/               # WPF UI components
│   ├── Core/             # Business logic
│   └── Models/           # Data models
├── scripts/              # PowerShell scripts
├── Resources/            # Icons and images
├── logs/                 # Conversion logs
└── output/               # NWC/NWD output files
```

## Troubleshooting

### Common Issues

1. **"RevitBatchProcessor not found"**
   - Install from: https://github.com/bvn-architecture/RevitBatchProcessor/releases
   - Default installation path: `C:\Users\[Username]\AppData\Local\RevitBatchProcessor`

2. **"A Navisworks Exporter is not available"**
   - Install Navisworks Exporter for your Revit version
   - Available from Autodesk Desktop App or Account Portal

3. **ASCII encoding error (Turkish characters)**
   - Known issue with Revit 2021 and special characters
   - Workaround: Use Revit 2022 or rename files without special characters

4. **Progress dialog freezes**
   - Check RevitBatchProcessor logs in output directory
   - Ensure Revit license is valid and activated

### Logs

Check logs in `logs/` directory for detailed information.

## Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- [RevitBatchProcessor](https://github.com/bvn-architecture/RevitBatchProcessor) - Core automation engine
- [Material Design In XAML](http://materialdesigninxaml.net/) - UI components
- [MahApps.Metro](https://mahapps.com/) - Modern WPF controls

## Support

For issues and questions:
- GitHub Issues: https://github.com/BTankut/Navis_Batch_Converter/issues
- Email: support@example.com

---

Made with ❤️ by BTankut