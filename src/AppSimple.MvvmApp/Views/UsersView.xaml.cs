using System.Windows;
using System.Windows.Controls;
using AppSimple.MvvmApp.ViewModels;

namespace AppSimple.MvvmApp.Views;

/// <summary>
/// Code-behind for the User Management admin view. Handles the Save button click
/// to supply the PasswordBox value to the ViewModel.
/// </summary>
public partial class UsersView : UserControl
{
    /// <summary>Initializes a new instance of <see cref="UsersView"/>.</summary>
    public UsersView()
    {
        InitializeComponent();
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not UsersViewModel vm) return;

        string password = FormPasswordBox.Password;
        await vm.SaveFormAsync(password);

        FormPasswordBox.Password = string.Empty;
    }
}
