using Microsoft.UI.Xaml.Controls;
using Scriptum.ViewModels;

namespace Scriptum.Views;

/// <summary>
/// Code-behind for the main application view.
/// </summary>
public sealed partial class MainView : Page
{
    public MainViewModel ViewModel { get; } = new();

    public MainView()
    {
        InitializeComponent();
        DataContext = ViewModel;
    }
}
