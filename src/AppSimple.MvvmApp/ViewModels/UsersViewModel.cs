using System.Collections.ObjectModel;
using AppSimple.Core.Common.Exceptions;
using AppSimple.Core.Enums;
using AppSimple.Core.Models;
using AppSimple.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AppSimple.MvvmApp.ViewModels;

/// <summary>Indicates what the right-panel form is currently doing.</summary>
public enum FormMode { None, Create, Edit }

/// <summary>
/// ViewModel for the admin User Management page. Provides a user list with
/// inline create/edit form panel.
/// </summary>
public partial class UsersViewModel : BaseViewModel
{
    private readonly IUserService _users;

    /// <summary>Gets the live collection of all users displayed in the DataGrid.</summary>
    public ObservableCollection<User> Users { get; } = new();

    /// <summary>Gets or sets the user currently selected in the DataGrid.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedUser))]
    private User? _selectedUser;

    /// <summary>Gets or sets the current form mode (None / Create / Edit).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFormVisible))]
    [NotifyPropertyChangedFor(nameof(IsCreateMode))]
    [NotifyPropertyChangedFor(nameof(IsEditMode))]
    [NotifyPropertyChangedFor(nameof(FormTitle))]
    [NotifyPropertyChangedFor(nameof(ShowPasswordField))]
    private FormMode _formMode = FormMode.None;

    // ─── Form fields ───────────────────────────────────────────────────────

    /// <summary>Gets or sets the username field in the create/edit form.</summary>
    [ObservableProperty] private string   _formUsername  = string.Empty;

    /// <summary>Gets or sets the email field in the create/edit form.</summary>
    [ObservableProperty] private string   _formEmail     = string.Empty;

    /// <summary>Gets or sets the first name field in the create/edit form.</summary>
    [ObservableProperty] private string   _formFirstName = string.Empty;

    /// <summary>Gets or sets the last name field in the create/edit form.</summary>
    [ObservableProperty] private string   _formLastName  = string.Empty;

    /// <summary>Gets or sets the role selection in the create/edit form.</summary>
    [ObservableProperty] private UserRole _formRole      = UserRole.User;

    /// <summary>Gets or sets the role label string bound to the ComboBox.</summary>
    public string FormRoleLabel
    {
        get => FormRole == UserRole.Admin ? "Admin" : "User";
        set => FormRole = value == "Admin" ? UserRole.Admin : UserRole.User;
    }

    /// <summary>Gets or sets whether the user is active in the create/edit form.</summary>
    [ObservableProperty] private bool     _formIsActive  = true;

    /// <summary>Gets or sets the plain-text password field for Create mode in the form.</summary>
    [ObservableProperty] private string _formPassword = string.Empty;

    // ─── Computed ──────────────────────────────────────────────────────────

    /// <summary>Gets a value indicating whether a user is selected in the DataGrid.</summary>
    public bool HasSelectedUser  => SelectedUser is not null;

    /// <summary>Gets a value indicating whether the form panel is visible.</summary>
    public bool IsFormVisible    => FormMode != FormMode.None;

    /// <summary>Gets a value indicating whether the form is in Create mode.</summary>
    public bool IsCreateMode     => FormMode == FormMode.Create;

    /// <summary>Gets a value indicating whether the form is in Edit mode.</summary>
    public bool IsEditMode       => FormMode == FormMode.Edit;

    /// <summary>Gets a value indicating whether the password field should be shown (Create only).</summary>
    public bool ShowPasswordField => FormMode == FormMode.Create;

    /// <summary>Gets the title label for the right-panel form.</summary>
    public string FormTitle => FormMode switch
    {
        FormMode.Create => "Add New User",
        FormMode.Edit   => $"Editing  {SelectedUser?.Username}",
        _               => string.Empty
    };

    /// <summary>Initializes a new instance of <see cref="UsersViewModel"/>.</summary>
    public UsersViewModel(IUserService users)
    {
        _users = users;
    }

