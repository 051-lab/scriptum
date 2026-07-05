namespace Scriptum.Models;

/// <summary>
/// A physical notebook or project container for imported notebook pages.
/// </summary>
public sealed class Notebook
{
    public static readonly Guid DefaultNotebookId = Guid.Parse("7d7f9826-e6fc-4d8c-8c58-f94d7af1c70d");

    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = "Notebook";

    public string? Description { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public static Notebook CreateDefault()
    {
        var timestamp = DateTimeOffset.UtcNow;
        return new Notebook
        {
            Id = DefaultNotebookId,
            Title = "Scriptum",
            Description = "Default notebook archive",
            CreatedAt = timestamp,
            UpdatedAt = timestamp
        };
    }
}
