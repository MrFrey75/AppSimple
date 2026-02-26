using AppSimple.Core.Enums;
using AppSimple.Core.Models;
using AppSimple.Core.Services;
using AppSimple.UserCLI.Session;
using AppSimple.UserCLI.UI;

namespace AppSimple.UserCLI.Menus;

/// <summary>
/// Menu for managing the logged-in user's contacts, including email addresses,
/// phone numbers, and postal addresses.
/// Admins can browse all contacts but only edit/delete their own.
/// </summary>
public class ContactsMenu
{
    private readonly IContactService _contacts;
    private readonly UserSession _session;

    /// <summary>Initializes a new instance of <see cref="ContactsMenu"/>.</summary>
    public ContactsMenu(IContactService contacts, UserSession session)
    {
        _contacts = contacts;
        _session  = session;
    }

    /// <summary>Displays the contacts menu and loops until the user selects Back.</summary>
    public async Task ShowAsync()
    {
        while (true)
        {
            ConsoleUI.Clear();
            ConsoleUI.WriteHeading("My Contacts");

            ConsoleUI.WriteMenuItem(1, "List Contacts");
            ConsoleUI.WriteMenuItem(2, "New Contact");
            ConsoleUI.WriteBackItem();
            ConsoleUI.WriteLine();

            int choice = ConsoleUI.ReadMenuChoice(2);
            switch (choice)
            {
                case 0: return;
                case 1: await ListContactsAsync(); break;
                case 2: await CreateContactAsync(); break;
            }
        }
    }

    // ─── List ────────────────────────────────────────────────────────────────

    private async Task ListContactsAsync()
    {
        var ownerUid = _session.CurrentUser!.Uid;
        var contacts = (await _contacts.GetByOwnerUidAsync(ownerUid)).ToList();

        ConsoleUI.Clear();
        ConsoleUI.WriteHeading("My Contacts");

        if (contacts.Count == 0)
        {
            ConsoleUI.WriteInfo("You have no contacts yet.");
            ConsoleUI.Pause();
            return;
        }

        ConsoleUI.WriteContactTable(contacts);
        ConsoleUI.WriteLine();
        ConsoleUI.WriteInfo("Enter a contact number to open it, or 0 to go back.");
        int choice = ConsoleUI.ReadMenuChoice(contacts.Count);
        if (choice == 0) return;
        await ContactDetailMenuAsync(contacts[choice - 1].Uid);
    }

    // ─── Create ──────────────────────────────────────────────────────────────

    private async Task CreateContactAsync()
    {
        ConsoleUI.Clear();
        ConsoleUI.WriteHeading("New Contact");

        string name = ConsoleUI.ReadLine("Name");
        ConsoleUI.WriteLine();

        try
        {
            var contact = await _contacts.CreateAsync(_session.CurrentUser!.Uid, name);
            ConsoleUI.WriteSuccess($"Contact \"{contact.Name}\" created.");
        }
        catch (Exception ex)
        {
            ConsoleUI.WriteError($"Failed to create contact: {ex.Message}");
        }

        ConsoleUI.Pause();
    }

    // ─── Contact detail / actions ─────────────────────────────────────────────

    private async Task ContactDetailMenuAsync(Guid contactUid)
    {
        while (true)
        {
            var contact = await _contacts.GetByUidAsync(contactUid);
            if (contact is null)
            {
                ConsoleUI.WriteError("Contact no longer exists.");
                ConsoleUI.Pause();
                return;
            }

            bool isOwner = contact.OwnerUserUid == _session.CurrentUser!.Uid;

            ConsoleUI.Clear();
            ConsoleUI.WriteHeading($"Contact: {contact.Name}");
            ConsoleUI.WriteContactDetail(contact);

            ConsoleUI.WriteMenuItem(1, "Edit Name");
            ConsoleUI.WriteMenuItem(2, "Email Addresses");
            ConsoleUI.WriteMenuItem(3, "Phone Numbers");
            ConsoleUI.WriteMenuItem(4, "Postal Addresses");
            ConsoleUI.WriteMenuItem(5, "Delete Contact");
            ConsoleUI.WriteBackItem();
            ConsoleUI.WriteLine();

            if (!isOwner)
                ConsoleUI.WriteWarning("View only — you cannot edit or delete this contact.");

            int choice = ConsoleUI.ReadMenuChoice(isOwner ? 5 : 0);
            switch (choice)
            {
                case 0: return;
                case 1 when isOwner: await EditContactNameAsync(contact); break;
                case 2 when isOwner: await EmailMenuAsync(contact); break;
                case 3 when isOwner: await PhoneMenuAsync(contact); break;
                case 4 when isOwner: await AddressMenuAsync(contact); break;
                case 5 when isOwner:
                    if (ConsoleUI.Confirm($"Delete contact \"{contact.Name}\"? This removes all their details."))
                    {
                        await _contacts.DeleteAsync(contact.Uid);
                        ConsoleUI.WriteSuccess("Contact deleted.");
                        ConsoleUI.Pause();
                        return;
                    }
                    break;
            }
        }
    }

