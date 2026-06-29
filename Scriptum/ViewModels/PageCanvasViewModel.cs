using Scriptum.Models;
using Scriptum.Services;

namespace Scriptum.ViewModels;

/// <summary>
/// ViewModel backing the first Scriptum drawing surface MVP.
/// </summary>
public sealed partial class PageCanvasViewModel : ViewModelBase
{
    private readonly IPageStorageService _storageService;

    public PageCanvasViewModel()
        : this(new JsonPageStorageService())
    {
    }

    public PageCanvasViewModel(IPageStorageService storageService)
    {
        _storageService = storageService;
        CurrentPage = new NotebookPage
        {
            Title = "First Scriptum Page"
        };
        StatusMessage = "Draw on the page to create editable vector ink.";
    }

    public NotebookPage CurrentPage { get; private set; }

    public string StatusMessage { get; private set; }

    public int StrokeCount => CurrentPage.Strokes.Count;

    public bool CanUndo => CurrentPage.Strokes.Count > 0;

    public void AddStroke(InkStroke stroke)
    {
        if (stroke.Points.Count < 2)
        {
            return;
        }

        CurrentPage.Strokes.Add(stroke);
        CurrentPage.UpdatedAt = DateTimeOffset.UtcNow;
        StatusMessage = $"Stroke captured. Total strokes: {StrokeCount}.";
        NotifyPageStateChanged();
    }

    public InkStroke? RemoveLastStroke()
    {
        if (!CanUndo)
        {
            StatusMessage = "There are no strokes to undo.";
            OnPropertyChanged(nameof(StatusMessage));
            return null;
        }

        var lastIndex = CurrentPage.Strokes.Count - 1;
        var stroke = CurrentPage.Strokes[lastIndex];
        CurrentPage.Strokes.RemoveAt(lastIndex);
        CurrentPage.UpdatedAt = DateTimeOffset.UtcNow;
        StatusMessage = $"Removed the latest stroke. Total strokes: {StrokeCount}.";
        NotifyPageStateChanged();
        return stroke;
    }

    public void ClearPage()
    {
        CurrentPage.Strokes.Clear();
        CurrentPage.UpdatedAt = DateTimeOffset.UtcNow;
        StatusMessage = "Page cleared.";
        NotifyPageStateChanged();
    }

    public async Task SaveAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            await _storageService.SavePageAsync(CurrentPage);
            StatusMessage = $"Saved {StrokeCount} stroke(s) locally.";
            OnPropertyChanged(nameof(StatusMessage));
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Unable to save page: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task<NotebookPage?> LoadLatestAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var page = await _storageService.LoadLatestPageAsync();
            if (page is null)
            {
                StatusMessage = "No saved page was found.";
                OnPropertyChanged(nameof(StatusMessage));
                return null;
            }

            CurrentPage = page;
            StatusMessage = $"Loaded page with {StrokeCount} stroke(s).";
            NotifyPageStateChanged();
            OnPropertyChanged(nameof(CurrentPage));
            return page;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Unable to load page: {ex.Message}";
            return null;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void NotifyPageStateChanged()
    {
        OnPropertyChanged(nameof(StatusMessage));
        OnPropertyChanged(nameof(StrokeCount));
        OnPropertyChanged(nameof(CanUndo));
    }
}
