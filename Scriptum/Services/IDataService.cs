using Scriptum.Models;

namespace Scriptum.Services;

/// <summary>
/// Service interface for data operations.
/// </summary>
public interface IDataService
{
    Task InitializeAsync();
    Task<IEnumerable<Notebook>> GetNotebooksAsync();
    Task<Notebook> CreateNotebookAsync(string name, string? description = null);
    Task DeleteNotebookAsync(int id);
    Task<IEnumerable<Note>> GetNotesAsync(int notebookId);
    Task<Note> SaveNoteAsync(Note note);
    Task DeleteNoteAsync(int id);
}