    /// <summary>Loads all users from the database into <see cref="Users"/>.</summary>
    public async Task LoadAsync()
    {
        IsBusy = true;
        ClearMessages();
        try
        {
            var all = await _users.GetAllAsync();
            Users.Clear();
            foreach (var u in all) Users.Add(u);
        }
        catch (Exception ex)
        {
            SetError($"Failed to load users: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    // ─── Commands ──────────────────────────────────────────────────────────

    /// <summary>Opens the right panel in Create mode with blank fields.</summary>
    [RelayCommand]
    private void ShowCreateForm()
    {
        FormUsername  = string.Empty;
        FormEmail     = string.Empty;
        FormFirstName = string.Empty;
        FormLastName  = string.Empty;
        FormRole      = UserRole.User;
        FormIsActive  = true;
        SelectedUser  = null;
        FormMode      = FormMode.Create;
        ClearMessages();
    }

    /// <summary>Populates the right panel with the selected user's data in Edit mode.</summary>
    [RelayCommand(CanExecute = nameof(HasSelectedUser))]
    private void EditSelectedUser()
    {
        if (SelectedUser is null) return;
        FormUsername  = SelectedUser.Username;
        FormEmail     = SelectedUser.Email;
        FormFirstName = SelectedUser.FirstName ?? string.Empty;
        FormLastName  = SelectedUser.LastName  ?? string.Empty;
        FormRole      = SelectedUser.Role;
        FormIsActive  = SelectedUser.IsActive;
        FormMode      = FormMode.Edit;
        ClearMessages();
    }

    /// <summary>Closes the form panel without saving.</summary>
    [RelayCommand]
    private void CancelForm()
    {
        FormMode = FormMode.None;
        ClearMessages();
    }

    /// <summary>Deletes the selected user after confirmation in the UI.</summary>
    [RelayCommand(CanExecute = nameof(HasSelectedUser))]
    private async Task DeleteSelectedUser()
    {
        if (SelectedUser is null) return;
        IsBusy = true;
        ClearMessages();
        try
        {
            await _users.DeleteAsync(SelectedUser.Uid);
            Users.Remove(SelectedUser);
            SelectedUser = null;
            FormMode     = FormMode.None;
            SetSuccess("User deleted successfully.");
        }
        catch (SystemEntityException)
        {
            SetError("Cannot delete a system user.");
        }
        catch (Exception ex)
        {
            SetError($"Delete failed: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Saves the form using <see cref="FormPassword"/> for Create mode.
    /// </summary>
    [RelayCommand]
    public async Task SaveForm()
    {
        IsBusy = true;
        ClearMessages();
        try
        {
            if (FormMode == FormMode.Create)
                await CreateUserInternalAsync(FormPassword);
            else if (FormMode == FormMode.Edit && SelectedUser is not null)
                await UpdateUserInternalAsync();

            FormPassword = string.Empty;
            await LoadAsync();
            FormMode = FormMode.None;
        }
        catch (DuplicateEntityException ex)
        {
            SetError($"Conflict: {ex.Message}");
        }
        catch (SystemEntityException)
        {
            SetError("Cannot modify a system user.");
        }
        catch (Exception ex)
        {
            SetError($"Save failed: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Saves the form. Kept for backward-compatibility with code-behind callers.
    /// </summary>
    public Task SaveFormAsync(string password)
    {
        FormPassword = password;
        return SaveForm();
    }

    // ─── Private helpers ───────────────────────────────────────────────────

    private async Task CreateUserInternalAsync(string password)
    {
        var user = await _users.CreateAsync(FormUsername, FormEmail, password);
        user.FirstName = string.IsNullOrWhiteSpace(FormFirstName) ? null : FormFirstName;
        user.LastName  = string.IsNullOrWhiteSpace(FormLastName)  ? null : FormLastName;
        user.Role      = FormRole;
        user.IsActive  = FormIsActive;
        await _users.UpdateAsync(user);
        SetSuccess($"User '{FormUsername}' created.");
    }

    private async Task UpdateUserInternalAsync()
    {
        var user = SelectedUser!;
        user.FirstName = string.IsNullOrWhiteSpace(FormFirstName) ? null : FormFirstName;
        user.LastName  = string.IsNullOrWhiteSpace(FormLastName)  ? null : FormLastName;
        user.Role      = FormRole;
        user.IsActive  = FormIsActive;
        await _users.UpdateAsync(user);
        SetSuccess($"User '{user.Username}' updated.");
    }
}
