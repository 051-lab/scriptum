using Scriptum.Models;

namespace Scriptum.ViewModels;

public sealed partial class NotebookListItemViewModel : ViewModelBase
{
    public NotebookListItemViewModel(Notebook notebook)
    {
        Id = notebook.Id;
        UpdateFrom(notebook, 0);
    }

    public Guid Id { get; }

    public Notebook Notebook { get; private set; } = new();

    public string Title { get; private set; } = "Notebook";

    public string Description { get; private set; } = "Notebook archive";

    public int PageCount { get; private set; }

    public string Metadata => PageCount == 1
        ? "1 imported page"
        : $"{PageCount} imported pages";

    public void UpdateFrom(Notebook notebook, int pageCount)
    {
        Notebook = notebook;
        Title = notebook.Title;
        Description = string.IsNullOrWhiteSpace(notebook.Description)
            ? "Notebook archive"
            : notebook.Description;
        PageCount = pageCount;

        OnPropertyChanged(nameof(Notebook));
        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(Description));
        OnPropertyChanged(nameof(PageCount));
        OnPropertyChanged(nameof(Metadata));
    }
}
