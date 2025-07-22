# Navis Batch Converter

A modern WPF application for batch converting Revit (.rvt) files to Navisworks (.nwc/.nwd) format using RevitBatchProcessor.

## Features

- 🎨 **Modern UI**: Material Design with MahApps.Metro
- 🚀 **Batch Processing**: Convert multiple Revit files at once
- 📁 **Drag & Drop**: Easy file management
- 🔍 **Smart Filtering**: Filter by views and worksets
- 📊 **Real-time Progress**: Track conversion status
- 🔧 **Dual Mode**: Integrated CLI mode + Advanced GUI mode
- 🔄 **Auto Recovery**: Error handling and retry logic
- 📝 **Comprehensive Logging**: Detailed conversion logs

## Requirements

- Windows 10/11
- .NET Framework 4.7.2 or higher
- Autodesk Revit 2021-2022
- Autodesk Navisworks Manage 2021-2022
- RevitBatchProcessor (will be installed during setup)

## Installation

1. Clone the repository:
```bash
git clone https://github.com/BTankut/Navis_Batch_Converter.git
```

2. Run the installation script as Administrator:
```powershell
.\scripts\Install.ps1
```

3. Follow the prompts to complete installation

## Quick Start

1. Launch the application
2. Click "Add Files" or drag & drop Revit files
3. Configure export settings:
   - View filter pattern (default: "navis_view")
   - Workset selection
   - Export options
4. Click "Start" to begin conversion
5. Monitor progress in real-time

## Usage Modes

### Integrated Mode (Default)
- RevitBatchProcessor runs in background
- No additional windows appear
- Full progress tracking in main UI

### Advanced Mode
- Click "Advanced Mode" button
- Opens RevitBatchProcessor native GUI
- For complex scenarios requiring:
  - Python scripts
  - Dynamo graphs
  - Custom scheduling

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
   - Install from: https://github.com/bvn-architecture/RevitBatchProcessor

2. **"No views exported"**
   - Ensure view names contain filter pattern
   - Check if views are 3D type

3. **"Worksets not filtering"**
   - Verify workset names match configuration
   - Ensure model is workshared

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