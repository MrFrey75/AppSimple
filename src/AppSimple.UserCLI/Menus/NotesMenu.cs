using AppSimple.Core.Models;
using AppSimple.Core.Services;
using AppSimple.UserCLI.Session;
using AppSimple.UserCLI.UI;

namespace AppSimple.UserCLI.Menus;

/// <summary>
/// Menu for managing the logged-in user's notes, including tag management.
/// Admins can browse all notes but only edit/delete their own.
/// </summary>
public class NotesMenu
{
    private readonly INoteService _notes;
    private readonly ITagService _tags;
    private readonly UserSession _session;

    /// <summary>Initializes a new instance of <see cref="NotesMenu"/>.</summary>
    public NotesMenu(INoteService notes, ITagService tags, UserSession session)
    {
        _notes   = notes;
        _tags    = tags;
        _session = session;
    }

    /// <summary>Displays the notes menu and loops until the user selects Back.</summary>
    public async Task ShowAsync()
    {
        while (true)
        {
            ConsoleUI.Clear();
            ConsoleUI.WriteHeading("My Notes");

            ConsoleUI.WriteMenuItem(1, "List Notes");
            ConsoleUI.WriteMenuItem(2, "New Note");
            ConsoleUI.WriteMenuItem(3, "My Tags");
            ConsoleUI.WriteBackItem();
            ConsoleUI.WriteLine();

            int choice = ConsoleUI.ReadMenuChoice(3);
            switch (choice)
            {
                case 0: return;
                case 1: await ListNotesAsync(); break;
                case 2: await CreateNoteAsync(); break;
                case 3: await TagsMenuAsync(); break;
            }
        }
    }

    // ─── List ────────────────────────────────────────────────────────────────

    private async Task ListNotesAsync()
    {
        var userUid = _session.CurrentUser!.Uid;
        var notes = (await _notes.GetByUserUidAsync(userUid)).ToList();

        ConsoleUI.Clear();
        ConsoleUI.WriteHeading("My Notes");

        if (notes.Count == 0)
        {
            ConsoleUI.WriteInfo("You have no notes yet.");
            ConsoleUI.Pause();
            return;
        }

        ConsoleUI.WriteNoteTable(notes);
        ConsoleUI.WriteLine();
        ConsoleUI.WriteInfo("Enter a note number to open it, or 0 to go back.");
        int choice = ConsoleUI.ReadMenuChoice(notes.Count);

        if (choice == 0) return;
        await NoteDetailMenuAsync(notes[choice - 1]);
    }

    // ─── Create ──────────────────────────────────────────────────────────────

    private async Task CreateNoteAsync()
    {
        ConsoleUI.Clear();
        ConsoleUI.WriteHeading("New Note");

        string title   = ConsoleUI.ReadOptionalLine("Title") ?? string.Empty;
        string content = ConsoleUI.ReadLine("Content");
        ConsoleUI.WriteLine();

        try
        {
            var note = await _notes.CreateAsync(_session.CurrentUser!.Uid, title, content);
            ConsoleUI.WriteSuccess($"Note created: \"{note.Title}\"");
        }
        catch (Exception ex)
        {
            ConsoleUI.WriteError($"Failed to create note: {ex.Message}");
        }

        ConsoleUI.Pause();
    }

    // ─── Note detail / actions ───────────────────────────────────────────────

    private async Task NoteDetailMenuAsync(Note note)
    {
        while (true)
        {
            // Reload so tags are up-to-date
            var current = await _notes.GetByUidAsync(note.Uid);
            if (current is null)
            {
                ConsoleUI.WriteError("Note no longer exists.");
                ConsoleUI.Pause();
                return;
            }

            bool isOwner = current.UserUid == _session.CurrentUser!.Uid;

            ConsoleUI.Clear();
            ConsoleUI.WriteHeading($"Note: {(string.IsNullOrEmpty(current.Title) ? "(untitled)" : current.Title)}");
            ConsoleUI.WriteNoteDetail(current);

            ConsoleUI.WriteMenuItem(1, "Edit Note");
            ConsoleUI.WriteMenuItem(2, "Manage Tags");
            ConsoleUI.WriteMenuItem(3, "Delete Note");
            ConsoleUI.WriteBackItem();
            ConsoleUI.WriteLine();

            if (!isOwner)
                ConsoleUI.WriteWarning("You can view this note but cannot edit or delete it (not your note).");

            int choice = ConsoleUI.ReadMenuChoice(isOwner ? 3 : 0);
            switch (choice)
            {
                case 0: return;
                case 1 when isOwner: await EditNoteAsync(current); break;
                case 2 when isOwner: await ManageNoteTagsAsync(current); break;
                case 3 when isOwner:
                    if (ConsoleUI.Confirm("Delete this note?"))
                    {
                        await _notes.DeleteAsync(current.Uid);
                        ConsoleUI.WriteSuccess("Note deleted.");
                        ConsoleUI.Pause();
                        return;
                    }
                    break;
            }
        }
    }

