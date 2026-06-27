using Microsoft.EntityFrameworkCore;
using Scriptum.Models;

namespace Scriptum.Data;

/// <summary>
/// Entity Framework DbContext for Scriptum database operations.
/// </summary>
public class AppDbContext : DbContext
{
    private readonly string _databasePath;

    public AppDbContext(string databasePath)
    {
        _databasePath = databasePath;
    }

    public DbSet<Notebook> Notebooks => Set<Notebook>();
    public DbSet<Note> Notes => Set<Note>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connectionString = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder
        {
            DataSource = _databasePath,
            Mode = Microsoft.Data.Sqlite.SqliteOpenMode.ReadWriteCreate
        }.ToString();

        optionsBuilder.UseSqlite(connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Notebook entity
        modelBuilder.Entity<Notebook>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Description).HasMaxLength(1024);
            entity.HasIndex(e => e.IsDeleted);
        });

        // Configure Note entity
        modelBuilder.Entity<Note>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(512);
            entity.Property(e => e.Content).HasMaxLength(4096);
            entity.HasOne(e => e.Notebook)
                  .WithMany(n => n.Notes)
                  .HasForeignKey(e => e.NotebookId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.NotebookId);
            entity.HasIndex(e => e.IsDeleted);
        });
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<Notebook>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.ModifiedAt = now;
            }
        }

        foreach (var entry in ChangeTracker.Entries<Note>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.ModifiedAt = now;
            }
        }
    }
}