    // ─── Edit name ───────────────────────────────────────────────────────────

    private async Task EditContactNameAsync(Contact contact)
    {
        ConsoleUI.Clear();
        ConsoleUI.WriteHeading("Edit Contact Name");

        string? newName = ConsoleUI.ReadOptionalLine("Name", contact.Name);
        if (!string.IsNullOrWhiteSpace(newName))
            contact.Name = newName;

        ConsoleUI.WriteLine();
        try
        {
            await _contacts.UpdateAsync(contact);
            ConsoleUI.WriteSuccess("Contact updated.");
        }
        catch (Exception ex)
        {
            ConsoleUI.WriteError($"Update failed: {ex.Message}");
        }
        ConsoleUI.Pause();
    }

    // ─── Email addresses ──────────────────────────────────────────────────────

    private async Task EmailMenuAsync(Contact contact)
    {
        while (true)
        {
            var current = await _contacts.GetByUidAsync(contact.Uid);
            if (current is null) return;

            ConsoleUI.Clear();
            ConsoleUI.WriteHeading($"Email Addresses — {current.Name}");

            if (current.EmailAddresses.Count > 0)
            {
                for (int i = 0; i < current.EmailAddresses.Count; i++)
                {
                    var e = current.EmailAddresses[i];
                    string primary = e.IsPrimary ? " ★" : "";
                    ConsoleUI.WriteMenuItem(i + 1, $"{e.Email}{primary}", $"{e.Type}");
                }
                ConsoleUI.WriteMenuGroupLabel("Actions");
                ConsoleUI.WriteMenuItem(current.EmailAddresses.Count + 1, "Add Email Address");
                ConsoleUI.WriteBackItem();
                ConsoleUI.WriteLine();

                int choice = ConsoleUI.ReadMenuChoice(current.EmailAddresses.Count + 1);
                if (choice == 0) return;
                if (choice <= current.EmailAddresses.Count)
                    await EmailDetailMenuAsync(current.EmailAddresses[choice - 1]);
                else
                    await AddEmailAsync(current.Uid);
            }
            else
            {
                ConsoleUI.WriteInfo("No email addresses.");
                ConsoleUI.WriteMenuItem(1, "Add Email Address");
                ConsoleUI.WriteBackItem();
                ConsoleUI.WriteLine();

                int choice = ConsoleUI.ReadMenuChoice(1);
                if (choice == 0) return;
                await AddEmailAsync(current.Uid);
            }
        }
    }

    private async Task EmailDetailMenuAsync(EmailAddress email)
    {
        ConsoleUI.Clear();
        ConsoleUI.WriteHeading("Email Address");

        ConsoleUI.WriteMenuItem(1, "Edit");
        ConsoleUI.WriteMenuItem(2, "Delete");
        ConsoleUI.WriteBackItem();
        ConsoleUI.WriteLine();

        int choice = ConsoleUI.ReadMenuChoice(2);
        switch (choice)
        {
            case 0: return;
            case 1: await EditEmailAsync(email); break;
            case 2:
                if (ConsoleUI.Confirm($"Delete {email.Email}?"))
                {
                    await _contacts.DeleteEmailAddressAsync(email.Uid);
                    ConsoleUI.WriteSuccess("Email address deleted.");
                    ConsoleUI.Pause();
                }
                break;
        }
    }

