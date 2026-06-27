using Microsoft.EntityFrameworkCore;
using Scriptum.Data;
using Scriptum.Models;

namespace Scriptum.Repositories;

/// <summary>
/// Generic repository implementation for data access operations.
/// </summary>
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
    }
}

/// <summary>
/// Notebook repository implementation with specialized queries.
/// </summary>
public class NotebookRepository : Repository<Notebook>, INotebookRepository
{
    public NotebookRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Notebook>> GetActiveNotebooksAsync()
    {
        return await _dbSet
            .Where(n => !n.IsDeleted)
            .OrderByDescending(n => n.ModifiedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Note>> GetNotesByNotebookIdAsync(int notebookId)
    {
        return await _context.Notes
            .Where(n => n.NotebookId == notebookId && !n.IsDeleted)
            .OrderByDescending(n => n.ModifiedAt)
            .ToListAsync();
    }
}

/// <summary>
/// Note repository implementation with specialized queries.
/// </summary>
public class NoteRepository : Repository<Note>, INoteRepository
{
    public NoteRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Note>> GetNotesByNotebookIdAsync(int notebookId)
    {
        return await _dbSet
            .Where(n => n.NotebookId == notebookId && !n.IsDeleted)
            .OrderByDescending(n => n.ModifiedAt)
            .ToListAsync();
    }

    public async Task<Note?> GetLatestNoteAsync()
    {
        return await _dbSet
            .Where(n => !n.IsDeleted)
            .OrderByDescending(n => n.ModifiedAt)
            .FirstOrDefaultAsync();
    }
}
