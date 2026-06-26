using Microsoft.UI.Xaml;
using Scriptum.ViewModels;

namespace Scriptum.Views;

/// <summary>
/// Code-behind for MainView.
/// </summary>
public sealed partial class MainView : Page
{
    public MainViewModel ViewModel { get; }

    public MainView()
    {
        ViewModel = new MainViewModel();
        InitializeComponent();
        DataContext = ViewModel;
    }
}
