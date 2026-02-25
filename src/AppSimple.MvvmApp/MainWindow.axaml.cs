using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AppSimple.MvvmApp.ViewModels;

namespace AppSimple.MvvmApp;

/// <summary>The main application window.</summary>
public partial class MainWindow : Window
{
    /// <summary>Parameterless constructor for Avalonia XAML loader (designer use).</summary>
    public MainWindow()
    {
        InitializeComponent();
    }

    /// <summary>Initializes a new instance of <see cref="MainWindow"/> with its ViewModel.</summary>
    public MainWindow(MainWindowViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
