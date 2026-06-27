using System.ComponentModel.DataAnnotations;

namespace Scriptum.Models;

/// <summary>
/// Represents a single note within a notebook.
/// </summary>
public class Note
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(512)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(4096)]
    public string? Content { get; set; }

    public int NotebookId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; }

    // Stroke data stored as MessagePack serialized bytes
    public byte[]? StrokeData { get; set; }

    // Navigation property
    public virtual Notebook? Notebook { get; set; }
}
