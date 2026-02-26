using System.Collections.ObjectModel;
using AppSimple.Core.Enums;
using AppSimple.Core.Models;
using AppSimple.Core.Services;
using AppSimple.MvvmApp.Session;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AppSimple.MvvmApp.ViewModels;

/// <summary>
/// ViewModel for the Contacts page. Lists the current user's contacts with an inline
/// create/edit panel and per-contact email/phone/address child management.
/// </summary>
public partial class ContactsViewModel : BaseViewModel
{
    private readonly IContactService _contacts;
    private readonly UserSession     _session;

    // ─── Collections ──────────────────────────────────────────────────────

    /// <summary>Gets the live collection of contacts shown in the list.</summary>
    public ObservableCollection<Contact> Contacts { get; } = new();

    // ─── Selection ────────────────────────────────────────────────────────

    /// <summary>Gets or sets the contact currently selected in the list.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedContact))]
    [NotifyPropertyChangedFor(nameof(IsDetailVisible))]
    [NotifyCanExecuteChangedFor(nameof(EditSelectedContactCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteSelectedContactCommand))]
    [NotifyCanExecuteChangedFor(nameof(AddEmailCommand))]
    [NotifyCanExecuteChangedFor(nameof(AddPhoneCommand))]
    [NotifyCanExecuteChangedFor(nameof(AddAddressCommand))]
    private Contact? _selectedContact;

    /// <summary>Gets or sets the email address currently selected in the detail panel.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedEmail))]
    [NotifyCanExecuteChangedFor(nameof(DeleteEmailCommand))]
    private EmailAddress? _selectedEmail;

    /// <summary>Gets or sets the phone number currently selected in the detail panel.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedPhone))]
    [NotifyCanExecuteChangedFor(nameof(DeletePhoneCommand))]
    private PhoneNumber? _selectedPhone;

    /// <summary>Gets or sets the address currently selected in the detail panel.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedAddress))]
    [NotifyCanExecuteChangedFor(nameof(DeleteAddressCommand))]
    private ContactAddress? _selectedAddress;

    // ─── Contact form ─────────────────────────────────────────────────────

    /// <summary>Gets or sets the current form mode for the contact (None / Create / Edit).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFormVisible))]
    [NotifyPropertyChangedFor(nameof(FormTitle))]
    private FormMode _formMode = FormMode.None;

    /// <summary>Gets or sets the contact name field in the create/edit form.</summary>
    [ObservableProperty] private string _formName = string.Empty;

    // ─── Email add form ───────────────────────────────────────────────────

    /// <summary>Gets or sets the email address field for adding a new email.</summary>
    [ObservableProperty] private string _newEmail     = string.Empty;

    /// <summary>Gets or sets the email type for the new email form.</summary>
    [ObservableProperty] private EmailType _newEmailType = EmailType.Personal;

    /// <summary>Gets or sets whether the new email is primary.</summary>
    [ObservableProperty] private bool _newEmailIsPrimary;

    // ─── Phone add form ───────────────────────────────────────────────────

    /// <summary>Gets or sets the phone number field for adding a new phone.</summary>
    [ObservableProperty] private string _newPhone     = string.Empty;

    /// <summary>Gets or sets the phone type for the new phone form.</summary>
    [ObservableProperty] private PhoneType _newPhoneType = PhoneType.Mobile;

    /// <summary>Gets or sets whether the new phone is primary.</summary>
    [ObservableProperty] private bool _newPhoneIsPrimary;

    // ─── Address add form ─────────────────────────────────────────────────

    /// <summary>Gets or sets the street field for adding a new address.</summary>
    [ObservableProperty] private string _newStreet     = string.Empty;

    /// <summary>Gets or sets the city field for adding a new address.</summary>
    [ObservableProperty] private string _newCity       = string.Empty;

    /// <summary>Gets or sets the state field for adding a new address.</summary>
    [ObservableProperty] private string _newState      = string.Empty;

    /// <summary>Gets or sets the postal code field for adding a new address.</summary>
    [ObservableProperty] private string _newPostalCode = string.Empty;

    /// <summary>Gets or sets the country field for adding a new address.</summary>
    [ObservableProperty] private string _newCountry    = string.Empty;

    /// <summary>Gets or sets the address type for the new address form.</summary>
    [ObservableProperty] private AddressType _newAddressType = AddressType.Home;

    // ─── Computed ─────────────────────────────────────────────────────────

    /// <summary>Gets a value indicating whether a contact is selected.</summary>
    public bool HasSelectedContact => SelectedContact is not null;

    /// <summary>Gets a value indicating whether the contact detail panel is visible.</summary>
    public bool IsDetailVisible => SelectedContact is not null && FormMode == FormMode.None;