    private async Task AddEmailAsync(Guid contactUid)
    {
        ConsoleUI.Clear();
        ConsoleUI.WriteHeading("Add Email Address");

        string emailStr = ConsoleUI.ReadLine("Email");
        EmailType type  = ReadEmailType();
        bool isPrimary  = ConsoleUI.Confirm("Set as primary?");
        ConsoleUI.WriteLine();

        try
        {
            await _contacts.AddEmailAddressAsync(contactUid, emailStr, type, isPrimary);
            ConsoleUI.WriteSuccess("Email address added.");
        }
        catch (Exception ex)
        {
            ConsoleUI.WriteError($"Failed: {ex.Message}");
        }
        ConsoleUI.Pause();
    }

    private async Task EditEmailAsync(EmailAddress email)
    {
        ConsoleUI.Clear();
        ConsoleUI.WriteHeading("Edit Email Address");
        ConsoleUI.WriteInfo("Press Enter to keep the current value.");
        ConsoleUI.WriteLine();

        string? newEmail = ConsoleUI.ReadOptionalLine("Email", email.Email);
        if (!string.IsNullOrWhiteSpace(newEmail)) email.Email = newEmail;
        email.Type      = ReadEmailType(email.Type);
        email.IsPrimary = ConsoleUI.Confirm("Set as primary?");
        ConsoleUI.WriteLine();

        try
        {
            await _contacts.UpdateEmailAddressAsync(email);
            ConsoleUI.WriteSuccess("Email address updated.");
        }
        catch (Exception ex)
        {
            ConsoleUI.WriteError($"Failed: {ex.Message}");
        }
        ConsoleUI.Pause();
    }

    // ─── Phone numbers ───────────────────────────────────────────────────────

    private async Task PhoneMenuAsync(Contact contact)
    {
        while (true)
        {
            var current = await _contacts.GetByUidAsync(contact.Uid);
            if (current is null) return;

            ConsoleUI.Clear();
            ConsoleUI.WriteHeading($"Phone Numbers — {current.Name}");

            if (current.PhoneNumbers.Count > 0)
            {
                for (int i = 0; i < current.PhoneNumbers.Count; i++)
                {
                    var p = current.PhoneNumbers[i];
                    string primary = p.IsPrimary ? " ★" : "";
                    ConsoleUI.WriteMenuItem(i + 1, $"{p.Number}{primary}", $"{p.Type}");
                }
                ConsoleUI.WriteMenuGroupLabel("Actions");
                ConsoleUI.WriteMenuItem(current.PhoneNumbers.Count + 1, "Add Phone Number");
                ConsoleUI.WriteBackItem();
                ConsoleUI.WriteLine();

                int choice = ConsoleUI.ReadMenuChoice(current.PhoneNumbers.Count + 1);
                if (choice == 0) return;
                if (choice <= current.PhoneNumbers.Count)
                    await PhoneDetailMenuAsync(current.PhoneNumbers[choice - 1]);
                else
                    await AddPhoneAsync(current.Uid);
            }
            else
            {
                ConsoleUI.WriteInfo("No phone numbers.");
                ConsoleUI.WriteMenuItem(1, "Add Phone Number");
                ConsoleUI.WriteBackItem();
                ConsoleUI.WriteLine();

                int choice = ConsoleUI.ReadMenuChoice(1);
                if (choice == 0) return;
                await AddPhoneAsync(current.Uid);
            }
        }
    }

    private async Task PhoneDetailMenuAsync(PhoneNumber phone)
    {
        ConsoleUI.Clear();
        ConsoleUI.WriteHeading("Phone Number");

        ConsoleUI.WriteMenuItem(1, "Edit");
        ConsoleUI.WriteMenuItem(2, "Delete");
        ConsoleUI.WriteBackItem();
        ConsoleUI.WriteLine();

        int choice = ConsoleUI.ReadMenuChoice(2);
        switch (choice)
        {
            case 0: return;
            case 1: await EditPhoneAsync(phone); break;
            case 2:
                if (ConsoleUI.Confirm($"Delete {phone.Number}?"))
                {
                    await _contacts.DeletePhoneNumberAsync(phone.Uid);
                    ConsoleUI.WriteSuccess("Phone number deleted.");
                    ConsoleUI.Pause();
                }
                break;
        }
    }