    // ─── Edit ────────────────────────────────────────────────────────────────

    private async Task EditNoteAsync(Note note)
    {
        ConsoleUI.Clear();
        ConsoleUI.WriteHeading("Edit Note");
        ConsoleUI.WriteInfo("Press Enter to keep the current value.");
        ConsoleUI.WriteLine();

        note.Title   = ConsoleUI.ReadOptionalLine("Title",   note.Title)   ?? string.Empty;
        note.Content = ConsoleUI.ReadOptionalLine("Content", note.Content) ?? note.Content;
        ConsoleUI.WriteLine();

        try
        {
            await _notes.UpdateAsync(note);
            ConsoleUI.WriteSuccess("Note updated.");
        }
        catch (Exception ex)
        {
            ConsoleUI.WriteError($"Update failed: {ex.Message}");
        }

        ConsoleUI.Pause();
    }

    // ─── Tag management ──────────────────────────────────────────────────────

    private async Task ManageNoteTagsAsync(Note note)
    {
        while (true)
        {
            var current = await _notes.GetByUidAsync(note.Uid);
            if (current is null) return;

            ConsoleUI.Clear();
            ConsoleUI.WriteHeading($"Tags for: {(string.IsNullOrEmpty(current.Title) ? "(untitled)" : current.Title)}");

            if (current.Tags.Count > 0)
            {
                ConsoleUI.WriteInfo("Current tags:");
                for (int i = 0; i < current.Tags.Count; i++)
                    ConsoleUI.WriteMenuItem(i + 1, current.Tags[i].Name, $"remove  [{current.Tags[i].Color}]");

                ConsoleUI.WriteMenuGroupLabel("Actions");
                ConsoleUI.WriteMenuItem(current.Tags.Count + 1, "Add Tag");
                ConsoleUI.WriteBackItem();
                ConsoleUI.WriteLine();

                int choice = ConsoleUI.ReadMenuChoice(current.Tags.Count + 1);
                if (choice == 0) return;
                if (choice <= current.Tags.Count)
                {
                    var tagToRemove = current.Tags[choice - 1];
                    if (ConsoleUI.Confirm($"Remove tag \"{tagToRemove.Name}\"?"))
                    {
                        await _notes.RemoveTagAsync(current.Uid, tagToRemove.Uid);
                        ConsoleUI.WriteSuccess("Tag removed.");
                        ConsoleUI.Pause();
                    }
                }
                else
                {
                    await PickAndAddTagAsync(current);
                }
            }
            else
            {
                ConsoleUI.WriteInfo("No tags on this note.");
                ConsoleUI.WriteMenuItem(1, "Add Tag");
                ConsoleUI.WriteBackItem();
                ConsoleUI.WriteLine();

                int choice = ConsoleUI.ReadMenuChoice(1);
                if (choice == 0) return;
                await PickAndAddTagAsync(current);
            }
        }
    }

    private async Task PickAndAddTagAsync(Note note)
    {
        var allTags = (await _tags.GetByUserUidAsync(_session.CurrentUser!.Uid)).ToList();
        var existingUids = note.Tags.Select(t => t.Uid).ToHashSet();
        var available = allTags.Where(t => !existingUids.Contains(t.Uid)).ToList();

        if (available.Count == 0)
        {
            ConsoleUI.WriteInfo("No available tags. Create a tag first from My Tags.");
            ConsoleUI.Pause();
            return;
        }

        ConsoleUI.Clear();
        ConsoleUI.WriteHeading("Select Tag to Add");
        for (int i = 0; i < available.Count; i++)
            ConsoleUI.WriteMenuItem(i + 1, available[i].Name, available[i].Description);

        ConsoleUI.WriteBackItem();
        ConsoleUI.WriteLine();
        int choice = ConsoleUI.ReadMenuChoice(available.Count);
        if (choice == 0) return;

        await _notes.AddTagAsync(note.Uid, available[choice - 1].Uid);
        ConsoleUI.WriteSuccess("Tag added.");
        ConsoleUI.Pause();
    }

