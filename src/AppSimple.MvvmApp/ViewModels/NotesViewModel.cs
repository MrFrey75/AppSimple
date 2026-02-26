using System.Collections.ObjectModel;
using AppSimple.Core.Models;
using AppSimple.Core.Services;
using AppSimple.MvvmApp.Session;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AppSimple.MvvmApp.ViewModels;

/// <summary>
/// ViewModel for the Notes page. Lists the current user's notes with an inline
/// create/edit panel and basic tag management.
/// </summary>
public partial class NotesViewModel : BaseViewModel
{
    private readonly INoteService _notes;
    private readonly ITagService  _tags;
    private readonly UserSession  _session;

    // ─── Collections ──────────────────────────────────────────────────────

    /// <summary>Gets the live collection of notes shown in the list.</summary>
    public ObservableCollection<Note> Notes { get; } = new();

    /// <summary>Gets the live collection of all tags owned by the current user.</summary>
    public ObservableCollection<Tag> AllTags { get; } = new();

    /// <summary>Gets the tags attached to the currently selected note.</summary>
    public ObservableCollection<Tag> SelectedNoteTags { get; } = new();

    // ─── Selection ────────────────────────────────────────────────────────

    /// <summary>Gets or sets the note currently selected in the list.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedNote))]
    [NotifyPropertyChangedFor(nameof(IsDetailVisible))]
    [NotifyPropertyChangedFor(nameof(SelectedNoteTagsText))]
    [NotifyCanExecuteChangedFor(nameof(EditSelectedNoteCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteSelectedNoteCommand))]
    [NotifyCanExecuteChangedFor(nameof(AddTagToNoteCommand))]
    private Note? _selectedNote;

    partial void OnSelectedNoteChanged(Note? value)
    {
        SelectedNoteTags.Clear();
        if (value is not null)
            foreach (var t in value.Tags)
                SelectedNoteTags.Add(t);
    }

    /// <summary>Gets or sets the tag currently selected in the "remove tag" list.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedNoteTag))]
    [NotifyCanExecuteChangedFor(nameof(RemoveTagFromNoteCommand))]
    private Tag? _selectedNoteTag;

    /// <summary>Gets or sets the tag selected in the "add tag" combo box.</summary>
    [ObservableProperty] private Tag? _tagToAdd;

    // ─── Form ─────────────────────────────────────────────────────────────

    /// <summary>Gets or sets the current form mode (None / Create / Edit).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFormVisible))]
    [NotifyPropertyChangedFor(nameof(IsDetailVisible))]
    [NotifyPropertyChangedFor(nameof(FormTitle))]
    private FormMode _formMode = FormMode.None;

    /// <summary>Gets or sets the note title in the create/edit form.</summary>
    [ObservableProperty] private string _noteTitle   = string.Empty;

    /// <summary>Gets or sets the note content in the create/edit form.</summary>
    [ObservableProperty] private string _noteContent = string.Empty;

    // ─── Computed ─────────────────────────────────────────────────────────

    /// <summary>Gets a value indicating whether a note is selected.</summary>
    public bool HasSelectedNote => SelectedNote is not null;

    /// <summary>Gets a value indicating whether the form panel should be visible.</summary>
    public bool IsFormVisible => FormMode != FormMode.None;

    /// <summary>Gets a value indicating whether the note detail panel should be visible.</summary>
    public bool IsDetailVisible => SelectedNote is not null && FormMode == FormMode.None;

    /// <summary>Gets the form panel title.</summary>
    public string FormTitle => FormMode switch
    {
        FormMode.Create => "New Note",
        FormMode.Edit   => "Edit Note",
        _               => string.Empty
    };

    /// <summary>Gets the tag names of the selected note as a comma-separated string.</summary>
    public string SelectedNoteTagsText => SelectedNote is null
        ? string.Empty
        : string.Join(", ", SelectedNote.Tags.Select(t => t.Name));

    /// <summary>Gets a value indicating whether the selected note has a selected tag.</summary>
    public bool HasSelectedNoteTag => SelectedNoteTag is not null;

    // ─── Constructor ──────────────────────────────────────────────────────

    /// <summary>Initializes a new instance of <see cref="NotesViewModel"/>.</summary>
    public NotesViewModel(INoteService notes, ITagService tags, UserSession session)
    {
        _notes   = notes;
        _tags    = tags;
        _session = session;
    }

    // ─── Load ─────────────────────────────────────────────────────────────

