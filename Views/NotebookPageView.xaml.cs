using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Scriptum.ViewModels;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.System;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace Scriptum.Views;

/// <summary>
/// Import-first surface for physical notebook pages.
/// </summary>
public sealed partial class NotebookPageView : UserControl
{
    public event EventHandler? PageLibraryChanged;

    public event EventHandler? NewPageRequested;

    public event EventHandler? ImportPageRequested;

    public event EventHandler? LoadLatestPageRequested;

    public NotebookPageViewModel ViewModel { get; private set; } = new();

    public NotebookPageView()
    {
        InitializeComponent();
        DataContext = ViewModel;
        UpdatePageStateOverlays();
    }

    public void SetViewModel(NotebookPageViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = ViewModel;
        Bindings.Update();
        UpdatePageStateOverlays();
    }

    public void RefreshView()
    {
        Bindings.Update();
        UpdatePageStateOverlays();
    }

    public async Task ImportPageAsync()
    {
        await PickAndImportPageAsync();
    }

    public async Task SavePageAsync()
    {
        await ViewModel.SaveTextEditsAsync();
        PageLibraryChanged?.Invoke(this, EventArgs.Empty);
        Bindings.Update();
        UpdatePageStateOverlays();
    }

    public async Task LoadLatestPageAsync()
    {
        await ViewModel.LoadLatestAsync();
        PageLibraryChanged?.Invoke(this, EventArgs.Empty);
        UpdatePageStateOverlays();
    }

    public async Task TranscribePageAsync()
    {
        await RunTranscriptionAsync();
    }

    private void NewPageButton_Click(object sender, RoutedEventArgs e)
    {
        if (NewPageRequested is not null)
        {
            NewPageRequested.Invoke(this, EventArgs.Empty);
            return;
        }

        ResetSurfaceOnly();
    }

    private void ResetSurfaceOnly()
    {
        ViewModel.ResetPage();
        Bindings.Update();
        UpdatePageStateOverlays();
    }

    private async void ImportImageButton_Click(object sender, RoutedEventArgs e)
    {
        if (ImportPageRequested is not null)
        {
            ImportPageRequested.Invoke(this, EventArgs.Empty);
            return;
        }

        await PickAndImportPageAsync();
    }

    private async Task PickAndImportPageAsync()
    {
        var picker = new FileOpenPicker
        {
            SuggestedStartLocation = PickerLocationId.PicturesLibrary
        };

        picker.FileTypeFilter.Add(".jpg");
        picker.FileTypeFilter.Add(".jpeg");
        picker.FileTypeFilter.Add(".png");
        picker.FileTypeFilter.Add(".bmp");
        picker.FileTypeFilter.Add(".gif");
        picker.FileTypeFilter.Add(".tif");
        picker.FileTypeFilter.Add(".tiff");

        if (MainWindow.Active is not null)
        {
            InitializeWithWindow.Initialize(picker, WindowNative.GetWindowHandle(MainWindow.Active));
        }

        var file = await picker.PickSingleFileAsync();
        if (file is null)
        {
            return;
        }

        await ViewModel.ImportImageAsync(file.Path);
        if (ViewModel.HasImportedImage)
        {
            await ViewModel.SaveAsync();
            PageLibraryChanged?.Invoke(this, EventArgs.Empty);
        }

        UpdatePageStateOverlays();
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        await SavePageAsync();
    }

    private async void SaveCorrectionButton_Click(object sender, RoutedEventArgs e)
    {
        await SavePageAsync();
    }

    private void CopyTextButton_Click(object sender, RoutedEventArgs e)
    {
        var correctedText = ViewModel.GetCorrectedTextForCopy();
        if (correctedText is null)
        {
            ViewModel.MarkCorrectedTextCopyUnavailable();
            Bindings.Update();
            return;
        }

        var package = new DataPackage();
        package.SetText(correctedText);
        Clipboard.SetContent(package);
        ViewModel.MarkCorrectedTextCopied();
        Bindings.Update();
    }

