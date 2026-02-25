using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using AppSimple.MvvmApp.ViewModels;

namespace AppSimple.MvvmApp.Controls;

/// <summary>Top navigation bar with branding, login form, and user chip.</summary>
public partial class NavBar : UserControl
{
    /// <summary>Initializes a new instance of <see cref="NavBar"/>.</summary>
    public NavBar()
    {
        InitializeComponent();
        ThemeComboBox.ItemsSource = AppSimple.MvvmApp.Services.ThemeManager.ThemeLabels;
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    private void OnUsernameKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter || e.Key == Key.Tab)
            this.FindControl<TextBox>("PasswordBox")?.Focus();
    }

    private void OnPasswordKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && DataContext is MainWindowViewModel vm)
        {
            if (vm.LoginCommand.CanExecute(null))
                vm.LoginCommand.Execute(null);
        }
    }
}
