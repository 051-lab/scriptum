using Scriptum.Models.DTOs;

namespace Scriptum.Services;

/// <summary>
/// Service for managing undo/redo operations for stroke data.
/// </summary>
public interface IUndoRedoService
{
    bool CanUndo { get; }
    bool CanRedo { get; }
    int UndoCount { get; }
    int RedoCount { get; }
    
    void AddState(List<StrokeData> strokes);
    void Undo();
    void Redo();
    void Clear();
    event Action? StateChanged;
}
