using System.Windows;
using AppSimple.MvvmApp.ViewModels;

namespace AppSimple.MvvmApp;

/// <summary>
/// Code-behind for the application shell window. The DataContext is set to
/// <see cref="MainWindowViewModel"/> by the DI container in <see cref="App"/>.
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>Initializes a new instance of <see cref="MainWindow"/>.</summary>
    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
