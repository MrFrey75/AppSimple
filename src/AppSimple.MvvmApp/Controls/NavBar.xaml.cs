using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AppSimple.MvvmApp.ViewModels;

namespace AppSimple.MvvmApp.Controls;

/// <summary>
/// Code-behind for the NavBar UserControl. Handles PasswordBox interactions
/// that cannot be expressed via standard MVVM bindings.
/// </summary>
public partial class NavBar : UserControl
{
    /// <summary>Initializes a new instance of <see cref="NavBar"/>.</summary>
    public NavBar()
    {
        InitializeComponent();
    }

    /// <summary>Pressing Enter in the username box moves focus to the password box.</summary>
    private void Username_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key is Key.Enter or Key.Tab)
        {
            PwdBox.Focus();
            e.Handled = true;
        }
    }

    /// <summary>Pressing Enter in the password box triggers login.</summary>
    private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            TriggerLogin();
    }

    /// <summary>Login button click handler.</summary>
    private void LoginButton_Click(object sender, RoutedEventArgs e) => TriggerLogin();

    private void TriggerLogin()
    {
        if (DataContext is not MainWindowViewModel vm) return;
        string password = PwdBox.Password;
        PwdBox.Password = string.Empty;
        _ = vm.LoginAsync(password);
    }
}
