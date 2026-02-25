using AppSimple.Core.Common.Exceptions;
using AppSimple.Core.Services;
using AppSimple.MvvmApp.Session;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AppSimple.MvvmApp.ViewModels;

/// <summary>
/// ViewModel for the Profile page. Allows the logged-in user to view and edit
/// their own profile information and change their password.
/// </summary>
public partial class ProfileViewModel : BaseViewModel
{
    private readonly IUserService _users;
    private readonly UserSession  _session;

    // ─── Display-only ──────────────────────────────────────────────────────

    /// <summary>Gets the current user's username (read-only).</summary>
    public string Username    => _session.CurrentUser?.Username                     ?? string.Empty;

    /// <summary>Gets the current user's email address (read-only).</summary>
    public string Email       => _session.CurrentUser?.Email                        ?? string.Empty;

    /// <summary>Gets the current user's role label (read-only).</summary>
    public string Role        => _session.CurrentUser?.Role.ToString()              ?? string.Empty;

    /// <summary>Gets the date the user account was created (read-only).</summary>
    public string MemberSince => _session.CurrentUser?.CreatedAt.ToString("yyyy-MM-dd") ?? string.Empty;

    // ─── Editable fields ───────────────────────────────────────────────────

    /// <summary>Gets or sets the user's first name.</summary>
    [ObservableProperty] private string _firstName   = string.Empty;

    /// <summary>Gets or sets the user's last name.</summary>
    [ObservableProperty] private string _lastName    = string.Empty;

    /// <summary>Gets or sets the user's phone number.</summary>
    [ObservableProperty] private string _phoneNumber = string.Empty;

    /// <summary>Gets or sets the user's biography text.</summary>
    [ObservableProperty] private string _bio         = string.Empty;

    /// <summary>Gets or sets the date of birth as an ISO-8601 string (yyyy-MM-dd).</summary>
    [ObservableProperty] private string _dobText     = string.Empty;

    /// <summary>Gets or sets a value indicating whether the password change operation is in progress.</summary>
    [ObservableProperty] private bool _isPasswordBusy;

    /// <summary>Initializes a new instance of <see cref="ProfileViewModel"/>.</summary>
    public ProfileViewModel(IUserService users, UserSession session)
    {
        _users   = users;
        _session = session;
    }

    /// <summary>
    /// Loads the current user's data from the database and populates the form fields.
    /// Called by <see cref="MainWindowViewModel"/> when navigating to this page.
    /// </summary>
    public async Task LoadAsync()
    {
        if (_session.CurrentUser is null) return;

        ClearMessages();
        var user = await _users.GetByUidAsync(_session.CurrentUser.Uid);
        if (user is null) return;

        _session.Login(user, _session.Token!);

        FirstName   = user.FirstName   ?? string.Empty;
        LastName    = user.LastName    ?? string.Empty;
        PhoneNumber = user.PhoneNumber ?? string.Empty;
        Bio         = user.Bio         ?? string.Empty;
        DobText     = user.DateOfBirth?.ToString("yyyy-MM-dd") ?? string.Empty;

        OnPropertyChanged(nameof(Username));
        OnPropertyChanged(nameof(Email));
        OnPropertyChanged(nameof(Role));
        OnPropertyChanged(nameof(MemberSince));
    }

    /// <summary>Saves the editable profile fields to the database.</summary>
    [RelayCommand]
    private async Task SaveProfile()
    {
        if (_session.CurrentUser is null) return;

        IsBusy = true;
        ClearMessages();
        try
        {
            var user = _session.CurrentUser;
            user.FirstName   = string.IsNullOrWhiteSpace(FirstName)   ? null : FirstName;
            user.LastName    = string.IsNullOrWhiteSpace(LastName)    ? null : LastName;
            user.PhoneNumber = string.IsNullOrWhiteSpace(PhoneNumber) ? null : PhoneNumber;
            user.Bio         = string.IsNullOrWhiteSpace(Bio)         ? null : Bio;

            if (!string.IsNullOrWhiteSpace(DobText) && DateTime.TryParse(DobText, out DateTime dob))
                user.DateOfBirth = dob;
            else if (string.IsNullOrWhiteSpace(DobText))
                user.DateOfBirth = null;

            await _users.UpdateAsync(user);
            _session.Login(user, _session.Token!);
            SetSuccess("Profile saved successfully.");
        }
        catch (Exception ex)
        {
            SetError($"Failed to save: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Changes the user's password. Called from code-behind to supply PasswordBox values.
    /// </summary>
    /// <param name="current">The current plain-text password.</param>
    /// <param name="newPwd">The new plain-text password.</param>
    /// <param name="confirm">Confirmation of the new password; must match <paramref name="newPwd"/>.</param>
    public async Task ChangePasswordAsync(string current, string newPwd, string confirm)
    {
        if (_session.CurrentUser is null) return;

        if (newPwd != confirm)
        {
            SetError("New password and confirmation do not match.");
            return;
        }

        IsPasswordBusy = true;
        ClearMessages();
        try
        {
            await _users.ChangePasswordAsync(_session.CurrentUser.Uid, current, newPwd);
            SetSuccess("Password changed successfully.");
        }
        catch (UnauthorizedException)
        {
            SetError("Current password is incorrect.");
        }
        catch (Exception ex)
        {
            SetError($"Failed to change password: {ex.Message}");
        }
        finally
        {
            IsPasswordBusy = false;
        }
    }
}
