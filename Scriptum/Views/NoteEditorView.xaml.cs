using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Scriptum.Services;
using Scriptum.ViewModels;

namespace Scriptum.Views;

public sealed partial class NoteEditorView : Page
{
    private readonly NoteEditorViewModel _viewModel;

    public NoteEditorView()
    {
        _viewModel = new NoteEditorViewModel();
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        
        if (e.Parameter is Models.Notebook notebook)
        {
            _viewModel.SelectedNotebook = notebook;
        }
    }
}