    // ─── Tags CRUD ───────────────────────────────────────────────────────────

    private async Task TagsMenuAsync()
    {
        while (true)
        {
            ConsoleUI.Clear();
            ConsoleUI.WriteHeading("My Tags");

            ConsoleUI.WriteMenuItem(1, "List Tags");
            ConsoleUI.WriteMenuItem(2, "New Tag");
            ConsoleUI.WriteBackItem();
            ConsoleUI.WriteLine();

            int choice = ConsoleUI.ReadMenuChoice(2);
            switch (choice)
            {
                case 0: return;
                case 1: await ListTagsAsync(); break;
                case 2: await CreateTagAsync(); break;
            }
        }
    }

    private async Task ListTagsAsync()
    {
        var tags = (await _tags.GetByUserUidAsync(_session.CurrentUser!.Uid)).ToList();

        ConsoleUI.Clear();
        ConsoleUI.WriteHeading("My Tags");

        if (tags.Count == 0)
        {
            ConsoleUI.WriteInfo("You have no tags yet.");
            ConsoleUI.Pause();
            return;
        }

        ConsoleUI.WriteTagTable(tags);
        ConsoleUI.WriteLine();
        ConsoleUI.WriteInfo("Enter a tag number to edit/delete, or 0 to go back.");
        int choice = ConsoleUI.ReadMenuChoice(tags.Count);
        if (choice == 0) return;
        await TagDetailMenuAsync(tags[choice - 1]);
    }

    private async Task TagDetailMenuAsync(Tag tag)
    {
        ConsoleUI.Clear();
        ConsoleUI.WriteHeading($"Tag: {tag.Name}");

        ConsoleUI.WriteMenuItem(1, "Edit Tag");
        ConsoleUI.WriteMenuItem(2, "Delete Tag");
        ConsoleUI.WriteBackItem();
        ConsoleUI.WriteLine();

        int choice = ConsoleUI.ReadMenuChoice(2);
        switch (choice)
        {
            case 0: return;
            case 1: await EditTagAsync(tag); break;
            case 2:
                if (ConsoleUI.Confirm($"Delete tag \"{tag.Name}\"? This will remove it from all notes."))
                {
                    await _tags.DeleteAsync(tag.Uid);
                    ConsoleUI.WriteSuccess("Tag deleted.");
                    ConsoleUI.Pause();
                }
                break;
        }
    }

    private async Task CreateTagAsync()
    {
        ConsoleUI.Clear();
        ConsoleUI.WriteHeading("New Tag");

        string name        = ConsoleUI.ReadLine("Name");
        string? description = ConsoleUI.ReadOptionalLine("Description");
        string? color      = ConsoleUI.ReadOptionalLine("Color (hex, e.g. #FF5500)", "#CCCCCC");
        ConsoleUI.WriteLine();

        try
        {
            await _tags.CreateAsync(_session.CurrentUser!.Uid, name, description, color);
            ConsoleUI.WriteSuccess($"Tag \"{name}\" created.");
        }
        catch (Exception ex)
        {
            ConsoleUI.WriteError($"Failed to create tag: {ex.Message}");
        }

        ConsoleUI.Pause();
    }

    private async Task EditTagAsync(Tag tag)
    {
        ConsoleUI.Clear();
        ConsoleUI.WriteHeading("Edit Tag");
        ConsoleUI.WriteInfo("Press Enter to keep the current value.");
        ConsoleUI.WriteLine();

        tag.Name        = ConsoleUI.ReadOptionalLine("Name",        tag.Name)        ?? tag.Name;
        tag.Description = ConsoleUI.ReadOptionalLine("Description", tag.Description) ?? tag.Description;
        tag.Color       = ConsoleUI.ReadOptionalLine("Color",       tag.Color)       ?? tag.Color;
        ConsoleUI.WriteLine();

        try
        {
            await _tags.UpdateAsync(tag);
            ConsoleUI.WriteSuccess("Tag updated.");
        }
        catch (Exception ex)
        {
            ConsoleUI.WriteError($"Update failed: {ex.Message}");
        }

        ConsoleUI.Pause();
    }
}
