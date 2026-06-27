using Scriptum.Models.DTOs;

namespace Scriptum.Services;

/// <summary>
/// Implementation of undo/redo functionality for stroke data.
/// Uses a command pattern with state snapshots.
/// </summary>
public class UndoRedoService : IUndoRedoService
{
    private readonly Stack<List<StrokeData>> _undoStack = new();
    private readonly Stack<List<StrokeData>> _redoStack = new();
    private readonly int _maxHistorySize;

    public bool CanUndo => _undoStack.Count > 1; // Need at least 2 states to undo
    public bool CanRedo => _redoStack.Count > 0;
    public int UndoCount => _undoStack.Count;
    public int RedoCount => _redoStack.Count;

    public event Action? StateChanged;

    public UndoRedoService(int maxHistorySize = 50)
    {
        _maxHistorySize = maxHistorySize;
    }

    public void AddState(List<StrokeData> strokes)
    {
        // Create a deep copy of the strokes
        var stateCopy = CloneStrokes(strokes);
        
        _undoStack.Push(stateCopy);
        
        // Clear redo stack when new action is performed
        _redoStack.Clear();
        
        // Limit history size
        while (_undoStack.Count > _maxHistorySize)
        {
            // Remove oldest state (bottom of stack)
            var temp = _undoStack.ToArray();
            _undoStack.Clear();
            for (int i = 0; i < temp.Length - 1; i++)
            {
                _undoStack.Push(temp[i]);
            }
        }

        StateChanged?.Invoke();
    }

    public void Undo()
    {
        if (!CanUndo) return;

        // Move current state to redo stack
        var currentState = _undoStack.Pop();
        _redoStack.Push(currentState);

        // Get previous state
        var previousState = _undoStack.Peek();
        
        // Notify listeners (they should restore the state)
        StateChanged?.Invoke();
    }

    public void Redo()
    {
        if (!CanRedo) return;

        // Move state from redo to undo stack
        var nextState = _redoStack.Pop();
        _undoStack.Push(nextState);

        StateChanged?.Invoke();
    }

    public void Clear()
    {
        _undoStack.Clear();
        _redoStack.Clear();
        StateChanged?.Invoke();
    }

    /// <summary>
    /// Gets the current state (top of undo stack).
    /// </summary>
    public List<StrokeData>? GetCurrentState()
    {
        return _undoStack.Count > 0 ? _undoStack.Peek() : null;
    }

    /// <summary>
    /// Creates a deep copy of stroke data list.
    /// </summary>
    private static List<StrokeData> CloneStrokes(List<StrokeData> strokes)
    {
        var cloned = new List<StrokeData>(strokes.Count);
        
        foreach (var stroke in strokes)
        {
            var clonedPoints = stroke.Points.Select(p => new StrokePoint
            {
                X = p.X,
                Y = p.Y,
                Pressure = p.Pressure,
                Timestamp = p.Timestamp
            }).ToList();

            cloned.Add(new StrokeData
            {
                Points = clonedPoints,
                Color = stroke.Color,
                Thickness = stroke.Thickness,
                IsHighlighter = stroke.IsHighlighter
            });
        }

        return cloned;
    }
}
