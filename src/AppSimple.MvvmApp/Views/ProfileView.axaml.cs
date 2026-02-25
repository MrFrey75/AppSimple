using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AppSimple.MvvmApp.Views;

/// <summary>Logged-in user profile page.</summary>
public partial class ProfileView : UserControl
{
    /// <summary>Initializes a new instance of <see cref="ProfileView"/>.</summary>
    public ProfileView()
    {
        InitializeComponent();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
