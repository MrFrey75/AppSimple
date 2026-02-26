using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AppSimple.MvvmApp.Views;

/// <summary>Notes management page.</summary>
public partial class NotesView : UserControl
{
    /// <summary>Initializes a new instance of <see cref="NotesView"/>.</summary>
    public NotesView()
    {
        InitializeComponent();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
