using Scriptum.Data;
using Scriptum.Models;
using Scriptum.Repositories;

namespace Scriptum.Services;

/// <summary>
/// Implementation of data service for notebook and note operations.
/// </summary>
public class DataService : IDataService
{
    private readonly AppDbContext _context;
    private readonly INotebookRepository _notebookRepository;
    private readonly INoteRepository _noteRepository;
    private bool _initialized;

    public DataService(string databasePath)
    {
        _context = new AppDbContext(databasePath);
        _notebookRepository = new NotebookRepository(_context);
        _noteRepository = new NoteRepository(_context);
    }

    public async Task InitializeAsync()
    {
        if (_initialized) return;

        await _context.Database.EnsureCreatedAsync();
        _initialized = true;
    }

    public async Task<IEnumerable<Notebook>> GetNotebooksAsync()
    {
        return await _notebookRepository.GetActiveNotebooksAsync();
    }

    public async Task<Notebook> CreateNotebookAsync(string name, string? description = null)
    {
        var notebook = new Notebook
        {
            Name = name,
            Description = description
        };
        return await _notebookRepository.AddAsync(notebook);
    }

    public async Task DeleteNotebookAsync(int id)
    {
        var notebook = await _notebookRepository.GetByIdAsync(id);
        if (notebook != null)
        {
            notebook.IsDeleted = true;
            await _notebookRepository.UpdateAsync(notebook);
        }
    }

    public async Task<IEnumerable<Note>> GetNotesAsync(int notebookId)
    {
        return await _noteRepository.GetNotesByNotebookIdAsync(notebookId);
    }

    public async Task<Note> SaveNoteAsync(Note note)
    {
        if (note.Id == 0)
        {
            return await _noteRepository.AddAsync(note);
        }
        else
        {
            await _noteRepository.UpdateAsync(note);
            return note;
        }
    }

    public async Task DeleteNoteAsync(int id)
    {
        var note = await _noteRepository.GetByIdAsync(id);
        if (note != null)
        {
            note.IsDeleted = true;
            await _noteRepository.UpdateAsync(note);
        }
    }
}
