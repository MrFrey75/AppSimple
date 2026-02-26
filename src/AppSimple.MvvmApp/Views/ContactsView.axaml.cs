using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AppSimple.MvvmApp.Views;

/// <summary>Contacts management page.</summary>
public partial class ContactsView : UserControl
{
    /// <summary>Initializes a new instance of <see cref="ContactsView"/>.</summary>
    public ContactsView()
    {
        InitializeComponent();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