    private async Task AddPhoneAsync(Guid contactUid)
    {
        ConsoleUI.Clear();
        ConsoleUI.WriteHeading("Add Phone Number");

        string number  = ConsoleUI.ReadLine("Number");
        PhoneType type = ReadPhoneType();
        bool isPrimary = ConsoleUI.Confirm("Set as primary?");
        ConsoleUI.WriteLine();

        try
        {
            await _contacts.AddPhoneNumberAsync(contactUid, number, type, isPrimary);
            ConsoleUI.WriteSuccess("Phone number added.");
        }
        catch (Exception ex)
        {
            ConsoleUI.WriteError($"Failed: {ex.Message}");
        }
        ConsoleUI.Pause();
    }

    private async Task EditPhoneAsync(PhoneNumber phone)
    {
        ConsoleUI.Clear();
        ConsoleUI.WriteHeading("Edit Phone Number");
        ConsoleUI.WriteInfo("Press Enter to keep the current value.");
        ConsoleUI.WriteLine();

        string? newNumber = ConsoleUI.ReadOptionalLine("Number", phone.Number);
        if (!string.IsNullOrWhiteSpace(newNumber)) phone.Number = newNumber;
        phone.Type      = ReadPhoneType(phone.Type);
        phone.IsPrimary = ConsoleUI.Confirm("Set as primary?");
        ConsoleUI.WriteLine();

        try
        {
            await _contacts.UpdatePhoneNumberAsync(phone);
            ConsoleUI.WriteSuccess("Phone number updated.");
        }
        catch (Exception ex)
        {
            ConsoleUI.WriteError($"Failed: {ex.Message}");
        }
        ConsoleUI.Pause();
    }

    // ─── Postal addresses ─────────────────────────────────────────────────────

    private async Task AddressMenuAsync(Contact contact)
    {
        while (true)
        {
            var current = await _contacts.GetByUidAsync(contact.Uid);
            if (current is null) return;

            ConsoleUI.Clear();
            ConsoleUI.WriteHeading($"Addresses — {current.Name}");

            if (current.Addresses.Count > 0)
            {
                for (int i = 0; i < current.Addresses.Count; i++)
                {
                    var a = current.Addresses[i];
                    string primary = a.IsPrimary ? " ★" : "";
                    ConsoleUI.WriteMenuItem(i + 1, $"{a.Street}, {a.City}{primary}", $"{a.Type}");
                }
                ConsoleUI.WriteMenuGroupLabel("Actions");
                ConsoleUI.WriteMenuItem(current.Addresses.Count + 1, "Add Address");
                ConsoleUI.WriteBackItem();
                ConsoleUI.WriteLine();

                int choice = ConsoleUI.ReadMenuChoice(current.Addresses.Count + 1);
                if (choice == 0) return;
                if (choice <= current.Addresses.Count)
                    await AddressDetailMenuAsync(current.Addresses[choice - 1]);
                else
                    await AddAddressAsync(current.Uid);
            }
            else
            {
                ConsoleUI.WriteInfo("No addresses.");
                ConsoleUI.WriteMenuItem(1, "Add Address");
                ConsoleUI.WriteBackItem();
                ConsoleUI.WriteLine();

                int choice = ConsoleUI.ReadMenuChoice(1);
                if (choice == 0) return;
                await AddAddressAsync(current.Uid);
            }
        }
    }

    private async Task AddressDetailMenuAsync(ContactAddress address)
    {
        ConsoleUI.Clear();
        ConsoleUI.WriteHeading("Address");

        ConsoleUI.WriteMenuItem(1, "Edit");
        ConsoleUI.WriteMenuItem(2, "Delete");
        ConsoleUI.WriteBackItem();
        ConsoleUI.WriteLine();

        int choice = ConsoleUI.ReadMenuChoice(2);
        switch (choice)
        {
            case 0: return;
            case 1: await EditAddressAsync(address); break;
            case 2:
                if (ConsoleUI.Confirm($"Delete address at {address.Street}?"))
                {
                    await _contacts.DeleteAddressAsync(address.Uid);
                    ConsoleUI.WriteSuccess("Address deleted.");
                    ConsoleUI.Pause();
                }
                break;
        }
    }

