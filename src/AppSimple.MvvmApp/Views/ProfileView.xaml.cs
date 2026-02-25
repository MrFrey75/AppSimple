using System.Windows;
using System.Windows.Controls;
using AppSimple.MvvmApp.ViewModels;

namespace AppSimple.MvvmApp.Views;

/// <summary>
/// Code-behind for the Profile page. Handles PasswordBox values that cannot
/// be bound via MVVM for security reasons.
/// </summary>
public partial class ProfileView : UserControl
{
    /// <summary>Initializes a new instance of <see cref="ProfileView"/>.</summary>
    public ProfileView()
    {
        InitializeComponent();
    }

    private async void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not ProfileViewModel vm) return;

        await vm.ChangePasswordAsync(
            CurrentPwd.Password,
            NewPwd.Password,
            ConfirmPwd.Password);

        CurrentPwd.Password = string.Empty;
        NewPwd.Password     = string.Empty;
        ConfirmPwd.Password = string.Empty;
    }
}
