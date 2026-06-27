using System.ComponentModel.DataAnnotations;

namespace Scriptum.Models;

/// <summary>
/// Represents a notebook container for notes.
/// </summary>
public class Notebook
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(256)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1024)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; }

    // Navigation property
    public virtual ICollection<Note>? Notes { get; set; }
}