    /// <summary>Gets a value indicating whether the contact form panel is visible.</summary>
    public bool IsFormVisible => FormMode != FormMode.None;

    /// <summary>Gets the form panel title.</summary>
    public string FormTitle => FormMode switch
    {
        FormMode.Create => "New Contact",
        FormMode.Edit   => $"Edit: {SelectedContact?.Name}",
        _               => string.Empty
    };

    /// <summary>Gets a value indicating whether an email is selected.</summary>
    public bool HasSelectedEmail => SelectedEmail is not null;

    /// <summary>Gets a value indicating whether a phone is selected.</summary>
    public bool HasSelectedPhone => SelectedPhone is not null;

    /// <summary>Gets a value indicating whether an address is selected.</summary>
    public bool HasSelectedAddress => SelectedAddress is not null;

    /// <summary>Gets available <see cref="EmailType"/> values for ComboBox binding.</summary>
    public IReadOnlyList<EmailType> EmailTypes { get; } =
        Enum.GetValues<EmailType>().ToList().AsReadOnly();

    /// <summary>Gets available <see cref="PhoneType"/> values for ComboBox binding.</summary>
    public IReadOnlyList<PhoneType> PhoneTypes { get; } =
        Enum.GetValues<PhoneType>().ToList().AsReadOnly();

    /// <summary>Gets available <see cref="AddressType"/> values for ComboBox binding.</summary>
    public IReadOnlyList<AddressType> AddressTypes { get; } =
        Enum.GetValues<AddressType>().ToList().AsReadOnly();

    // ─── Constructor ──────────────────────────────────────────────────────

    /// <summary>Initializes a new instance of <see cref="ContactsViewModel"/>.</summary>
    public ContactsViewModel(IContactService contacts, UserSession session)
    {
        _contacts = contacts;
        _session  = session;
    }

    // ─── Load ─────────────────────────────────────────────────────────────

