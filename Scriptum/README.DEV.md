# Scriptum Development Guide

## Project Structure

```
Scriptum/
├── Models/              # Data entities (Note, Notebook) and DTOs
│   └── DTOs/            # MessagePack-serializable data transfer objects
├── Data/                # Database context and EF Core setup
├── Repositories/        # Repository pattern implementation
├── Services/            # Business logic services
│   ├── DataService.cs           # Notebook/note CRUD operations
│   ├── OpenCvImageProcessingService.cs  # Image preprocessing
│   ├── QwenVlRecognitionService.cs      # Handwriting recognition
│   ├── StrokeSerializer.cs            # MessagePack stroke serialization
│   └── StrokeToImageRenderer.cs       # Win2D stroke rendering
├── ViewModels/          # MVVM ViewModels
├── Views/               # XAML views
│   └── Settings/        # Settings page
├── Controls/            # Custom controls (InkCanvasControl)
├── Converters/          # XAML value converters
├── Helpers/             # Utility classes
│   └── AppSettingsHelper.cs  # Application settings management
└── StringResources.cs   # Centralized string constants
```

## Architecture

- **MVVM Pattern**: Separation of concerns with ViewModels
- **Repository Pattern**: Abstracted data access layer
- **Service-Based Architecture**: Loosely coupled services
- **EF Core**: SQLite database with SQLCipher encryption

## Key Features Implemented

### Phase 1-2: Foundation ✅
- WinUI 3 project structure
- SQLCipher-encrypted SQLite database
- Entity Framework Core integration

### Phase 3: Data Layer ✅
- Notebook and Note models
- Repository pattern with generic and specialized repositories
- DataService for CRUD operations

### Phase 4: UI Components ✅
- InkCanvasControl for handwriting input
- NoteEditorView with tool selection
- MainView with notebook management

### Phase 5: Services ✅
- OpenCvImageProcessingService for image preprocessing
- QwenVlRecognitionService for AI handwriting recognition
- StrokeToImageRenderer for rendering strokes to images

### Phase 6: Data Serialization ✅
- MessagePack DTOs (StrokePoint, StrokeData)
- StrokeSerializer for efficient binary serialization
- Stroke persistence in database

### Phase 7: Settings & Configuration ✅
- AppSettingsHelper for persistent settings
- SettingsView for API key configuration
- Theme support (Light/Dark/System)
- BoolNegationConverter and EmptyStringToVisibilityConverter

### Phase 8: Undo/Redo Functionality ✅
- IUndoRedoService interface for state management
- UndoRedoService implementation with stack-based history
- Integration with NoteEditorViewModel
- Undo/Redo commands in UI toolbar
- State snapshot cloning for deep copy

### Phase 9: Export Functionality ✅
- IExportService interface for export operations
- ExportService implementation for PNG and PDF export
- ExportToPngAsync command in ViewModel
- Progress indicators for export operations
- Integration with StrokeToImageRenderer

## Dependencies

- .NET 8.0 SDK
- Windows App SDK 1.4+
- Win2D.uwp 1.26.0
- OpenCvSharp4 4.9.0 (+ Extensions, Windows)
- MessagePack 2.5.140
- CommunityToolkit.Mvvm 8.2.2
- Microsoft.EntityFrameworkCore.Sqlite.Core 8.0.0
- SQLitePCLRaw.bundle_sqlcipher 2.1.6

## Configuration

### Qwen-VL API Key

Configure through the Settings UI or programmatically:

```csharp
// Via settings helper
AppSettingsHelper.QwenApiKey = "your-api-key";

// Or retrieve
var apiKey = AppSettingsHelper.QwenApiKey;
```

The application will automatically use the configured API key for handwriting recognition.

## Build Instructions

1. Restore NuGet packages:
   ```bash
   dotnet restore
   ```

2. Build the project:
   ```bash
   dotnet build
   ```

3. Run the application (Windows only):
   ```bash
   dotnet run
   ```

## Next Steps

1. **Canvas Performance**: Optimize Win2D rendering for large stroke counts
2. **Dynamic Canvas Sizing**: Pass actual canvas dimensions to recognition service
3. **Unit Tests**: Add comprehensive tests for services and ViewModels
4. **UI Enhancements**: 
   - Undo/Redo functionality
   - Zoom and pan support
   - Export to PDF/Image
5. **Cloud Sync**: Optional cloud backup for notebooks
6. **Multi-language Support**: Expand Resources.resw for localization

## Notes

- The Qwen-VL API requires a valid API key from Alibaba Cloud DashScope
- OpenCvSharp4.Windows includes native binaries (~100MB)
- Stroke data is stored as MessagePack binary blobs in SQLite
