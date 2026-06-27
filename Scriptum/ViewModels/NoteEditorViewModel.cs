using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Scriptum.Data;
using Scriptum.Models;
using Scriptum.Models.DTOs;
using Scriptum.Services;
using System.IO;
using Windows.Foundation;
using Windows.Storage;

namespace Scriptum.ViewModels;

/// <summary>
/// ViewModel for the ink canvas note editor.
/// </summary>
public partial class NoteEditorViewModel : ViewModelBase
{
    private readonly IDataService? _dataService;
    private readonly IHandwritingRecognitionService? _recognitionService;
    private readonly IImageProcessingService? _imageProcessingService;
    private readonly StrokeToImageRenderer? _strokeRenderer;
    private readonly IUndoRedoService? _undoRedoService;
    private readonly IExportService? _exportService;

    [ObservableProperty]
    private string _noteTitle = string.Empty;

    [ObservableProperty]
    private Notebook? _selectedNotebook;

    [ObservableProperty]
    private float _brushSize = 3f;

    [ObservableProperty]
    private Windows.UI.Color _brushColor = Windows.UI.Colors.Black;

    [ObservableProperty]
    private bool _isDirty;

    [ObservableProperty]
    private bool _isRecognizing;

    [ObservableProperty]
    private string _recognizedText = string.Empty;

    [ObservableProperty]
    private bool _isHighlighterMode;

    [ObservableProperty]
    private bool _canUndo;

    [ObservableProperty]
    private bool _canRedo;

    [ObservableProperty]
    private bool _isExporting;

    public List<PointF> CurrentStroke { get; } = new();
    public List<StrokeData> Strokes { get; } = new();

    public NoteEditorViewModel() : this(null, null, null, null, null)
    {
    }

    public NoteEditorViewModel(IDataService? dataService, 
        IHandwritingRecognitionService? recognitionService,
        IImageProcessingService? imageProcessingService,
        IUndoRedoService? undoRedoService = null,
        IExportService? exportService = null)
    {
        _dataService = dataService;
        _recognitionService = recognitionService;
        _imageProcessingService = imageProcessingService;
        _strokeRenderer = new StrokeToImageRenderer();
        _undoRedoService = undoRedoService ?? new UndoRedoService();
        _exportService = exportService ?? new ExportService();
        
        // Subscribe to undo/redo state changes
        if (_undoRedoService != null)
        {
            _undoRedoService.StateChanged += OnUndoRedoStateChanged;
        }
    }

    private void OnUndoRedoStateChanged()
    {
        CanUndo = _undoRedoService?.CanUndo ?? false;
        CanRedo = _undoRedoService?.CanRedo ?? false;
        
        // Restore strokes from current state
        var currentState = _undoRedoService?.GetCurrentState();
        if (currentState != null)
        {
            Strokes.Clear();
            Strokes.AddRange(currentState);
            IsDirty = true;
        }
    }

    [RelayCommand]
    private void ClearCanvas()
    {
        Strokes.Clear();
        _undoRedoService?.Clear();
        IsDirty = true;
    }

    [RelayCommand]
    private void Undo()
    {
        _undoRedoService?.Undo();
    }

    [RelayCommand]
    private void Redo()
    {
        _undoRedoService?.Redo();
    }

    [RelayCommand]
    private void SetPenTool()
    {
        BrushSize = 3f;
        BrushColor = Windows.UI.Colors.Black;
        IsHighlighterMode = false;
    }

    [RelayCommand]
    private void SetHighlighterTool()
    {
        BrushSize = 20f;
        BrushColor = Windows.UI.Color.FromArgb(128, 255, 255, 0);
        IsHighlighterMode = true;
    }

    [RelayCommand]
    private void SetEraserTool()
    {
        BrushSize = 15f;
        BrushColor = Windows.UI.Colors.White;
        IsHighlighterMode = false;
    }

    public void AddStrokePoint(PointF point)
    {
        CurrentStroke.Add(point);
    }

    public void CompleteStroke()
    {
        if (CurrentStroke.Count >= 2)
        {
            var stroke = new StrokeData
            {
                Points = CurrentStroke.Select(p => new StrokePoint
                {
                    X = p.X,
                    Y = p.Y,
                    Pressure = 0.5f, // Default pressure
                    Timestamp = DateTime.Now
                }).ToList(),
                Color = $"#{BrushColor.R:X2}{BrushColor.G:X2}{BrushColor.B:X2}",
                Thickness = BrushSize,
                IsHighlighter = IsHighlighterMode
            };
            Strokes.Add(stroke);
            
            // Add state to undo/redo service after completing stroke
            _undoRedoService?.AddState(new List<StrokeData>(Strokes));
        }
        CurrentStroke.Clear();
        IsDirty = true;
    }

    public void LoadNote(Note note)
    {
        NoteTitle = note.Title;
        if (note.StrokeData != null && note.StrokeData.Length > 0)
        {
            var loadedStrokes = StrokeSerializer.Deserialize(note.StrokeData);
            Strokes.Clear();
            Strokes.AddRange(loadedStrokes);
        }
    }

    public Note CreateNote(int notebookId)
    {
        return new Note
        {
            Title = NoteTitle,
            NotebookId = notebookId,
            StrokeData = Strokes.Count > 0 ? StrokeSerializer.Serialize(Strokes) : null
        };
    }

    [RelayCommand]
    private async Task SaveNoteAsync()
    {
        if (_dataService == null || SelectedNotebook == null) return;

        var note = CreateNote(SelectedNotebook.Id);
        await _dataService.SaveNoteAsync(note);
        IsDirty = false;
    }

    [RelayCommand]
    private async Task RecognizeHandwritingAsync()
    {
        if (_recognitionService == null || Strokes.Count == 0) return;

        IsRecognizing = true;
        try
        {
            // Use the Qwen-VL service to recognize handwriting from strokes
            var recognizedText = await _recognitionService.RecognizeHandwritingFromStrokesAsync(
                Strokes, 
                800f, // Default canvas width - should be dynamic
                600f  // Default canvas height - should be dynamic
            );
            RecognizedText = recognizedText;
        }
        catch (Exception ex)
        {
            RecognizedText = $"Error: {ex.Message}";
        }
        finally
        {
            IsRecognizing = false;
        }
    }

    [RelayCommand]
    private async Task ExportToPngAsync()
    {
        if (_exportService == null || Strokes.Count == 0) return;

        IsExporting = true;
        try
        {
            // Default canvas dimensions - should be dynamic based on actual canvas size
            var pngBytes = await _exportService.ExportToPngAsync(Strokes, 800f, 600f);
            
            // In a real app, you would save this to a file using FileSavePicker
            // For now, we just log the export success
            System.Diagnostics.Debug.WriteLine($"Exported PNG: {pngBytes.Length} bytes");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Export error: {ex.Message}");
        }
        finally
        {
            IsExporting = false;
        }
    }
}
