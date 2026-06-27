using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Scriptum.Data;
using Scriptum.Services;
using Scriptum.ViewModels;

namespace Scriptum.Views;

public sealed partial class MainView : Page
{
    private readonly DataService _dataService;

    public MainView()
    {
        var dbPath = System.IO.Path.Combine(
            Windows.Storage.ApplicationData.Current.LocalFolder.Path,
            "scriptum.db");
        
        _dataService = new DataService(dbPath);
        ViewModel = new MainViewModel(_dataService);
        
        this.InitializeComponent();
        ViewModel.LoadNotebooksCommand.Execute(null);
    }

    public MainViewModel ViewModel { get; }

    private async void NewNotebookButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            Title = new TextBlock { Text = "Create New Notebook" },
            XamlRoot = this.XamlRoot
        };

        var nameBox = new TextBox
        {
            PlaceholderText = "Notebook Name",
            Margin = new Thickness(0, 10, 0, 0)
        };

        var descBox = new TextBox
        {
            PlaceholderText = "Description (optional)",
            Margin = new Thickness(0, 10, 0, 0)
        };

        var stackPanel = new StackPanel();
        stackPanel.Children.Add(nameBox);
        stackPanel.Children.Add(descBox);
        dialog.Content = stackPanel;

        dialog.PrimaryButtonText = "Create";
        dialog.SecondaryButtonText = "Cancel";
        dialog.DefaultButton = ContentDialogButton.Primary;

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary && !string.IsNullOrWhiteSpace(nameBox.Text))
        {
            await ViewModel.CreateNotebookCommand.ExecuteAsync(nameBox.Text);
        }
    }

    private void OpenNoteEditorButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedNotebook != null)
        {
            Frame.Navigate(typeof(NoteEditorView), ViewModel.SelectedNotebook);
        }
    }
}
