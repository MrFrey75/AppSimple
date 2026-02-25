using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AppSimple.MvvmApp.Views;

/// <summary>Admin user-management page.</summary>
public partial class UsersView : UserControl
{
    /// <summary>Initializes a new instance of <see cref="UsersView"/>.</summary>
    public UsersView()
    {
        InitializeComponent();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