    /// <summary>Loads the current user's notes and tags from the database.</summary>
    public async Task LoadAsync()
    {
        if (_session.CurrentUser is null) return;

        IsBusy = true;
        ClearMessages();
        try
        {
            var uid = _session.CurrentUser.Uid;

            var noteList = await _notes.GetByUserUidAsync(uid);
            Notes.Clear();
            foreach (var n in noteList.OrderByDescending(x => x.UpdatedAt))
                Notes.Add(n);

            var tagList = await _tags.GetByUserUidAsync(uid);
            AllTags.Clear();
            foreach (var t in tagList)
                AllTags.Add(t);

            SelectedNote = null;
            FormMode     = FormMode.None;
        }
        catch (Exception ex)
        {
            SetError($"Failed to load notes: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    // ─── Note CRUD ────────────────────────────────────────────────────────

    /// <summary>Opens the form in Create mode.</summary>
    [RelayCommand]
    private void ShowCreateForm()
    {
        NoteTitle   = string.Empty;
        NoteContent = string.Empty;
        SelectedNote = null;
        FormMode     = FormMode.Create;
        ClearMessages();
    }

    /// <summary>Opens the form in Edit mode for the selected note.</summary>
    [RelayCommand(CanExecute = nameof(HasSelectedNote))]
    private void EditSelectedNote()
    {
        if (SelectedNote is null) return;
        NoteTitle   = SelectedNote.Title;
        NoteContent = SelectedNote.Content;
        FormMode    = FormMode.Edit;
        ClearMessages();
    }

    /// <summary>Closes the form without saving.</summary>
    [RelayCommand]
    private void CancelForm()
    {
        FormMode = FormMode.None;
        ClearMessages();
    }

    /// <summary>Saves the form (create or update).</summary>
    [RelayCommand]
    private async Task SaveForm()
    {
        if (_session.CurrentUser is null) return;
        if (string.IsNullOrWhiteSpace(NoteContent))
        {
            SetError("Content is required.");
            return;
        }

        IsBusy = true;
        ClearMessages();
        try
        {
            if (FormMode == FormMode.Create)
            {
                await _notes.CreateAsync(_session.CurrentUser.Uid, NoteTitle, NoteContent);
                SetSuccess("Note created.");
            }
            else if (FormMode == FormMode.Edit && SelectedNote is not null)
            {
                SelectedNote.Title   = NoteTitle;
                SelectedNote.Content = NoteContent;
                await _notes.UpdateAsync(SelectedNote);
                SetSuccess("Note saved.");
            }

            FormMode = FormMode.None;
            await LoadAsync();
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

    /// <summary>Deletes the selected note.</summary>
    [RelayCommand(CanExecute = nameof(HasSelectedNote))]
    private async Task DeleteSelectedNote()
    {
        if (SelectedNote is null) return;

        IsBusy = true;
        ClearMessages();
        try
        {
            await _notes.DeleteAsync(SelectedNote.Uid);
            SetSuccess("Note deleted.");
            await LoadAsync();
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

    // ─── Tag management ───────────────────────────────────────────────────

    /// <summary>Adds the selected tag (<see cref="TagToAdd"/>) to the selected note.</summary>
    [RelayCommand(CanExecute = nameof(HasSelectedNote))]
    private async Task AddTagToNote()
    {
        if (SelectedNote is null || TagToAdd is null) return;

        try
        {
            await _notes.AddTagAsync(SelectedNote.Uid, TagToAdd.Uid);
            // Reload to get updated tag list
            var refreshed = await _notes.GetByUidAsync(SelectedNote.Uid);
            if (refreshed is not null)
            {
                SelectedNote = null;
                var idx = Notes.IndexOf(Notes.FirstOrDefault(n => n.Uid == refreshed.Uid)!);
                if (idx >= 0) Notes[idx] = refreshed;
                else Notes.Insert(0, refreshed);
                SelectedNote = refreshed;
            }
            TagToAdd = null;
        }
        catch (Exception ex)
        {
            SetError($"Failed to add tag: {ex.Message}");
        }
    }

    /// <summary>Removes the selected tag (<see cref="SelectedNoteTag"/>) from the selected note.</summary>
    [RelayCommand(CanExecute = nameof(HasSelectedNoteTag))]
    private async Task RemoveTagFromNote()
    {
        if (SelectedNote is null || SelectedNoteTag is null) return;

        try
        {
            await _notes.RemoveTagAsync(SelectedNote.Uid, SelectedNoteTag.Uid);
            var refreshed = await _notes.GetByUidAsync(SelectedNote.Uid);
            if (refreshed is not null)
            {
                var idx = Notes.IndexOf(Notes.FirstOrDefault(n => n.Uid == refreshed.Uid)!);
                if (idx >= 0) Notes[idx] = refreshed;
                SelectedNote    = refreshed;
                SelectedNoteTag = null;
            }
        }
        catch (Exception ex)
        {
            SetError($"Failed to remove tag: {ex.Message}");
        }
    }
}
