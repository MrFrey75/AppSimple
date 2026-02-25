using CommunityToolkit.Mvvm.ComponentModel;

namespace AppSimple.MvvmApp.ViewModels;

/// <summary>
/// Base class for all page ViewModels. Provides shared busy, error, and success state.
/// </summary>
public abstract partial class BaseViewModel : ObservableObject
{
    /// <summary>Gets or sets a value indicating whether an async operation is in progress.</summary>
    [ObservableProperty] private bool _isBusy;

    /// <summary>Gets or sets the current error message. Empty string means no error.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string _errorMessage = string.Empty;

    /// <summary>Gets or sets the current success message. Empty string means no message.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSuccess))]
    private string _successMessage = string.Empty;

    /// <summary>Gets a value indicating whether there is an active error message.</summary>
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    /// <summary>Gets a value indicating whether there is an active success message.</summary>
    public bool HasSuccess => !string.IsNullOrEmpty(SuccessMessage);

    /// <summary>Sets an error message and clears any success message.</summary>
    protected void SetError(string message)
    {
        SuccessMessage = string.Empty;
        ErrorMessage   = message;
    }

    /// <summary>Sets a success message and clears any error message.</summary>
    protected void SetSuccess(string message)
    {
        ErrorMessage   = string.Empty;
        SuccessMessage = message;
    }

    /// <summary>Clears both the error and success messages.</summary>
    protected void ClearMessages()
    {
        ErrorMessage   = string.Empty;
        SuccessMessage = string.Empty;
    }
}