    /// <summary>Loads the current user's contacts from the database.</summary>
    public async Task LoadAsync()
    {
        if (_session.CurrentUser is null) return;

        IsBusy = true;
        ClearMessages();
        try
        {
            var list = await _contacts.GetByOwnerUidAsync(_session.CurrentUser.Uid);
            Contacts.Clear();
            foreach (var c in list.OrderBy(x => x.Name))
                Contacts.Add(c);

            SelectedContact = null;
            SelectedEmail   = null;
            SelectedPhone   = null;
            SelectedAddress = null;
            FormMode        = FormMode.None;
        }
        catch (Exception ex)
        {
            SetError($"Failed to load contacts: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    // ─── Contact CRUD ─────────────────────────────────────────────────────

    /// <summary>Opens the form in Create mode.</summary>
    [RelayCommand]
    private void ShowCreateForm()
    {
        FormName        = string.Empty;
        SelectedContact = null;
        FormMode        = FormMode.Create;
        ClearMessages();
    }

    /// <summary>Opens the form in Edit mode for the selected contact.</summary>
    [RelayCommand(CanExecute = nameof(HasSelectedContact))]
    private void EditSelectedContact()
    {
        if (SelectedContact is null) return;
        FormName = SelectedContact.Name;
        FormMode = FormMode.Edit;
        ClearMessages();
    }

    /// <summary>Closes the form without saving.</summary>
    [RelayCommand]
    private void CancelForm()
    {
        FormMode = FormMode.None;
        OnPropertyChanged(nameof(IsDetailVisible));
        ClearMessages();
    }

    /// <summary>Saves the form (create or update).</summary>
    [RelayCommand]
    private async Task SaveForm()
    {
        if (_session.CurrentUser is null) return;
        if (string.IsNullOrWhiteSpace(FormName))
        {
            SetError("Name is required.");
            return;
        }

        IsBusy = true;
        ClearMessages();
        try
        {
            if (FormMode == FormMode.Create)
            {
                var contact = await _contacts.CreateAsync(_session.CurrentUser.Uid, FormName);
                FormMode        = FormMode.None;
                await LoadAsync();
                SelectedContact = Contacts.FirstOrDefault(c => c.Uid == contact.Uid);
                SetSuccess("Contact created.");
            }
            else if (FormMode == FormMode.Edit && SelectedContact is not null)
            {
                SelectedContact.Name = FormName;
                await _contacts.UpdateAsync(SelectedContact);
                FormMode = FormMode.None;
                await LoadAsync();
                SetSuccess("Contact saved.");
            }
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

    /// <summary>Deletes the selected contact.</summary>
    [RelayCommand(CanExecute = nameof(HasSelectedContact))]
    private async Task DeleteSelectedContact()
    {
        if (SelectedContact is null) return;

        IsBusy = true;
        ClearMessages();
        try
        {
            await _contacts.DeleteAsync(SelectedContact.Uid);
            await LoadAsync();
            SetSuccess("Contact deleted.");
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

    // ─── Email CRUD ───────────────────────────────────────────────────────

    /// <summary>Adds a new email address to the selected contact.</summary>
    [RelayCommand(CanExecute = nameof(HasSelectedContact))]
    private async Task AddEmail()
    {
        if (SelectedContact is null || string.IsNullOrWhiteSpace(NewEmail)) return;

        try
        {
            await _contacts.AddEmailAddressAsync(
                SelectedContact.Uid, NewEmail, NewEmailType, NewEmailIsPrimary);
            NewEmail          = string.Empty;
            NewEmailIsPrimary = false;
            await RefreshSelectedContactAsync();
        }
        catch (Exception ex)
        {
            SetError($"Failed to add email: {ex.Message}");
        }
    }

    /// <summary>Deletes the selected email address.</summary>
    [RelayCommand(CanExecute = nameof(HasSelectedEmail))]
    private async Task DeleteEmail()
    {
        if (SelectedEmail is null) return;
        try
        {
            await _contacts.DeleteEmailAddressAsync(SelectedEmail.Uid);
            SelectedEmail = null;
            await RefreshSelectedContactAsync();
        }
        catch (Exception ex)
        {
            SetError($"Failed to delete email: {ex.Message}");
        }
    }

    // ─── Phone CRUD ───────────────────────────────────────────────────────

    /// <summary>Adds a new phone number to the selected contact.</summary>
    [RelayCommand(CanExecute = nameof(HasSelectedContact))]
    private async Task AddPhone()
    {
        if (SelectedContact is null || string.IsNullOrWhiteSpace(NewPhone)) return;

        try
        {
            await _contacts.AddPhoneNumberAsync(
                SelectedContact.Uid, NewPhone, NewPhoneType, NewPhoneIsPrimary);
            NewPhone          = string.Empty;
            NewPhoneIsPrimary = false;
            await RefreshSelectedContactAsync();
        }
        catch (Exception ex)
        {
            SetError($"Failed to add phone: {ex.Message}");
        }
    }

    /// <summary>Deletes the selected phone number.</summary>
    [RelayCommand(CanExecute = nameof(HasSelectedPhone))]
    private async Task DeletePhone()
    {
        if (SelectedPhone is null) return;
        try
        {
            await _contacts.DeletePhoneNumberAsync(SelectedPhone.Uid);
            SelectedPhone = null;
            await RefreshSelectedContactAsync();
        }
        catch (Exception ex)
        {
            SetError($"Failed to delete phone: {ex.Message}");
        }
    }

    // ─── Address CRUD ─────────────────────────────────────────────────────

    /// <summary>Adds a new address to the selected contact.</summary>
    [RelayCommand(CanExecute = nameof(HasSelectedContact))]
    private async Task AddAddress()
    {
        if (SelectedContact is null || string.IsNullOrWhiteSpace(NewStreet)) return;

        try
        {
            var address = new ContactAddress
            {
                ContactUid = SelectedContact.Uid,
                Street     = NewStreet,
                City       = NewCity,
                State      = NewState,
                PostalCode = NewPostalCode,
                Country    = NewCountry,
                Type       = NewAddressType,
            };
            await _contacts.AddAddressAsync(SelectedContact.Uid, address);
            NewStreet     = string.Empty;
            NewCity       = string.Empty;
            NewState      = string.Empty;
            NewPostalCode = string.Empty;
            NewCountry    = string.Empty;
            await RefreshSelectedContactAsync();
        }
        catch (Exception ex)
        {
            SetError($"Failed to add address: {ex.Message}");
        }
    }

    /// <summary>Deletes the selected address.</summary>
    [RelayCommand(CanExecute = nameof(HasSelectedAddress))]
    private async Task DeleteAddress()
    {
        if (SelectedAddress is null) return;
        try
        {
            await _contacts.DeleteAddressAsync(SelectedAddress.Uid);
            SelectedAddress = null;
            await RefreshSelectedContactAsync();
        }
        catch (Exception ex)
        {
            SetError($"Failed to delete address: {ex.Message}");
        }
    }

    // ─── Helpers ──────────────────────────────────────────────────────────

    private async Task RefreshSelectedContactAsync()
    {
        if (SelectedContact is null) return;
        var uid       = SelectedContact.Uid;
        var refreshed = await _contacts.GetByUidAsync(uid);
        if (refreshed is null) return;

        var idx = Contacts.IndexOf(Contacts.FirstOrDefault(c => c.Uid == uid)!);
        if (idx >= 0) Contacts[idx] = refreshed;

        SelectedContact = refreshed;
        SelectedEmail   = null;
        SelectedPhone   = null;
        SelectedAddress = null;
    }
}
