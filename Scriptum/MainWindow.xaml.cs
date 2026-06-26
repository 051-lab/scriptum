using Microsoft.UI.Xaml;

namespace Scriptum;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        // Set the content to MainView for MVVM navigation
        Content = new Views.MainView();
    }
}
