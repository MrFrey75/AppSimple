using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AppSimple.MvvmApp.Views;

/// <summary>Public landing page view.</summary>
public partial class HomeView : UserControl
{
    /// <summary>Initializes a new instance of <see cref="HomeView"/>.</summary>
    public HomeView()
    {
        InitializeComponent();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
