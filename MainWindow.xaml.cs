using Microsoft.UI.Xaml;

namespace Scriptum;

public sealed partial class MainWindow : Window
{
    public static MainWindow? Active { get; private set; }

    public MainWindow()
    {
        Active = this;
        InitializeComponent();
        Content = new Views.MainView();
    }
}
