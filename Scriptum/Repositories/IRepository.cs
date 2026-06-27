using Scriptum.Models;

namespace Scriptum.Repositories;

/// <summary>
/// Generic repository interface for data access operations.
/// </summary>
public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
}

/// <summary>
/// Repository interface for Notebook-specific operations.
/// </summary>
public interface INotebookRepository : IRepository<Notebook>
{
    Task<IEnumerable<Notebook>> GetActiveNotebooksAsync();
    Task<IEnumerable<Note>> GetNotesByNotebookIdAsync(int notebookId);
}

/// <summary>
/// Repository interface for Note-specific operations.
/// </summary>
public interface INoteRepository : IRepository<Note>
{
    Task<IEnumerable<Note>> GetNotesByNotebookIdAsync(int notebookId);
    Task<Note?> GetLatestNoteAsync();
}
