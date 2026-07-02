using Microsoft.UI.Xaml;

namespace Scriptum;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Content = new Views.MainView();
    }
}