    private async void ClearRawButton_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.ClearRawTranscriptionAsync();
        PageLibraryChanged?.Invoke(this, EventArgs.Empty);
        Bindings.Update();
    }

    private async void ClearCorrectedButton_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.ClearCorrectedTranscriptionAsync();
        PageLibraryChanged?.Invoke(this, EventArgs.Empty);
        Bindings.Update();
    }

    private async void ExportMarkdownButton_Click(object sender, RoutedEventArgs e)
    {
        var markdown = ViewModel.BuildMarkdownExport();
        if (markdown is null)
        {
            Bindings.Update();
            return;
        }

        var picker = new FileSavePicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            SuggestedFileName = ViewModel.MarkdownExportFileName
        };
        picker.FileTypeChoices.Add("Markdown", [".md"]);

        if (MainWindow.Active is not null)
        {
            InitializeWithWindow.Initialize(picker, WindowNative.GetWindowHandle(MainWindow.Active));
        }

        var file = await picker.PickSaveFileAsync();
        if (file is null)
        {
            return;
        }

        await FileIO.WriteTextAsync(file, markdown);
        ViewModel.MarkMarkdownExported(file.Path);
        Bindings.Update();
    }

    private async void LoadLatestButton_Click(object sender, RoutedEventArgs e)
    {
        if (LoadLatestPageRequested is not null)
        {
            LoadLatestPageRequested.Invoke(this, EventArgs.Empty);
            return;
        }

        await LoadLatestPageAsync();
    }

    private async void PrepareTranscriptionButton_Click(object sender, RoutedEventArgs e)
    {
        await TranscribePageAsync();
    }

    private async void TranscribeButton_Click(object sender, RoutedEventArgs e)
    {
        await TranscribePageAsync();
    }

    private void FitPageButton_Click(object sender, RoutedEventArgs e)
    {
        PageScrollViewer.ChangeView(null, null, 1.0f);
    }

    private void ZoomPageButton_Click(object sender, RoutedEventArgs e)
    {
        var nextZoom = Math.Min(PageScrollViewer.ZoomFactor + 0.25f, PageScrollViewer.MaxZoomFactor);
        PageScrollViewer.ChangeView(null, null, nextZoom);
    }

    private async void RotateLeftButton_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.RotatePageAsync(-90);
        PageLibraryChanged?.Invoke(this, EventArgs.Empty);
        Bindings.Update();
    }

    private async void RotateRightButton_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.RotatePageAsync(90);
        PageLibraryChanged?.Invoke(this, EventArgs.Empty);
        Bindings.Update();
    }

    private async void OpenOriginalButton_Click(object sender, RoutedEventArgs e)
    {
        if (!ViewModel.HasImportedImage || string.IsNullOrWhiteSpace(ViewModel.CurrentPage.SourceImagePath))
        {
            return;
        }

        try
        {
            var file = await Windows.Storage.StorageFile.GetFileFromPathAsync(ViewModel.CurrentPage.SourceImagePath);
            await Launcher.LaunchFileAsync(file);
        }
        catch (FileNotFoundException)
        {
            await ShowMessageAsync("The original imported image file could not be found.");
        }
        catch (UnauthorizedAccessException)
        {
            await ShowMessageAsync("Scriptum does not have permission to open the original image file.");
        }
    }

    private async Task RunTranscriptionAsync()
    {
        await ViewModel.TranscribeAsync();
        PageLibraryChanged?.Invoke(this, EventArgs.Empty);
        Bindings.Update();
        UpdatePageStateOverlays();
    }

    private void UpdatePageStateOverlays()
    {
        EmptyState.Visibility = ViewModel.HasImportedImage ? Visibility.Collapsed : Visibility.Visible;
        MissingImageState.Visibility = ViewModel.IsImportedImageMissing ? Visibility.Visible : Visibility.Collapsed;
    }

    private async Task ShowMessageAsync(string message)
    {
        var dialog = new ContentDialog
        {
            Title = "Notebook page",
            Content = message,
            CloseButtonText = "OK",
            XamlRoot = XamlRoot
        };

        await dialog.ShowAsync();
    }
}