    private async Task AddAddressAsync(Guid contactUid)
    {
        ConsoleUI.Clear();
        ConsoleUI.WriteHeading("Add Address");

        var address = new ContactAddress
        {
            Uid        = Guid.CreateVersion7(),
            ContactUid = contactUid,
            Street     = ConsoleUI.ReadLine("Street"),
            City       = ConsoleUI.ReadLine("City"),
            State      = ConsoleUI.ReadOptionalLine("State / Province") ?? string.Empty,
            PostalCode = ConsoleUI.ReadOptionalLine("Postal / ZIP Code") ?? string.Empty,
            Country    = ConsoleUI.ReadLine("Country"),
            Type       = ReadAddressType(),
            IsPrimary  = ConsoleUI.Confirm("Set as primary?"),
            CreatedAt  = DateTime.UtcNow,
            UpdatedAt  = DateTime.UtcNow,
        };

        ConsoleUI.WriteLine();
        try
        {
            await _contacts.AddAddressAsync(contactUid, address);
            ConsoleUI.WriteSuccess("Address added.");
        }
        catch (Exception ex)
        {
            ConsoleUI.WriteError($"Failed: {ex.Message}");
        }
        ConsoleUI.Pause();
    }

    private async Task EditAddressAsync(ContactAddress address)
    {
        ConsoleUI.Clear();
        ConsoleUI.WriteHeading("Edit Address");
        ConsoleUI.WriteInfo("Press Enter to keep the current value.");
        ConsoleUI.WriteLine();

        address.Street     = ConsoleUI.ReadOptionalLine("Street",             address.Street)     ?? address.Street;
        address.City       = ConsoleUI.ReadOptionalLine("City",               address.City)       ?? address.City;
        address.State      = ConsoleUI.ReadOptionalLine("State / Province",   address.State)      ?? address.State;
        address.PostalCode = ConsoleUI.ReadOptionalLine("Postal / ZIP Code",  address.PostalCode) ?? address.PostalCode;
        address.Country    = ConsoleUI.ReadOptionalLine("Country",            address.Country)    ?? address.Country;
        address.Type       = ReadAddressType(address.Type);
        address.IsPrimary  = ConsoleUI.Confirm("Set as primary?");
        ConsoleUI.WriteLine();

        try
        {
            await _contacts.UpdateAddressAsync(address);
            ConsoleUI.WriteSuccess("Address updated.");
        }
        catch (Exception ex)
        {
            ConsoleUI.WriteError($"Failed: {ex.Message}");
        }
        ConsoleUI.Pause();
    }

    // ─── Enum pickers ─────────────────────────────────────────────────────────

    private static EmailType ReadEmailType(EmailType current = EmailType.Personal)
    {
        ConsoleUI.WriteInfo($"Email type: [1] Personal  [2] Work  [3] Other  (current: {current})");
        Console.Write("  Select: ");
        string? input = Console.ReadLine()?.Trim();
        return input switch
        {
            "1" => EmailType.Personal,
            "2" => EmailType.Work,
            "3" => EmailType.Other,
            _   => current,
        };
    }

    private static PhoneType ReadPhoneType(PhoneType current = PhoneType.Mobile)
    {
        ConsoleUI.WriteInfo($"Phone type: [1] Mobile  [2] Home  [3] Work  [4] Other  (current: {current})");
        Console.Write("  Select: ");
        string? input = Console.ReadLine()?.Trim();
        return input switch
        {
            "1" => PhoneType.Mobile,
            "2" => PhoneType.Home,
            "3" => PhoneType.Work,
            "4" => PhoneType.Other,
            _   => current,
        };
    }

    private static AddressType ReadAddressType(AddressType current = AddressType.Home)
    {
        ConsoleUI.WriteInfo($"Address type: [1] Home  [2] Work  [3] Other  (current: {current})");
        Console.Write("  Select: ");
        string? input = Console.ReadLine()?.Trim();
        return input switch
        {
            "1" => AddressType.Home,
            "2" => AddressType.Work,
            "3" => AddressType.Other,
            _   => current,
        };
    }
}
